# Motion 模块设计文档（com.abilitykit.combat.motion）

> 本文描述 `com.abilitykit.combat.motion` 的目标、核心抽象、tick 数据流、碰撞求解、快照恢复和对象池设计。

## 1. 目标与定位

Motion 模块负责把战斗中的多种移动来源统一组合为一个最终位移，并交给碰撞/约束层求解。

核心目标：

- 支持多来源移动：输入、技能位移、击退、路径跟随、控制效果。
- 支持分组、优先级、叠加和跨组抑制。
- 支持可插拔碰撞世界和运动约束。
- 支持预测、回滚和重放需要的快照/恢复。
- 高频路径尽量复用对象和集合，降低 GC 压力。
- 保持纯逻辑层可测试，不强绑定 Unity 组件生命周期。

## 2. 目录结构与核心类型

```text
com.abilitykit.combat.motion/
├── Runtime/
│   └── MotionSystem/
│       ├── Core/
│       │   ├── IMotionSource.cs
│       │   ├── IMotionSnapshotSource.cs
│       │   ├── MotionSourceSnapshot.cs
│       │   ├── MotionPipeline.cs
│       │   ├── MotionPipelinePool.cs
│       │   ├── MotionPipelinePolicy.cs
│       │   ├── MotionGroups.cs
│       │   ├── MotionStacking.cs
│       │   ├── MotionState.cs
│       │   └── MotionOutput.cs
│       ├── Collision/
│       │   ├── IMotionSolver.cs
│       │   ├── IMotionSolverDiagnostics.cs
│       │   ├── IMotionCollisionWorld.cs
│       │   ├── ConfigurableMotionSolver.cs
│       │   └── NoMotionSolver.cs
│       ├── Constraints/
│       │   ├── MotionConstraints.cs
│       │   ├── MotionCollisionConstraints.cs
│       │   └── MotionLeashConstraints.cs
│       ├── Trajectory/
│       │   ├── ITrajectory3D.cs
│       │   ├── LinearTrajectory3D.cs
│       │   └── TrajectoryMotionSource.cs
│       └── Generic/
│           ├── FixedDeltaMotionSource.cs
│           ├── PathFollowerMotionSource.cs
│           ├── ScaledMotionSource.cs
│           └── WaypointTrajectory3D.cs
└── Tests/
    └── Editor/
        ├── com.abilitykit.combat.motion.tests.asmdef
        └── MotionSystemTests.cs
```

## 3. 核心抽象

### 3.1 `IMotionSource`

`IMotionSource` 是移动来源接口。每个 source 在 tick 中向 `outDesiredDelta` 累加或覆盖期望位移。

典型实现：

- `LocomotionMotionSource`：输入移动。
- `TrajectoryMotionSource`：轨迹位移。
- `FixedDeltaMotionSource`：持续击退/拉拽。
- `PathFollowerMotionSource`：路径跟随。
- `ScaledMotionSource`：包装并缩放另一个 source 的输出。

### 3.2 `MotionPipeline`

`MotionPipeline` 负责：

1. 移除 inactive source。
2. 按组选择有效 source。
3. 应用同组叠加策略。
4. 应用跨组抑制策略。
5. 调用有效 source 生成 `DesiredDelta`。
6. 调用 `IMotionSolver` 求解最终 `AppliedDelta`。
7. 写回 `MotionState` 并触发事件。

### 3.3 `IMotionSolver`

`IMotionSolver` 根据当前状态、期望位移和约束输出最终位移。默认 `NoMotionSolver` 直接接受期望位移。

`ConfigurableMotionSolver` 用于项目集成：

- `ConstraintsProvider` 决定每个 mover 的碰撞、穿透、leash 等规则。
- `IMotionCollisionWorld` 对接物理世界或自定义空间查询。
- `IMotionSolverDiagnostics` 用于记录异常、投影失败等诊断信息。

### 3.4 `IMotionSnapshotSource`

`IMotionSnapshotSource` 为预测/回滚提供 source 内部状态导出和恢复。

```csharp
public interface IMotionSnapshotSource
{
    bool ExportSnapshot(out MotionSourceSnapshot snapshot);
    bool ImportSnapshot(in MotionSourceSnapshot snapshot);
}
```

`MotionSourceSnapshot` 保存 group、priority、stacking、active、time、timeLeft、index、向量和标量扩展字段。它只保存运行时进度，不替代技能配置或轨迹资源。

## 4. Tick 数据流

单帧数据流：

```text
MotionState + Sources + dt
        │
        ▼
MotionPipeline.Tick
        │
        ├─ cleanup inactive sources
        ├─ select effective sources by group/priority/stacking
        ├─ apply cross-group suppression policy
        ├─ source.Tick => DesiredDelta
        ├─ IMotionSolver.Solve
        ├─ write AppliedDelta/NewVelocity/NewForward
        └─ update MotionState.Position/Velocity/Time
```

关键输出：

- `DesiredDelta`：所有有效 source 生成的期望位移。
- `AppliedDelta`：solver 处理后的最终位移。
- `NewVelocity`：由 `AppliedDelta / dt` 计算得到。
- `NewForward`：当前朝向，供上层动画或表现层使用。

## 5. 分组、优先级与叠加策略

`MotionGroups` 提供常用分组：

- `Locomotion`：常规输入移动。
- `Ability`：技能位移。
- `Control`：控制效果。
- `Path`：寻路/路径移动。

`MotionStacking` 决定同组策略：

- `Additive`：同组多个 source 可叠加。
- `ExclusiveHighestPriority`：同组只保留最高优先级 source。
- `OverrideLowerPriority`：作为强控制 source，并通过 `MotionPipelinePolicy` 抑制其他组。

`MotionPipelinePolicy` 描述跨组抑制关系。例如控制组抑制输入、技能和路径：

```csharp
policy.SetSuppressedGroups(
    MotionGroups.Control,
    MotionGroups.Locomotion,
    MotionGroups.Ability,
    MotionGroups.Path);
```

## 6. 碰撞与约束设计

### 6.1 约束拆分

`MotionConstraints` 由两部分组成：

- `MotionCollisionConstraints`：碰撞开关、穿透、半径、skin、mask、结束重叠策略。
- `MotionLeashConstraints`：限制 mover 不能离开锚点太远。

这样可以让战斗逻辑按状态动态决定移动规则，例如：

- 普通移动启用碰撞。
- 瞬移或特殊技能允许穿透。
- Boss 技能限制在战斗区域半径内。

### 6.2 碰撞世界接口

`IMotionCollisionWorld` 不绑定 Unity Physics，调用方可用 Unity `Physics`、自定义定点碰撞、导航网格或服务器空间索引实现。

核心能力：

- `Sweep`：沿期望位移检测并返回可用位移。
- `Overlap`：检测结束位置是否重叠。
- `TryProjectToFree`：尝试把重叠位置投影到最近可用点。

### 6.3 结束重叠策略

`MotionEndOverlapPolicy` 处理 sweep 后结束点仍重叠的情况：

- `AllowInside`：接受结束点，适合幽灵、穿模、无碰撞状态。
- `ProjectToNearestFree`：投影到最近可用点，适合角色胶囊体被轻微挤入障碍。
- `ClampToLastValid`：保留 sweep 提供的可用位移，适合保守移动。
- `Reject`：拒绝本帧位移，适合严格阻挡。

## 7. 快照与恢复设计

快照目标不是复制完整对象图，而是保存可确定恢复的运行进度。

各 source 的快照策略：

- `LocomotionMotionSource`：保存输入、速度和输入空间。
- `TrajectoryMotionSource`：保存轨迹时间 `_time` 和 active 状态。
- `FixedDeltaMotionSource`：保存剩余时间 `_timeLeft` 和每秒位移。
- `PathFollowerMotionSource`：保存当前路径点索引、速度、到达阈值和 active 状态。
- `ScaledMotionSource`：保存缩放系数和包装状态；内部 source 如需恢复应单独快照。

恢复顺序建议：

1. 根据技能/配置重建或租借 source。
2. 注入相同轨迹或路径配置。
3. 调用 `ImportSnapshot`。
4. 加入 `MotionPipeline`。

## 8. 对象池设计

模块依赖 `com.abilitykit.core` 的 `AbilityKit.Core.Pooling`。

已池化对象：

- `MotionPipeline`
- `LocomotionMotionSource`
- `TrajectoryMotionSource`
- `FixedDeltaMotionSource`
- `PathFollowerMotionSource`
- `ScaledMotionSource`
- `MotionPipeline` 内部临时集合：source list、best-index dictionary、suppressed-group list

池化约定：

- `Rent(...)` 负责配置对象到可用状态。
- `Release(...)` 负责归还对象，并通过 pool 的 `onRelease` 调用 `Reset()`。
- public 构造函数保留兼容性，但新代码推荐使用池化入口。
- `MotionPipeline.Dispose()` 会释放内部集合，业务侧通常应使用 `MotionPipelinePool.Release()`，不要重复 dispose 同一个对象。

仍可能分配的区域：

- 路径点防御性拷贝数组。
- waypoint 轨迹的累计长度数组。
- 外部业务创建轨迹/路径配置时的数组。

这些分配是为了避免外部可变数据导致非确定性。如果项目要求严格零 GC，可进一步接入数组池或把路径/轨迹预构建为不可变资源。

## 9. 测试策略

当前 Editor 测试覆盖重点：

- pipeline additive 合成。
- control 组对 locomotion 的默认抑制。
- trajectory snapshot 导出/恢复。
- path follower 和 waypoint trajectory 防御性拷贝。
- solver `ProjectToNearestFree` 策略。

后续建议补充：

- source 自动移除和 finish event。
- exclusive highest priority 选择。
- custom `MotionPipelinePolicy` 抑制矩阵。
- solver 的 `Reject`、`AllowInside`、`ClampToLastValid`。
- constraints provider 异常诊断。
- pool rent/release 生命周期。

## 10. 稳定性边界

该模块已经具备较完整的抽象和基础实现，但生产接入时仍建议：

- 在项目自己的碰撞世界中补齐 sweep/overlap/project 的确定性测试。
- 对所有技能位移配置建立回放用例。
- 明确 source 所有权和归还时机。
- 在帧同步项目中固定 tick dt，并避免 solver 读取非确定性外部状态。

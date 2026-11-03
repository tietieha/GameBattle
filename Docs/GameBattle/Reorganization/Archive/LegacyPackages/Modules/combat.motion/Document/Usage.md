# Motion 模块使用文档（com.abilitykit.combat.motion）

> 本文说明 `AbilityKit.Combat.MotionSystem` 的推荐接入方式、对象池用法、快照恢复和常见组合策略。

## 1. 模块定位

`com.abilitykit.combat.motion` 提供战斗移动的组合层：

- `MotionPipeline`：统一调度多个 `IMotionSource`。
- `IMotionSource`：输入移动、技能位移、击退、路径跟随等移动来源。
- `IMotionSolver`：约束、碰撞、范围限制和最终位移求解。
- `IMotionEventSink`：接收命中、到达、过期等事件。
- `IMotionSnapshotSource`：导出/导入移动源内部进度，支持预测、回滚和重放。

适合 MOBA、ARPG、RTS、帧同步或服务端权威移动等场景。

## 2. 推荐的实体移动组件组织方式

每个可移动实体建议持有一个 `MotionPipeline`，并在实体销毁或退场时归还：

```csharp
using AbilityKit.Core.Mathematics;
using AbilityKit.Combat.MotionSystem.Core;
using AbilityKit.Combat.MotionSystem.Collision;

public sealed class CombatMover
{
    private readonly int _entityId;
    private MotionPipeline _pipeline;
    private MotionState _state;
    private MotionOutput _output;

    public CombatMover(int entityId, Vec3 spawnPosition)
    {
        _entityId = entityId;
        _pipeline = MotionPipelinePool.Rent();
        _state = new MotionState(spawnPosition);
        _output = default;
    }

    public void Tick(float dt)
    {
        _pipeline.Tick(_entityId, ref _state, dt, ref _output);
    }

    public void Dispose()
    {
        MotionPipelinePool.Release(_pipeline);
        _pipeline = null;
    }
}
```

如果已有生命周期框架，也可以直接 `new MotionPipeline()`，但推荐使用 `MotionPipelinePool.Rent()` / `MotionPipelinePool.Release()` 降低实体频繁创建销毁时的 GC 压力。

## 3. 输入移动（Locomotion）

`LocomotionMotionSource` 用于玩家摇杆、键盘或 AI 输入移动。

```csharp
var locomotion = LocomotionMotionSource.Rent(
    speed: 5f,
    space: MotionInputSpace.World,
    priority: 0);

locomotion.SetInput(x: 1f, z: 0f);
pipeline.AddSource(locomotion);
```

不再使用时归还：

```csharp
pipeline.RemoveSource(locomotion);
LocomotionMotionSource.Release(locomotion);
```

输入空间：

- `MotionInputSpace.World`：输入直接解释为世界方向。
- `MotionInputSpace.Local`：输入根据 `MotionState.Forward` 转换为局部前后/左右移动。

## 4. 技能位移、击退和路径移动

### 4.1 轨迹位移

`TrajectoryMotionSource` 适合冲刺、跳跃、突进等按时间采样的位移。

```csharp
var trajectory = new LinearTrajectory3D(
    start: Vec3.Zero,
    end: new Vec3(4f, 0f, 0f),
    duration: 0.35f);

var dash = TrajectoryMotionSource.Rent(
    trajectory,
    priority: 20,
    groupId: MotionGroups.Ability,
    stacking: MotionStacking.ExclusiveHighestPriority);

pipeline.AddSource(dash);
```

`TrajectoryMotionSource` 会在轨迹结束后自动变为 inactive，`MotionPipeline` 后续 tick 会自动移除 inactive source，并向 `IMotionEventSink` 发出过期事件。

### 4.2 固定位移/击退

`FixedDeltaMotionSource` 适合击退、拉拽、短时间持续推力。

```csharp
var knockback = FixedDeltaMotionSource.Rent(
    deltaPerSecond: new Vec3(-6f, 0f, 0f),
    duration: 0.2f,
    priority: 30,
    groupId: MotionGroups.Ability,
    stacking: MotionStacking.Additive);

pipeline.AddSource(knockback);
```

### 4.3 路径跟随

`PathFollowerMotionSource` 会对传入的点数组做防御性拷贝，避免外部修改路径造成非确定性。

```csharp
var points = new[]
{
    new Vec3(0f, 0f, 0f),
    new Vec3(2f, 0f, 1f),
    new Vec3(4f, 0f, 1f),
};

var path = PathFollowerMotionSource.Rent(
    points,
    speed: 3f,
    arriveEpsilon: 0.05f,
    priority: 10,
    groupId: MotionGroups.Path,
    stacking: MotionStacking.ExclusiveHighestPriority);

pipeline.AddSource(path);
```

## 5. 跨组抑制策略

默认策略中，`MotionGroups.Control` 通常用于眩晕、强制位移、强控等控制类效果，并会抑制 `Locomotion`、`Ability`、`Path` 等组。

可通过 `MotionPipeline.Policy` 配置自定义抑制关系：

```csharp
pipeline.Policy = new MotionPipelinePolicy();
pipeline.Policy.SetSuppressedGroups(
    MotionGroups.Control,
    MotionGroups.Locomotion,
    MotionGroups.Ability,
    MotionGroups.Path);
```

常见约定：

- `Locomotion`：玩家/AI 常规移动，通常 `Additive`。
- `Ability`：技能位移，通常 `ExclusiveHighestPriority`。
- `Control`：强控或强制移动，通常 `OverrideLowerPriority`。
- `Path`：导航路径移动，通常 `ExclusiveHighestPriority`。

## 6. 碰撞与约束求解

`ConfigurableMotionSolver` 通过 `IMotionCollisionWorld` 接入项目自己的碰撞世界。

```csharp
pipeline.Solver = new ConfigurableMotionSolver(
    world,
    (int moverId, in MotionState state, in MotionOutput input, float dt) =>
        new MotionConstraints(
            new MotionCollisionConstraints(
                enable: true,
                allowPassThrough: false,
                endOverlapPolicy: MotionEndOverlapPolicy.ProjectToNearestFree,
                radius: 0.4f,
                skin: 0.02f,
                obstacleMask: 1,
                ignoreMask: 0),
            MotionLeashConstraints.Disabled));
```

`MotionEndOverlapPolicy`：

- `AllowInside`：允许结束点仍在重叠内。
- `ProjectToNearestFree`：尝试调用 `TryProjectToFree` 投影到最近可用位置。
- `ClampToLastValid`：保留 sweep 已求出的可用位移。
- `Reject`：重叠时拒绝本帧位移。

如果需要诊断异常或投影失败，可实现 `IMotionSolverDiagnostics` 并传入 solver。

## 7. 快照与回滚

支持快照的 source 实现 `IMotionSnapshotSource`：

```csharp
if (source is IMotionSnapshotSource snapshotSource &&
    snapshotSource.ExportSnapshot(out var snapshot))
{
    // 保存 snapshot 到预测/回滚状态
}
```

恢复时，先用相同配置创建或租借 source，再导入 snapshot：

```csharp
var restored = TrajectoryMotionSource.Rent(trajectory);
restored.ImportSnapshot(in snapshot);
pipeline.AddSource(restored);
```

注意：轨迹对象本身通常应由技能配置或回放上下文重新创建，`MotionSourceSnapshot` 只保存运行进度，不保存完整轨迹数据。

## 8. 对象池使用建议

当前模块推荐优先使用以下池化入口：

- `MotionPipelinePool.Rent()` / `MotionPipelinePool.Release()`
- `LocomotionMotionSource.Rent()` / `LocomotionMotionSource.Release()`
- `TrajectoryMotionSource.Rent()` / `TrajectoryMotionSource.Release()`
- `FixedDeltaMotionSource.Rent()` / `FixedDeltaMotionSource.Release()`
- `PathFollowerMotionSource.Rent()` / `PathFollowerMotionSource.Release()`
- `ScaledMotionSource.Rent()` / `ScaledMotionSource.Release()`

兼容历史代码的 public 构造函数仍保留，但在高频创建销毁路径上不推荐使用。

归还 source 前建议先从 pipeline 移除，避免仍被 tick：

```csharp
pipeline.RemoveSource(source);
TrajectoryMotionSource.Release(source);
```

## 9. 事件处理

实现 `IMotionEventSink` 可接收命中、到达、过期等事件：

```csharp
public sealed class MotionEvents : IMotionEventSink
{
    public void OnHit(int moverId, in MotionState state, in MotionHit hit)
    {
        // 播放碰撞反馈或通知战斗逻辑
    }

    public void OnArrive(int moverId, IMotionSource source)
    {
        // 路径到达
    }

    public void OnExpired(int moverId, IMotionSource source)
    {
        // 技能位移结束、击退结束等
    }
}
```

## 10. 常见问题

### Q1：为什么 source 结束后没有立即手动释放？

`MotionPipeline` 只负责从内部列表移除 inactive source，不会假设 source 的所有权。业务侧如果使用池化 source，应在确认不再引用后调用对应 `Release()`。

### Q2：如何避免不同平台结果不一致？

- 输入数组会做防御性拷贝，避免外部修改。
- 同一组使用明确的 priority 决定胜出 source。
- 快照只保存进度和必要标量，恢复时应使用同一份技能/轨迹配置。
- 尽量避免在 solver 中读取非确定性状态。

### Q3：是否完全没有分配？

模块已将 pipeline、常用 source 和内部临时集合接入 core 对象池。路径点数组与轨迹累计长度数组仍可能因防御性拷贝产生分配；如需要严格零 GC，可继续接入数组池或预构建不可变轨迹资源。

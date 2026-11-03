# Buff 运行时提取与接入

日期：2026-10-26。工程：`E:/Unity/AbilityKit/Unity`。
本阶段继续前轮 Example 提取，将实体 Buff 的可复用生命周期核心提取到 Runtime，
并让 MOBA 调用共享实现；全局 Buff 暂缓，不新增独立时钟或程序集。

## 目录与职责

```text
Assets/Scripts/GameBattle/Runtime/
  Buff/
    State/BuffState.cs
    Policies/BuffStackingSettings.cs
    Policies/BuffStackingPolicyApplier.cs
    Storage/BuffRuntimeKey.cs
    Storage/BuffStorage.cs
    Commands/BuffCommandQueue.cs
    Lifecycle/BuffApplyProcessor.cs
    Lifecycle/BuffEndProcessor.cs
    Lifecycle/BuffLifetimeReconciler.cs
    Timing/BuffDuration.cs
    Context/BuffContextBinding.cs
  Effect/Continuous/Timing/FixedIntervalTicker.cs
```

| 模块 | Runtime 负责 | 外部调用方负责 |
| --- | --- | --- |
| State | Buff、来源、层数、剩余时间和上下文身份字段 | 扩展业务状态与保存格式 |
| Policies | 忽略、替换、加层、刷新和初始化 | 将配置转换为 `BuffStackingSettings` |
| Storage | 按 Buff/来源/上下文匹配、索引、替换与移除 | 提供实例列表；直接修改匹配字段后调用 `MarkDirty` |
| Commands | 顺序号、预算化消费、立即请求结果和重入保护 | 定义命令载荷、提交时机和执行回调 |
| Apply | 实例选择、叠层、候选提交、替换顺序和失败清理 | 工厂、配置、效果、上下文及通知绑定 |
| End | 移除后按顺序清理；单步异常不阻断后续清理 | 停止效果、解除绑定、通知、清空与归还实例 |
| Lifetime | 倒序遍历、同步、终止判断与移除调用 | 判断打断条件、同步持续状态、判断结束 |
| Duration | 基于已推进的定点 elapsed 计算剩余时间和到期 | 提供 elapsed；不再次累加同一个逻辑帧 |
| Context | 上下文身份创建、恢复、所有权校验和销毁 | live 数据、快照载荷、序列化和绑定判定 |
| FixedIntervalTicker | 定点间隔调度、单次执行预算 | 提供帧步长、间隔、剩余量和执行回调 |

`BuffRuntimeKey`、`BuffStorage<T>` 等稳定数据/算法使用具体类型。
只在业务可变的生命周期阶段使用端口，不引入全局服务定位器。

## 上下文字段

以下字段从 MOBA `BuffRuntime` 提取到 `BuffState`：

- `RuntimeContextId`：运行时上下文编号。
- `RuntimeContextVersion`：持久化/恢复使用的版本。
- `RuntimeContextIdentity`：当前 Registry 内的本地引用身份。

`SourceContextId` 是来源上下文，不是 Buff 自身的上下文。
MOBA `BuffRuntime` 继承通用字段，原字段访问方式不变。
`ClearRuntimeContext()` 统一清空三个运行时身份字段。
`BuffContextBinding` 不规定 MOBA 快照数据结构；恢复时仍须由外部恢复业务载荷并绑定本地身份。

## 逻辑帧推进

现有 MOBA 接入顺序保持不变：

```text
战斗逻辑帧
  -> MobaBuffCommandDrainSystem
     -> MobaBuffService -> BuffCommandQueue -> BuffApplyProcessor / BuffEndProcessor
  -> MobaContinuousTickSystem
     -> IWorldClock.DeltaTime -> MobaContinuousManager.Tick
     -> BuffContinuousRuntime 推进 elapsed
     -> BuffDuration 计算剩余时间
     -> MobaContinuousTickProcessor -> FixedIntervalTicker 执行间隔效果
  -> MobaBuffLifecycleReconcileSystem
     -> MobaBuffService -> BuffLifetimeReconciler
     -> 同步剩余时间 / 判断中断和结束 -> BuffEndProcessor
```

当前系统顺序是 BuffCommandsDrain (+295)、ContinuousTick (+307)、
BuffLifecycleReconcile (+310)，均处于 World Execute 阶段。
生命周期调和不是第二套计时；它读取持续过程的结果，不能再次扣时。
新外部宿主应从自己的战斗逻辑帧提供时间和调用时机，不从 `Time.deltaTime` 单独推进 Buff。
是否暂停由宿主与持续过程的暂停策略决定。

## 外部接入

1. 定义业务运行时类型，继承 `BuffState`，持有实体实例列表。
2. 将配置解析成 `BuffApplication<TPayload>`，实现 `IBuffApplyPort` 的业务绑定阶段。
3. 实现 `IBuffEndPort` 的清理与对象回收，实例移除走共享处理器。
4. 使用 `BuffCommandQueue` 统一消费命令，在逻辑帧中提供执行预算。
5. 推进持续过程一次，再通过 `IBuffLifetimePort` 同步和处理到期。
6. 需要上下文时接入 `BuffContextBinding`；载荷与快照仍由自己的业务层定义。

这些端口不自动接管游戏宿主；本轮已接入 MOBA 的既有调用链。
容器、队列和生命周期调用按单线程使用，不宣称具备并发安全。
持续时间和间隔的定点计算复用既有精度；对外 float 字段与 MOBA 回滚格式保留。

## 保留在 Example

- MOBA Buff 配置解析、Actor/Entitas 访问与具体系统装配。
- 标签、技能运行时保留、来源追踪及触发器绑定。
- 阶段效果、表现 cue、业务错误报告与条件判断。
- live 上下文内容和 MOBA 快照载荷/恢复编排。

这次不是将 MOBA BuffService 整体改名为通用服务，
也不表示所有玩法规则已经实现。实体/队伍/全局作用域仍需后续设计。
原代码与元数据备份、授权改动哈希见 `changes.json` 和 `Backups`。

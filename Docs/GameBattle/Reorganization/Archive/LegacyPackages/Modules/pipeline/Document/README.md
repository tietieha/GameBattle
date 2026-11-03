# Pipeline — 技能管线编排

## 设计理念

Pipeline 回答的问题是："一个技能应该经历哪些执行步骤？"

Pipeline 将技能建模为一个 Phase（阶段）序列。每个 Phase 都有自己的生命周期：`Execute()` 决定是否执行，`OnUpdate(dt)` 在每一帧驱动进度，`IsComplete` 标志阶段是否结束。Pipeline 本身是一个**配置容器**，调用 `Start()` 后返回 `Run` 实例，通过外部 `Tick(dt)` 驱动整个流程。

**Pipeline vs Flow**：两者都支持 Sequence/Parallel/Conditional，但定位不同。Pipeline 面向**技能执行流程**，强结构化，支持中断/暂停/恢复。Flow 面向**异步/时间驱动的通用逻辑**，更灵活，支持 WAKE/PUMP 机制。Pipeline 适合技能施放（冷却→吟唱→施法→后摇），Flow 适合跨系统协调。

## 核心抽象

```
IAbilityPipelinePhase<TCtx>
├── Execute()          → ShouldExecute 判断
├── OnUpdate(dt)       → 驱动 IsComplete
├── IsComplete          → 阶段是否结束
├── ShouldExecute()     → 条件预检
└── Reset()            → 重置状态

IAbilityPipelineRun<TCtx>
├── Tick(dt)            → 驱动 Run
├── Pause() / Resume()  → 暂停与恢复
├── Interrupt()         → 中断
└── State / Context     → 状态与上下文
```

## 内置 Phase 类型

| 类型 | 说明 |
|------|------|
| `AbilitySequencePhase` | 顺序执行子阶段，失败即停 |
| `AbilityParallelPhase` | 并行执行，收集所有结果 |
| `AbilityConditionalPhase` | 条件分支，多路分支 |
| `AbilityDelayPhase` | 简单时间延迟 |
| `AbilityRepeatPhase` | 循环/迭代 |
| `AbilityTimelinePhase` | 关键帧事件调度（TODO） |

## 快速示例

```csharp
// 定义 Pipeline
var pipeline = AbilityPipeline.Start(config, context)
    .AddPhase(new InstantPhase("CheckCooldown"))       // 瞬时：冷却检查
    .AddPhase(new DurationalPhase("Cast", 0.5f))       // 持续：吟唱 0.5s
    .AddPhase(new TimelinePhase("Execute")               // 时间轴：关键帧
        .AddEvent(0.0f, () => SpawnProjectile())
        .AddEvent(0.3f, () => PlayEffect())
        .AddEvent(0.5f, () => DealDamage())
    )
    .AddPhase(new InstantPhase("Aftercast"));           // 瞬时：后摇

// 外部 Tick 驱动
while (pipeline.State == PipelineState.Executing)
{
    pipeline.Run.Tick(deltaTime);
}
```

## 扩展机制

- **自定义 Phase**：实现 `IAbilityPipelinePhase<TCtx>`
- **中断支持**：实现 `IInterruptiblePhase<TCtx>` 提供 `OnInterrupt()`
- **扩展点**：通过 `AbilityPipeline_ExtensionPoint` 在 Phase 生命周期注入逻辑

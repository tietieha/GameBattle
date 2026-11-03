# Triggering — 事件触发规则引擎

## 一、定位

Triggering 回答的问题是：**当事件发生时，哪些规则应该被评估、以什么顺序执行、以及动作如何被调度？**

它是 AbilityKit 的事件触发与规则执行包，正式主线围绕 `TriggerRunner<TCtx>`、`TriggerPlan<TArgs>`、`PlannedTrigger<TArgs, TCtx>`、`ExecCtx<TCtx>`、`ActionRegistry`、`FunctionRegistry` 和验证体系展开。包内不假设 Buff、子弹、AOE 等业务概念；这些概念应作为项目层动作、条件、上下文或调度驱动接入。

Triggering 与 Pipeline 的关系：Pipeline 负责流程编排，Triggering 负责流程中事件响应与规则执行。两者互补，不互相替代。

## 二、正式主线

```text
TriggerRunner<TCtx>
├── Register / RegisterPlan       → 注册手写触发器或数据化计划
├── Dispatch / EventBus.Publish   → 发布事件并触发规则
└── Phase → Priority → Order      → 稳定排序与执行顺序

TriggerPlan<TArgs>
├── PredicateExprPlan             → 数据化条件表达式
├── ActionCallPlan[]              → 数据化动作调用
└── ActionSchedulePlan            → 触发器内部动作调度

ExecCtx<TCtx>
├── Context                       → 项目上下文
├── FunctionRegistry              → 条件函数扩展点
├── ActionRegistry                → 动作扩展点
├── BlackboardResolver            → 黑板数据解析
├── NumericDomains                → 数值域解析
├── ActionSchedulerManager        → TriggerPlan 内部动作调度
└── ExecutionControl              → StopPropagation / Cancel
```

## 三、扩展方式

- 条件扩展：通过 `FunctionRegistry` 注册确定性或非确定性函数，再由 `PredicateExprPlan` 引用。
- 动作扩展：通过 `ActionRegistry` 注册强类型动作，再由 `ActionCallPlan` 引用。
- 上下文扩展：通过自定义 `TCtx`、`ITriggerContextSource<TCtx>` 或 `ITriggerDispatcherContext` 提供项目服务。
- 调度扩展：触发器内部动作延迟/周期执行使用 `Runtime.ActionScheduler`；自然语言规则调度使用 `Runtime.RuleScheduler`。
- 校验扩展：通过 `ITriggerValidator<TCtx>` 或独立验证器在运行前发现配置错误。

## 四、调度边界

Triggering 内存在多层历史调度代码，正式版本按以下边界使用：

| 层级 | 定位 | 推荐状态 |
| --- | --- | --- |
| `Runtime.ActionScheduler` | `TriggerPlan` 内部动作的延迟、周期、连续执行 | 正式主线 |
| `Runtime.RuleScheduler` | 自然语言规则拆解后的通用时间意图，例如“立即/延后/每隔/保持期间” | 正式主线 |
| `Runtime.Schedule` | 早期通用调度与适配层 | 兼容层，谨慎新增依赖 |
| `Runtime.Scheduler` | 更早的业务 ID 风格调度器 | 遗留兼容层，迁移到前两者 |

`RuleScheduler` 的 `WhileActive` 表示“调度条目未完成、未取消、未中断时持续按间隔执行”，不代表 Core Continuous 的 `ContinuousState.Active`，也不接管 Buff 生命周期。Buff 等持续行为仍应由 Core `IContinuousManager` 管理，Triggering 只负责规则触发与调度驱动适配。

## 五、最小接入示例

```csharp
var actionId = new ActionId(StableStringId.Get("demo:apply_damage"));
var actions = new ActionRegistry();
actions.Register<NamedAction0<DamageEvent, object, BattleContext>>(
    actionId,
    (evt, args, ctx) => ctx.Context.ApplyDamage(evt.Amount),
    isDeterministic: true);

var plan = new TriggerPlan<DamageEvent>(
    phase: 0,
    priority: 100,
    triggerId: 10001,
    predicate: PredicateExprPlan.True,
    actions: new[] { ActionCallPlan.NoArgs(actionId) });

var runner = new TriggerRunner<BattleContext>(
    eventBus,
    new FunctionRegistry(),
    actions,
    contextSource: battleContextSource);

runner.RegisterPlan(damageEventKey, in plan);
```

## 六、正式文档索引

- [`StageClosureGuide.md`](StageClosureGuide.md)：阶段性可用包结论、推荐接入路径、对象池接入建议和后续测试流程。
- [`FormalApiBoundary.md`](FormalApiBoundary.md)：正式 API、兼容 API、迁移方向和调度选择表。
- [`LegacyMigrationPolicy.md`](LegacyMigrationPolicy.md)：legacy、compatibility、experimental 入口分级、迁移优先级、删除条件和延后决策。
- [`ProductAcceptanceGuide.md`](ProductAcceptanceGuide.md)：产品化验收基线、测试矩阵、诊断链路、FAQ、升级说明模板和样板工程边界。
- [`Triggering-Commercial-Remediation-Checklist.md`](Triggering-Commercial-Remediation-Checklist.md)：商业化整改状态、剩余产品化项和后续执行顺序。
- [`Samples/FormalTriggeringMainlineExample.cs`](../Samples/FormalTriggeringMainlineExample.cs)：只使用正式入口的可复制接入样板。

## 七、稳定性约束

- 数据化计划进入运行前应执行验证，至少覆盖引用、动作参数、调度语义和 UGC 限制。
- `ExecPolicy.RequireDeterministic` 下，计划不得依赖随机数、系统时间、网络请求等非确定性能力。
- Runtime 包不应新增 Buff、Projectile、AOE 等项目词汇；这些应下沉到 Demo、Samples 或业务包。
- 遗留目录保留是为了兼容现有 GUID 和旧项目引用，不代表正式主线入口。

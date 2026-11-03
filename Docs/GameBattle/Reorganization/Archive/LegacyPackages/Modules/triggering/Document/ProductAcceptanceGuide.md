# Triggering 产品化验收指南

## 验收目标

| 维度 | 要求 | 验收方式 |
| --- | --- | --- |
| 主线入口 | 新功能只依赖 `TriggerRunner<TCtx>`、`TriggerPlan<TArgs>`、`PlannedTrigger<TArgs, TCtx>`、`ExecCtx<TCtx>`、`ActionRegistry`、`FunctionRegistry`、`ActionScheduler`、`RuleScheduler` 与 `Runtime/Validation` | 搜索新增代码是否继续引入旧派发、旧调度或旧执行入口 |
| 确定性 | `ExecPolicy.RequireDeterministic` 下不得依赖随机数、系统时间、网络请求或未登记的非确定性函数/动作 | 同一计划、同一事件序列、同一上下文快照重复执行，动作顺序和结果一致 |
| 计划动作 | 计划内动作必须通过正式的 `ActionScheduler` 组织 | 检查计划执行路径是否仍然保留直达执行捷径 |
| 业务调度 | 持续、周期或延迟类业务节奏必须通过 `RuleScheduler` 组织 | 检查样例与测试是否都只使用正式调度入口 |
| 对象池 | 运行时热点对象应通过 `TriggeringRuntimePools` 或显式 fallback 管理 | 检查 `TriggeringRuntimePoolingHotspotScanTests` 是否覆盖新增热点 |
| 阶段收口 | 包说明、接入路径、测试命令与验收标准必须能从文档索引找到 | 检查 `StageClosureGuide.md`、`Documentation~/README.md` 与本指南是否同步 |

## 记录要求

- 验收报告只记录正式入口、正式边界和正式样例。
- 文档、示例与测试不再维护旧兼容结论。
- 新增能力必须先落到正式主线，再补充对应验证。
- 正式样例以 `Samples/FormalTriggeringMainlineExample.cs` 作为最小可复制入口。
- 运行时诊断以 `TriggeringDiagnosticCollector` 和编辑器诊断菜单作为验收入口。

## 固定验证流程

每次修改 Triggering Runtime、Plans、Scheduling、Validation、Pooling、Samples、Document 或 Documentation~ 后，执行以下命令：

```powershell
dotnet build Unity\AbilityKit.Triggering.csproj --no-restore -v:minimal
dotnet build Unity\AbilityKit.Triggering.Tests.csproj --no-restore -v:minimal
```

构建通过后，在 Unity Test Runner 中执行 `com.abilitykit.triggering` 的 Editor tests，重点确认以下扫描测试：

- `RuntimeCompatibilityLegacyEntryScanTests`：外部包不得重新引用历史触发器入口。
- `TriggeringProductAcceptanceChecklistTests`：文档、样例、菜单验收入口保持同步。
- `TriggeringRuntimePoolingHotspotScanTests`：已池化运行时热点不得重新散落直接实例化。

## 常用检查

1. 搜索新增代码是否仍然引用旧调度或旧派发入口。
2. 检查示例是否仅使用正式计划、上下文与调度类型。
3. 检查验收测试是否覆盖计划执行、动作调度与业务调度。
4. 检查对象池接入是否只覆盖短生命周期运行时热点。
5. 检查产品说明是否仅描述正式能力，并指向 `StageClosureGuide.md`。

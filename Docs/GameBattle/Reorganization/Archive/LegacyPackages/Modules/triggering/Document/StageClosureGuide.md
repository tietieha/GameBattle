# Triggering 阶段性收口指南

## 当前状态

Triggering 当前可以作为 AbilityKit 中“事件发生后，按规则评估并执行动作”的阶段性可用包使用。它已经具备正式主线、基础产品化验收、对象池试点接入和回归扫描，适合进入小规模业务接入、样板工程验证和后续增量测试阶段。

当前结论：

- **主线基本稳定。** 新接入优先围绕 `TriggerRunner<TCtx>`、`TriggerPlan<TArgs>`、`PlannedTrigger<TArgs, TCtx>`、`ExecCtx<TCtx>`、`ActionRegistry`、`FunctionRegistry`、`ActionScheduler`、`RuleScheduler` 和 `Runtime/Validation` 展开。
- **调度职责已收口。** 计划内动作使用 `ActionScheduler`；自然语言规则拆出来的延迟、周期、持续节奏使用 `RuleScheduler`；早期调度目录只作为兼容或历史层存在。
- **对象池已接入第一阶段。** `ActionInstance`、动作 executor 包装器和短生命周期 schedule adapter/context 已接入 `com.abilitykit.core` 的 `PoolScope`/`ObjectPool<T>`。
- **验收扫描已建立。** 已有测试覆盖正式入口、legacy 防回流、产品化清单和运行时池化热点边界。

## 推荐接入路径

1. **先接入正式样板。** 参考 `Samples/FormalTriggeringMainlineExample.cs`，用 `TriggerRunner<TCtx>` 注册数据化计划。
2. **再注册项目动作。** 通过 `ActionRegistry` 暴露项目层动作，不在 Triggering Runtime 中新增 Buff、Projectile、AOE 等业务词汇。
3. **按需接入条件函数。** 通过 `FunctionRegistry` 注册确定性函数；如果计划声明 `ExecPolicy.RequireDeterministic`，不得依赖随机数、系统时间、网络请求或未标记的非确定性动作。
4. **调度只走正式入口。** `TriggerPlan` 内部延迟/周期动作走 `ActionScheduler`；跨规则或自然语言持续节奏走 `RuleScheduler`。
5. **运行时热点启用对象池。** 战斗、关卡、世界或玩法域创建一个 `TriggeringRuntimePools`，传给调度管理对象，并在生命周期结束时释放。

## 对象池接入建议

对象池不是为了隐藏所有 `new`，而是让高频、短生命周期对象的生命周期可控。当前优先池化以下对象：

| 对象 | 用途 | 生命周期建议 |
| --- | --- | --- |
| `ActionInstance` | 计划动作调度实例 | 随 `ActionScheduler` 注册/完成/移除租还 |
| `DefaultActionExecutor` | 默认动作执行包装 | 随拥有它的 `ActionInstance` 归还 |
| `QueuedActionExecutor` | 队列策略包装 | 递归释放内部 executor |
| `RetryActionExecutor` | 重试策略包装 | 递归释放内部 executor |
| `ScheduleToBehaviorContextAdapter` | 调度上下文到行为上下文的短生命周期适配 | 每次评估/更新后立即归还 |

推荐生命周期：

```csharp
var pools = TriggeringRuntimePools.CreateDefault("battle-001");
pools.PrewarmAll();

var schedulerManager = new ActionSchedulerManager(pools);

// 战斗/关卡结束时：
schedulerManager.Clear();
pools.Dispose();
```

注意事项：

- 池作用域应绑定世界、战斗、关卡或玩法域，不建议散落静态全局池。
- 被外部长期持有、跨帧引用或由业务层拥有的对象不要强行池化。
- 使用 `GetDebugSnapshots()` 可在 Unity Editor 下观察池数量和活跃状态。
- 若业务没有传入 `TriggeringRuntimePools`，代码仍保留无池 fallback 路径，便于渐进迁移。

## 后续测试流程

每次修改 Triggering Runtime、Plans、Scheduling、Validation、Pooling、Samples 或文档验收内容后，至少执行以下流程：

1. **主工程构建**

   ```powershell
   dotnet build Unity\AbilityKit.Triggering.csproj --no-restore -v:minimal
   ```

2. **测试工程构建**

   ```powershell
   dotnet build Unity\AbilityKit.Triggering.Tests.csproj --no-restore -v:minimal
   ```

3. **热点与入口扫描**

   通过编辑器测试覆盖以下扫描：

   - `RuntimeCompatibilityLegacyEntryScanTests`：防止外部包重新引用历史入口。
   - `TriggeringProductAcceptanceChecklistTests`：防止验收文档、样例和菜单入口脱节。
   - `TriggeringRuntimePoolingHotspotScanTests`：防止已池化热点重新散落直接实例化。

4. **Unity Editor 回归**

   在 Unity Test Runner 中执行 `com.abilitykit.triggering` 的 Editor tests，确认构建外的编辑器菜单、包路径解析和扫描测试正常。

## 阶段完成标准

一个 Triggering 变更可以视为阶段性完成，需要同时满足：

- 新增能力从正式主线进入，不引入新的 legacy/compatibility 入口。
- 文档索引、验收指南和样例能说明该能力如何使用。
- 对运行时热点对象，明确是否池化；不池化时说明生命周期原因。
- `AbilityKit.Triggering.csproj` 与 `AbilityKit.Triggering.Tests.csproj` 均能构建通过。
- 对涉及正式边界、目录迁移、对象池或调度行为的改动，补充对应扫描或回归测试。

## 后续演进建议

- 继续把业务样板集中到 Samples 或 Demo，不污染 Runtime 包。
- 对 `RuleScheduler` 与 `ActionScheduler` 的典型业务组合补更完整样例。
- 后续若继续池化更多对象，先补热点扫描，再改生命周期，不直接为了减少 `new` 扩大池化范围。
- 在正式发布前，用 Unity Test Runner 跑完整 Editor tests，并保留构建日志作为版本验收记录。

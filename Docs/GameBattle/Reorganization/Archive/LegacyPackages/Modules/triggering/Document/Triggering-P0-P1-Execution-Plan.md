# Triggering P0-P1 执行计划

## 总体原则
- 保持 `TriggerPlan`、`PlannedTrigger`、`ExecCtx`、`ActionRegistry`、`FunctionRegistry`、`TriggerRunner`、`RuleScheduler`、`Validation` 为正式入口。
- 新增功能不得再扩散到 `Runtime/Legacy`、`Runtime/Scheduler`、`Runtime/Experimental`。
- `Runtime` 根目录 `.cs` 兼容占位文件已清理完成，后续不再作为新代码落点或旧路径保留区。

## 现状
- 规则级持续执行统一走 `RuleScheduler`。
- 旧 `Scheduler` 和 `SchedulerRegistry` 不再作为正式扩展点。
- 包内文档、示例、测试已指向主线调度方案；后续若发现旧术语残留，应直接替换为正式类型名称。

## 推荐推进顺序
1. 清理文档、样例与扫描测试中的旧术语。
2. 复核是否还有任何旧入口回流。
3. 最后执行全仓搜索并复核构建结果。

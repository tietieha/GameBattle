# Triggering 运行时设计

- 新能力只进入正式主线，不再新增旧派发、旧调度或迁移壳。
- 计划内动作使用 `ActionScheduler`，业务节奏使用 `RuleScheduler`。
- 上下文访问通过正式服务与适配层完成，不回流到历史兼容入口。
- `Runtime/Dispatcher`、`Runtime/Scheduler`、`Runtime/Legacy`、`Runtime/Experimental` 不作为新增正式能力的落点。
- 示例、验收与诊断材料只描述正式入口与正式边界。
- 历史记录如无继续使用价值，优先清理而不是长期保留。

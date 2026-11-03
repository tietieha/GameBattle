using AbilityKit.Triggering.Runtime.Config;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Triggering.Validation
{
    /// <summary>
    /// ActionCallPlan 结构语义校验器。
    /// 负责在运行前拦截 Action 调用计划中的非法组合，避免错误延迟到 PlannedTrigger/ActionScheduler 执行期。
    /// </summary>
    public sealed class ActionCallPlanValidator<TCtx> : ITriggerValidator<TCtx>
    {
        public string Name => "Action 调用计划校验";
        public int Priority => 2;
        public bool IsCritical => true;

        public ValidationResult Validate(in TriggerPlanDatabase<TCtx> database, in ValidationContext<TCtx> context)
        {
            var result = new ValidationResult();

            foreach (var entry in database.Plans)
            {
                ValidateEntry(entry, ref result);
            }

            return result;
        }

        private static void ValidateEntry(TriggerPlanEntry<TCtx> entry, ref ValidationResult result)
        {
            var actions = entry.Plan.Actions;
            if (actions == null) return;

            var path = entry.GetPath();
            for (int i = 0; i < actions.Length; i++)
            {
                ValidateAction(in actions[i], $"{path}.actions[{i}]", ref result);
            }
        }

        private static void ValidateAction(in ActionCallPlan action, string path, ref ValidationResult result)
        {
            var arguments = action.Arguments;
            var schedule = action.Schedule;
            var execution = action.Execution;

            ValidateArity(in arguments, path, ref result);
            ValidateSchedule(in schedule, path, ref result);
            ValidateExecutionPolicy(in execution, path, ref result);
        }

        private static void ValidateArity(in ActionArgumentsPlan arguments, string path, ref ValidationResult result)
        {
            if (arguments.Arity > 2)
            {
                result.AddError(
                    ValidationErrorCodes.UNSUPPORTED_ACTION_ARITY,
                    $"Action 参数数量 {arguments.Arity} 超过当前 PlannedTrigger 执行器支持范围 0-2",
                    path);
            }

            if (arguments.NamedArgs != null && arguments.NamedArgs.Count != arguments.Arity)
            {
                result.AddError(
                    ValidationErrorCodes.ACTION_ARG_COUNT_MISMATCH,
                    $"Action 具名参数数量 {arguments.NamedArgs.Count} 与 Arity {arguments.Arity} 不一致",
                    $"{path}.args");
            }

            if (arguments.NamedArgs != null)
            {
                foreach (var pair in arguments.NamedArgs)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                    {
                        result.AddError(
                            ValidationErrorCodes.INVALID_ACTION_ARGUMENT,
                            "Action 具名参数名不能为空",
                            $"{path}.args");
                    }
                }
            }
        }

        private static void ValidateSchedule(in ActionSchedulePlan schedule, string path, ref ValidationResult result)
        {
            switch (schedule.Mode)
            {
                case EActionScheduleMode.Immediate:
                    if (schedule.Param != 0f)
                    {
                        result.AddWarning(
                            ValidationErrorCodes.UNUSED_ACTION_SCHEDULE_PARAM,
                            $"Immediate Action 配置了无效调度参数 {schedule.Param}",
                            $"{path}.scheduleParam");
                    }
                    break;

                case EActionScheduleMode.Delayed:
                    if (schedule.Param < 0f)
                    {
                        result.AddError(
                            ValidationErrorCodes.INVALID_ACTION_SCHEDULE,
                            $"Delayed Action 延迟时间不能为负数: {schedule.Param}",
                            $"{path}.scheduleParam");
                    }
                    break;

                case EActionScheduleMode.Periodic:
                    if (schedule.Param <= 0f)
                    {
                        result.AddError(
                            ValidationErrorCodes.INVALID_ACTION_SCHEDULE,
                            $"Periodic Action 周期间隔必须大于 0: {schedule.Param}",
                            $"{path}.scheduleParam");
                    }
                    break;

                case EActionScheduleMode.Continuous:
                    if (schedule.Param < 0f)
                    {
                        result.AddError(
                            ValidationErrorCodes.INVALID_ACTION_SCHEDULE,
                            $"Continuous Action 调度间隔不能为负数: {schedule.Param}",
                            $"{path}.scheduleParam");
                    }
                    break;

                case EActionScheduleMode.Timeline:
                    result.AddError(
                        ValidationErrorCodes.UNSUPPORTED_ACTION_SCHEDULE,
                        "Timeline Action 当前没有正式的子 Action 时间线计划结构，不能作为主线 ActionCallPlan 使用",
                        $"{path}.scheduleMode");
                    break;

                default:
                    result.AddError(
                        ValidationErrorCodes.INVALID_ACTION_SCHEDULE,
                        $"未知 Action 调度模式: {schedule.Mode}",
                        $"{path}.scheduleMode");
                    break;
            }

            if (schedule.MaxExecutions < -1 || schedule.MaxExecutions == 0)
            {
                result.AddError(
                    ValidationErrorCodes.INVALID_ACTION_SCHEDULE,
                    $"Action 最大执行次数必须为 -1 或正数: {schedule.MaxExecutions}",
                    $"{path}.maxExecutions");
            }
        }

        private static void ValidateExecutionPolicy(in ActionExecutionPlan execution, string path, ref ValidationResult result)
        {
            if (execution.RetryMaxRetries < 0)
            {
                result.AddError(
                    ValidationErrorCodes.INVALID_ACTION_RETRY,
                    $"Action 重试次数不能为负数: {execution.RetryMaxRetries}",
                    $"{path}.retryMaxRetries");
            }

            if (execution.RetryDelayMs < 0f)
            {
                result.AddError(
                    ValidationErrorCodes.INVALID_ACTION_RETRY,
                    $"Action 重试延迟不能为负数: {execution.RetryDelayMs}",
                    $"{path}.retryDelayMs");
            }

            if (execution.Policy == EActionExecutionPolicy.WithRollback)
            {
                result.AddError(
                    ValidationErrorCodes.UNSUPPORTED_ACTION_EXECUTION_POLICY,
                    "WithRollback 当前没有正式的补偿 Action 或回滚计划结构，不能作为主线 ActionCallPlan 使用",
                    $"{path}.executionPolicy");
            }

            if (execution.Policy != EActionExecutionPolicy.WithRetry &&
                (execution.RetryMaxRetries != 3 || execution.RetryDelayMs != 0f))
            {
                result.AddWarning(
                    ValidationErrorCodes.UNUSED_ACTION_RETRY,
                    $"Action 执行策略为 {execution.Policy}，但配置了重试参数",
                    path);
            }
        }
    }
}

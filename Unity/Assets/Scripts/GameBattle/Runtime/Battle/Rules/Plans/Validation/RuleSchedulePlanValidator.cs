using System;
using AbilityKit.Triggering.Runtime.RuleScheduler;

namespace AbilityKit.Triggering.Validation
{
    /// <summary>
    /// 规则调度计划校验器。
    /// 面向 Runtime.RuleScheduler 的自然语言时间意图，独立于具体业务对象生命周期。
    /// </summary>
    public static class RuleSchedulePlanValidator
    {
        public static ValidationResult Validate(in RuleSchedulePlan plan, string path = "ruleSchedule")
        {
            var result = new ValidationResult();
            Validate(in plan, ref result, path);
            return result;
        }

        public static void Validate(in RuleSchedulePlan plan, ref ValidationResult result, string path = "ruleSchedule")
        {
            if (!Enum.IsDefined(typeof(ERuleScheduleMode), plan.Mode))
            {
                result.AddError(
                    ValidationErrorCodes.INVALID_RULE_SCHEDULE,
                    $"未知规则调度模式: {plan.Mode}",
                    $"{path}.mode");
                return;
            }

            if (plan.Speed <= 0f)
            {
                result.AddError(
                    ValidationErrorCodes.INVALID_RULE_SCHEDULE,
                    $"规则调度速度必须大于 0: {plan.Speed}",
                    $"{path}.speed");
            }

            switch (plan.Mode)
            {
                case ERuleScheduleMode.Immediate:
                    ValidateImmediate(in plan, ref result, path);
                    break;
                case ERuleScheduleMode.Delayed:
                    ValidateDelayed(in plan, ref result, path);
                    break;
                case ERuleScheduleMode.Every:
                    ValidateEvery(in plan, ref result, path);
                    break;
                case ERuleScheduleMode.WhileActive:
                    ValidateWhileActive(in plan, ref result, path);
                    break;
            }

            if (plan.ReplaceExisting && string.IsNullOrEmpty(plan.GroupId) && string.IsNullOrEmpty(plan.SubjectId))
            {
                result.AddWarning(
                    ValidationErrorCodes.UNUSED_RULE_SCHEDULE_PARAM,
                    "ReplaceExisting 未提供 GroupId 或 SubjectId，默认驱动不会替换任何已有规则调度。",
                    $"{path}.replaceExisting");
            }
        }

        private static void ValidateImmediate(in RuleSchedulePlan plan, ref ValidationResult result, string path)
        {
            if (plan.DelayMs > 0f)
            {
                result.AddWarning(
                    ValidationErrorCodes.UNUSED_RULE_SCHEDULE_PARAM,
                    "Immediate 调度不会使用 DelayMs。",
                    $"{path}.delayMs");
            }

            if (plan.IntervalMs > 0f)
            {
                result.AddWarning(
                    ValidationErrorCodes.UNUSED_RULE_SCHEDULE_PARAM,
                    "Immediate 调度不会使用 IntervalMs。",
                    $"{path}.intervalMs");
            }
        }

        private static void ValidateDelayed(in RuleSchedulePlan plan, ref ValidationResult result, string path)
        {
            if (plan.DelayMs <= 0f)
            {
                result.AddWarning(
                    ValidationErrorCodes.AMBIGUOUS_RULE_SCHEDULE,
                    "Delayed 调度的 DelayMs 为 0，语义等同于 Immediate。",
                    $"{path}.delayMs");
            }

            if (plan.IntervalMs > 0f)
            {
                result.AddWarning(
                    ValidationErrorCodes.UNUSED_RULE_SCHEDULE_PARAM,
                    "Delayed 调度不会使用 IntervalMs。",
                    $"{path}.intervalMs");
            }
        }

        private static void ValidateEvery(in RuleSchedulePlan plan, ref ValidationResult result, string path)
        {
            if (plan.IntervalMs <= 0f)
            {
                result.AddError(
                    ValidationErrorCodes.INVALID_RULE_SCHEDULE,
                    $"Every 调度间隔必须大于 0: {plan.IntervalMs}",
                    $"{path}.intervalMs");
            }

            if (plan.MaxOccurrences == 0)
            {
                result.AddError(
                    ValidationErrorCodes.INVALID_RULE_SCHEDULE,
                    "Every 调度 MaxOccurrences 不能为 0；使用 -1 表示无限次，正数表示最大次数。",
                    $"{path}.maxOccurrences");
            }
        }

        private static void ValidateWhileActive(in RuleSchedulePlan plan, ref ValidationResult result, string path)
        {
            if (plan.IntervalMs <= 0f)
            {
                result.AddError(
                    ValidationErrorCodes.INVALID_RULE_SCHEDULE,
                    $"WhileActive 调度间隔必须大于 0: {plan.IntervalMs}",
                    $"{path}.intervalMs");
            }

            if (plan.MaxOccurrences > 0)
            {
                result.AddWarning(
                    ValidationErrorCodes.AMBIGUOUS_RULE_SCHEDULE,
                    "WhileActive 通常由外部取消或中断，正数 MaxOccurrences 会让它在达到次数后自动完成。",
                    $"{path}.maxOccurrences");
            }
        }
    }
}

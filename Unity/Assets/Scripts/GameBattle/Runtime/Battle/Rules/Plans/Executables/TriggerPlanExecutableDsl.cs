using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Registry;
using System.Linq;
using AbilityKit.Triggering.Runtime.Config;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Variables.Numeric;

namespace AbilityKit.Triggering.Runtime.Plan
{
    /// <summary>
    /// TriggerPlan 主线执行节点 DSL。
    /// 用于替代 Runtime/Executable 中的旧行为构建入口。
    /// </summary>
    public static class TriggerPlanExecutableDsl
    {
        public static ActionCallTriggerPlanExecutable Action(ActionCallPlan action, ITriggerPlanCondition condition = null, float weight = 1f)
        {
            if (action.Equals(default(ActionCallPlan)))
                throw new ArgumentException("ActionCallPlan cannot be empty.", nameof(action));

            return new ActionCallTriggerPlanExecutable(action, condition: condition, weight: weight);
        }

        public static ActionCallTriggerPlanExecutable Action(ActionId actionId, ITriggerPlanCondition condition = null, float weight = 1f)
            => Action(new ActionCallPlan(actionId), condition, weight);

        public static ActionCallTriggerPlanExecutable Action(ActionId actionId, NumericValueRef arg0, ITriggerPlanCondition condition = null, float weight = 1f)
            => Action(new ActionCallPlan(actionId, arg0), condition, weight);

        public static ActionCallTriggerPlanExecutable Action(ActionId actionId, NumericValueRef arg0, NumericValueRef arg1, ITriggerPlanCondition condition = null, float weight = 1f)
            => Action(new ActionCallPlan(actionId, arg0, arg1), condition, weight);

        public static ActionCallTriggerPlanExecutable Action(ActionId actionId, double arg0, ITriggerPlanCondition condition = null, float weight = 1f)
            => Action(new ActionCallPlan(actionId, arg0), condition, weight);

        public static ActionCallTriggerPlanExecutable Action(ActionId actionId, double arg0, double arg1, ITriggerPlanCondition condition = null, float weight = 1f)
            => Action(new ActionCallPlan(actionId, arg0, arg1), condition, weight);

        public static ActionCallTriggerPlanExecutable ActionArgs(ActionId actionId, Dictionary<string, ActionArgValue> args, ITriggerPlanCondition condition = null, float weight = 1f)
            => Action(ActionCallPlan.WithArgs(actionId, args), condition, weight);

        public static SequenceTriggerPlanExecutable Sequence(params ITriggerPlanExecutable[] children)
            => Sequence(children, null, 1f);

        public static SequenceTriggerPlanExecutable Sequence(ITriggerPlanExecutable[] children, ITriggerPlanCondition condition = null, float weight = 1f)
            => new SequenceTriggerPlanExecutable(children ?? Array.Empty<ITriggerPlanExecutable>(), condition, weight);

        public static SelectorTriggerPlanExecutable Selector(params ITriggerPlanExecutable[] children)
            => Selector(children, null, 1f);

        public static SelectorTriggerPlanExecutable Selector(ITriggerPlanExecutable[] children, ITriggerPlanCondition condition = null, float weight = 1f)
            => new SelectorTriggerPlanExecutable(children ?? Array.Empty<ITriggerPlanExecutable>(), condition, weight);

        public static ParallelTriggerPlanExecutable Parallel(params ITriggerPlanExecutable[] children)
            => Parallel(children, null, 1f);

        public static ParallelTriggerPlanExecutable Parallel(ITriggerPlanExecutable[] children, ITriggerPlanCondition condition = null, float weight = 1f)
            => new ParallelTriggerPlanExecutable(children ?? Array.Empty<ITriggerPlanExecutable>(), condition, weight);

        public static RandomTriggerPlanExecutable Random(params ITriggerPlanExecutable[] children)
            => Random(children, null, 1f);

        public static RandomTriggerPlanExecutable Random(ITriggerPlanExecutable[] children, ITriggerPlanCondition condition = null, float weight = 1f)
            => new RandomTriggerPlanExecutable(children ?? Array.Empty<ITriggerPlanExecutable>(), condition, weight);

        public static RandomTriggerPlanExecutable RandomSelector(params ITriggerPlanExecutable[] children)
            => Random(children);

        public static RandomTriggerPlanExecutable RandomSelector(ITriggerPlanExecutable[] children, ITriggerPlanCondition condition = null, float weight = 1f)
            => Random(children, condition, weight);

        public static ITriggerPlanCondition Condition(PredicateExprPlan predicate)
            => new PredicateExprTriggerPlanCondition(predicate);

        public static ITriggerPlanCondition Condition(PredicateExprBuilder builder)
            => builder == null ? null : Condition(builder.Build());

        public static ITriggerPlanCondition ConstCondition(bool value)
            => Condition(PredicateExprDsl.Const(value));

        public static ITriggerPlanCondition CompareCondition(ECompareOp op, NumericValueRef left, NumericValueRef right)
            => Condition(PredicateExprDsl.Compare(op, left, right));

        public static ITriggerPlanCondition EqCondition(NumericValueRef left, NumericValueRef right)
            => CompareCondition(ECompareOp.Equal, left, right);

        public static ITriggerPlanCondition NeCondition(NumericValueRef left, NumericValueRef right)
            => CompareCondition(ECompareOp.NotEqual, left, right);

        public static ITriggerPlanCondition GtCondition(NumericValueRef left, NumericValueRef right)
            => CompareCondition(ECompareOp.GreaterThan, left, right);

        public static ITriggerPlanCondition GeCondition(NumericValueRef left, NumericValueRef right)
            => CompareCondition(ECompareOp.GreaterThanOrEqual, left, right);

        public static ITriggerPlanCondition LtCondition(NumericValueRef left, NumericValueRef right)
            => CompareCondition(ECompareOp.LessThan, left, right);

        public static ITriggerPlanCondition LeCondition(NumericValueRef left, NumericValueRef right)
            => CompareCondition(ECompareOp.LessThanOrEqual, left, right);

        public static RepeatTriggerPlanExecutable Repeat(ITriggerPlanExecutable child, int count, ITriggerPlanCondition condition = null, float weight = 1f)
            => new RepeatTriggerPlanExecutable(child, count, condition, weight);

        public static UntilTriggerPlanExecutable Until(ITriggerPlanExecutable child, ITriggerPlanCondition untilCondition, int maxIterations, ITriggerPlanCondition guardCondition = null, float weight = 1f)
            => new UntilTriggerPlanExecutable(child, untilCondition, maxIterations, guardCondition, weight);

        public static IfTriggerPlanExecutable If(ITriggerPlanCondition branchCondition, ITriggerPlanExecutable thenBranch, ITriggerPlanExecutable elseBranch = null, ITriggerPlanCondition guardCondition = null, float weight = 1f)
            => new IfTriggerPlanExecutable(branchCondition, thenBranch, elseBranch, guardCondition, weight);

        public static SucceedTriggerPlanExecutable Succeed(ITriggerPlanExecutable child = null, ITriggerPlanCondition condition = null, float weight = 1f)
            => new SucceedTriggerPlanExecutable(child, condition, weight);

        public static SucceedTriggerPlanExecutable Success(ITriggerPlanExecutable child = null, ITriggerPlanCondition condition = null, float weight = 1f)
            => Succeed(child, condition, weight);

        public static SucceedTriggerPlanExecutable AlwaysSuccess(ITriggerPlanExecutable child = null, ITriggerPlanCondition condition = null, float weight = 1f)
            => Succeed(child, condition, weight);

        public static SucceedTriggerPlanExecutable NoOp(ITriggerPlanCondition condition = null, float weight = 1f)
            => Succeed(null, condition, weight);

        public static FailTriggerPlanExecutable Fail(string reason = null, ITriggerPlanExecutable child = null, ITriggerPlanCondition condition = null, float weight = 1f)
            => new FailTriggerPlanExecutable(child, reason, condition, weight);

        public static FailTriggerPlanExecutable Failure(string reason = null, ITriggerPlanExecutable child = null, ITriggerPlanCondition condition = null, float weight = 1f)
            => Fail(reason, child, condition, weight);

        public static FailTriggerPlanExecutable AlwaysFail(string reason = null, ITriggerPlanExecutable child = null, ITriggerPlanCondition condition = null, float weight = 1f)
            => Fail(reason, child, condition, weight);

        public static InvertTriggerPlanExecutable Invert(ITriggerPlanExecutable child, ITriggerPlanCondition condition = null, float weight = 1f)
            => new InvertTriggerPlanExecutable(child, condition, weight);

        public static InvertTriggerPlanExecutable Not(ITriggerPlanExecutable child, ITriggerPlanCondition condition = null, float weight = 1f)
            => Invert(child, condition, weight);

        public static ScheduledTriggerPlanExecutable Scheduled(
            ITriggerPlanExecutable child,
            EScheduleMode scheduleMode,
            float intervalMs = 0f,
            int maxExecutions = -1,
            bool canBeInterrupted = true,
            ITriggerPlanCondition condition = null,
            float weight = 1f)
            => new ScheduledTriggerPlanExecutable(child, scheduleMode, intervalMs, maxExecutions, canBeInterrupted, condition, weight);

        public static ScheduledTriggerPlanExecutable Timed(ITriggerPlanExecutable child, float delayMs, ITriggerPlanCondition condition = null, float weight = 1f)
            => Scheduled(child, EScheduleMode.Timed, delayMs, 1, true, condition, weight);

        public static ScheduledTriggerPlanExecutable Periodic(ITriggerPlanExecutable child, float intervalMs, int maxExecutions = -1, bool canBeInterrupted = true, ITriggerPlanCondition condition = null, float weight = 1f)
            => Scheduled(child, EScheduleMode.Periodic, intervalMs, maxExecutions, canBeInterrupted, condition, weight);

        public static ScheduledTriggerPlanExecutable Continuous(ITriggerPlanExecutable child, float intervalMs = 0f, int maxExecutions = -1, bool canBeInterrupted = true, ITriggerPlanCondition condition = null, float weight = 1f)
            => Scheduled(child, EScheduleMode.Continuous, intervalMs, maxExecutions, canBeInterrupted, condition, weight);

        public static ScheduledTriggerPlanExecutable External(ITriggerPlanExecutable child, ITriggerPlanCondition condition = null, float weight = 1f)
            => Scheduled(child, EScheduleMode.External, 0f, -1, true, condition, weight);

        public static MetadataTriggerPlanExecutable Metadata(
            ITriggerPlanExecutable child,
            ETriggerPlanMetadataKind metadataKind,
            IReadOnlyDictionary<string, string> values = null,
            ITriggerPlanCondition condition = null,
            float weight = 1f)
            => new MetadataTriggerPlanExecutable(child, metadataKind, values, condition, weight);

        public static MetadataTriggerPlanExecutable Tags(ITriggerPlanExecutable child, params string[] tags)
            => Metadata(child, ETriggerPlanMetadataKind.Tags, ToValues("tag", tags));

        public static MetadataTriggerPlanExecutable Stack(ITriggerPlanExecutable child, int initialStack = 1, float stackMultiplier = 1f)
            => Metadata(child, ETriggerPlanMetadataKind.Stack, new Dictionary<string, string>
            {
                ["initialStack"] = Math.Max(1, initialStack).ToString(),
                ["stackMultiplier"] = stackMultiplier.ToString(System.Globalization.CultureInfo.InvariantCulture),
            });

        public static MetadataTriggerPlanExecutable Hierarchy(ITriggerPlanExecutable child, int? parentId = null)
            => Metadata(child, ETriggerPlanMetadataKind.Hierarchy, parentId.HasValue
                ? new Dictionary<string, string> { ["parentId"] = parentId.Value.ToString() }
                : null);

        public static MetadataTriggerPlanExecutable Capability(ITriggerPlanExecutable child, string capabilityNamespace, string capabilityName)
            => Metadata(child, ETriggerPlanMetadataKind.Capability, new Dictionary<string, string>
            {
                ["namespace"] = capabilityNamespace ?? string.Empty,
                ["name"] = capabilityName ?? string.Empty,
            });

        public static MetadataTriggerPlanExecutable Duration(ITriggerPlanExecutable child, float durationMs, bool autoStart = true)
            => Metadata(child, ETriggerPlanMetadataKind.Duration, new Dictionary<string, string>
            {
                ["durationMs"] = Math.Max(0f, durationMs).ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["autoStart"] = autoStart ? "true" : "false",
            });

        public static MetadataTriggerPlanExecutable ContinuousMetadata(ITriggerPlanExecutable child, string continuationId = null)
            => Metadata(child, ETriggerPlanMetadataKind.Continuous, string.IsNullOrEmpty(continuationId)
                ? null
                : new Dictionary<string, string> { ["continuationId"] = continuationId });

        public static MetadataTriggerPlanExecutable Modifiers(ITriggerPlanExecutable child, params string[] modifierIds)
            => Metadata(child, ETriggerPlanMetadataKind.Modifiers, ToValues("modifier", modifierIds));

        private static Dictionary<string, string> ToValues(string prefix, string[] values)
        {
            if (values == null || values.Length == 0)
                return null;

            return values
                .Where(value => !string.IsNullOrEmpty(value))
                .Select((value, index) => new { Key = prefix + index, Value = value })
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}

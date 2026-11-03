namespace AbilityKit.Triggering.Runtime.RuleScheduler
{
    /// <summary>
    /// 默认规则调度驱动内部使用的可变运行时条目。
    /// </summary>
    internal sealed class RuleScheduleEntry
    {
        public readonly RuleScheduleHandle Handle;
        public readonly RuleSchedulePlan Plan;
        public readonly IRuleScheduleEffect Effect;
        public ERuleScheduleState State;
        public float ElapsedMs;
        public float LastExecuteMs;
        public int OccurrenceCount;
        public string InterruptReason;

        public RuleScheduleEntry(RuleScheduleHandle handle, RuleSchedulePlan plan, IRuleScheduleEffect effect, ERuleScheduleState state)
        {
            Handle = handle;
            Plan = plan;
            Effect = effect;
            State = state;
        }

        public bool IsTerminal => State == ERuleScheduleState.Completed || State == ERuleScheduleState.Interrupted || State == ERuleScheduleState.Cancelled;

        public RuleScheduleSnapshot CreateSnapshot()
        {
            return new RuleScheduleSnapshot(Handle, Plan, State, ElapsedMs, LastExecuteMs, OccurrenceCount, InterruptReason);
        }

        public RuleScheduleContext CreateContext(float deltaTimeMs, float scaledDeltaMs, object userContext)
        {
            return new RuleScheduleContext(Handle, Plan, deltaTimeMs, scaledDeltaMs, ElapsedMs, OccurrenceCount, userContext);
        }
    }
}

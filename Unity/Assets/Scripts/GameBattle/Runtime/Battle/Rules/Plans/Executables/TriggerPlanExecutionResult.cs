namespace AbilityKit.Triggering.Runtime.Plan
{
    public readonly struct TriggerPlanExecutionResult
    {
        public readonly ETriggerPlanExecutionStatus Status;
        public readonly int ExecutedCount;
        public readonly string Reason;

        public bool IsSuccess => Status == ETriggerPlanExecutionStatus.Success;
        public bool IsSkipped => Status == ETriggerPlanExecutionStatus.Skipped;
        public bool IsFailed => Status == ETriggerPlanExecutionStatus.Failed;

        public static TriggerPlanExecutionResult Success(int executedCount = 1)
            => new TriggerPlanExecutionResult(ETriggerPlanExecutionStatus.Success, executedCount, null);

        public static TriggerPlanExecutionResult Skipped(string reason = null)
            => new TriggerPlanExecutionResult(ETriggerPlanExecutionStatus.Skipped, 0, reason);

        public static TriggerPlanExecutionResult Failed(string reason)
            => new TriggerPlanExecutionResult(ETriggerPlanExecutionStatus.Failed, 0, reason);

        public static TriggerPlanExecutionResult None => Success(0);

        private TriggerPlanExecutionResult(ETriggerPlanExecutionStatus status, int executedCount, string reason)
        {
            Status = status;
            ExecutedCount = executedCount;
            Reason = reason;
        }

        public TriggerPlanExecutionResult Merge(TriggerPlanExecutionResult other)
        {
            if (IsFailed) return this;
            if (other.IsFailed) return other;
            if (IsSkipped && other.IsSkipped) return other;
            if (IsSkipped) return other;
            if (other.IsSkipped) return this;
            return Success(ExecutedCount + other.ExecutedCount);
        }
    }
}

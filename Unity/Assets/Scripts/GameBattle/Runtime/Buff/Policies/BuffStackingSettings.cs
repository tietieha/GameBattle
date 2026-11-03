namespace AbilityKit.Buffs
{
    public enum BuffStackingPolicy
    {
        None = 0,
        IgnoreIfExists = 1,
        Replace = 2,
        AddStack = 3,
        RefreshDuration = 4,
    }

    public enum BuffRefreshPolicy
    {
        None = 0,
        KeepRemaining = 1,
        ResetRemaining = 2,
        AddRemaining = 3,
    }

    public readonly struct BuffStackingSettings
    {
        public BuffStackingSettings(BuffStackingPolicy stackingPolicy, BuffRefreshPolicy refreshPolicy,
            int maxStacks, int intervalMs)
        {
            StackingPolicy = stackingPolicy;
            RefreshPolicy = refreshPolicy;
            MaxStacks = maxStacks;
            IntervalMs = intervalMs;
        }

        public BuffStackingPolicy StackingPolicy { get; }
        public BuffRefreshPolicy RefreshPolicy { get; }
        public int MaxStacks { get; }
        public int IntervalMs { get; }
    }

    public enum BuffStackingApplyOutcome
    {
        Ignored = 0,
        Replaced = 1,
        StackAdded = 2,
        DurationRefreshed = 3,
    }

    public readonly struct BuffStackingApplyResult
    {
        public static readonly BuffStackingApplyResult Ignored =
            new BuffStackingApplyResult(false, BuffStackingApplyOutcome.Ignored, string.Empty);

        public BuffStackingApplyResult(bool applied, BuffStackingApplyOutcome outcome, string rejectCode)
        {
            Applied = applied;
            Outcome = outcome;
            RejectCode = rejectCode;
        }

        public bool Applied { get; }
        public BuffStackingApplyOutcome Outcome { get; }
        public string RejectCode { get; }
        public bool IsReplace => Outcome == BuffStackingApplyOutcome.Replaced;
        public bool ShouldResetInterval => Applied;

        public static BuffStackingApplyResult AppliedResult(BuffStackingApplyOutcome outcome) =>
            new BuffStackingApplyResult(true, outcome, string.Empty);
    }
}

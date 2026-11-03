namespace AbilityKit.Buffs
{
    /// <summary>Changes only shared state, never executes effects, ticks, or registers an owner.</summary>
    public sealed class BuffStackingPolicyApplier
    {
        public BuffStackingApplyResult ApplyToExisting(BuffState existing, in BuffStackingSettings settings,
            int sourceActorId, float durationSeconds)
        {
            if (existing == null) return BuffStackingApplyResult.Ignored;
            switch (settings.StackingPolicy)
            {
                case BuffStackingPolicy.Replace:
                    existing.SourceId = sourceActorId;
                    existing.StackCount = 0;
                    existing.Remaining = durationSeconds;
                    AddStack(existing, settings.MaxStacks);
                    return BuffStackingApplyResult.AppliedResult(BuffStackingApplyOutcome.Replaced);
                case BuffStackingPolicy.AddStack:
                    AddStack(existing, settings.MaxStacks);
                    RefreshRemaining(existing, settings.RefreshPolicy, durationSeconds);
                    existing.SourceId = sourceActorId;
                    return BuffStackingApplyResult.AppliedResult(BuffStackingApplyOutcome.StackAdded);
                case BuffStackingPolicy.RefreshDuration:
                    RefreshRemaining(existing, settings.RefreshPolicy, durationSeconds);
                    existing.SourceId = sourceActorId;
                    return BuffStackingApplyResult.AppliedResult(BuffStackingApplyOutcome.DurationRefreshed);
                default:
                    return BuffStackingApplyResult.Ignored;
            }
        }

        // The caller can rent a derived instance; allocation and registration are not policy responsibilities.
        public static void Initialize(BuffState state, int buffId, in BuffStackingSettings settings,
            int sourceActorId, float durationSeconds)
        {
            if (state == null) throw new System.ArgumentNullException(nameof(state));
            state.BuffId = buffId;
            state.Remaining = durationSeconds;
            state.SourceId = sourceActorId;
            state.StackCount = 0;
            state.SourceContextId = 0;
            state.ClearRuntimeContext();
            AddStack(state, settings.MaxStacks);
            ResetInterval(state, in settings);
        }

        public static void ResetInterval(BuffState state, in BuffStackingSettings settings)
        {
            if (state == null) return;
            state.IntervalRemainingSeconds = settings.IntervalMs > 0 ? settings.IntervalMs / 1000f : 0f;
        }

        private static void RefreshRemaining(BuffState state, BuffRefreshPolicy policy, float durationSeconds)
        {
            switch (policy)
            {
                case BuffRefreshPolicy.ResetRemaining:
                    state.Remaining = durationSeconds;
                    break;
                case BuffRefreshPolicy.AddRemaining:
                    state.Remaining += durationSeconds;
                    break;
            }
        }

        private static void AddStack(BuffState state, int maxStacks)
        {
            if (maxStacks <= 0) maxStacks = int.MaxValue;
            if (state.StackCount < maxStacks) state.StackCount++;
        }
    }
}

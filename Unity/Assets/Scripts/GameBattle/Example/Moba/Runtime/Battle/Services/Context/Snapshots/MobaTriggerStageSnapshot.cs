namespace AbilityKit.Demo.Moba.Services
{
    public interface IMobaTriggerStageSnapshotProvider
    {
        bool TryGetStageSnapshot(out MobaTriggerStageSnapshot snapshot);
    }

    public readonly struct MobaTriggerStageSnapshot
    {
        public MobaTriggerStageSnapshot(
            int stackCount = 0,
            float elapsedSeconds = 0f,
            float remainingSeconds = 0f,
            float durationSeconds = 0f)
        {
            StackCount = stackCount;
            ElapsedSeconds = elapsedSeconds;
            RemainingSeconds = remainingSeconds;
            DurationSeconds = durationSeconds;
        }

        public int StackCount { get; }
        public float ElapsedSeconds { get; }
        public float RemainingSeconds { get; }
        public float DurationSeconds { get; }

        public bool IsValid => StackCount != 0 || ElapsedSeconds != 0f || RemainingSeconds != 0f || DurationSeconds != 0f;
    }

    public static class MobaTriggerStageSnapshotResolver
    {
        public static bool TryResolve(object payload, out MobaTriggerStageSnapshot snapshot)
        {
            snapshot = default;
            return payload is IMobaTriggerStageSnapshotProvider provider
                   && provider.TryGetStageSnapshot(out snapshot)
                   && snapshot.IsValid;
        }
    }
}

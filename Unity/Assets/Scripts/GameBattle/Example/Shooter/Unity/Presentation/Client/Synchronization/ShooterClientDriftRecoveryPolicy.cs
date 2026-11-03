#nullable enable

using System;

namespace AbilityKit.Demo.Shooter.View
{
    public readonly struct ShooterClientDriftRecoveryPolicy
    {
        public static ShooterClientDriftRecoveryPolicy Default => new ShooterClientDriftRecoveryPolicy(
            smallCatchUpThreshold: 8,
            replayThreshold: 120,
            maxCatchUpTicksPerUpdate: 4,
            snapshotTimeoutTicks: TimeSpan.TicksPerSecond * 2);

        public readonly int SmallCatchUpThreshold;
        public readonly int ReplayThreshold;
        public readonly int MaxCatchUpTicksPerUpdate;
        public readonly long SnapshotTimeoutTicks;

        public ShooterClientDriftRecoveryPolicy(
            int smallCatchUpThreshold,
            int replayThreshold,
            int maxCatchUpTicksPerUpdate,
            long snapshotTimeoutTicks)
        {
            if (smallCatchUpThreshold < 0) throw new ArgumentOutOfRangeException(nameof(smallCatchUpThreshold));
            if (replayThreshold < 0) throw new ArgumentOutOfRangeException(nameof(replayThreshold));
            if (maxCatchUpTicksPerUpdate <= 0) throw new ArgumentOutOfRangeException(nameof(maxCatchUpTicksPerUpdate));
            if (snapshotTimeoutTicks < 0L) throw new ArgumentOutOfRangeException(nameof(snapshotTimeoutTicks));

            SmallCatchUpThreshold = smallCatchUpThreshold;
            ReplayThreshold = replayThreshold;
            MaxCatchUpTicksPerUpdate = maxCatchUpTicksPerUpdate;
            SnapshotTimeoutTicks = snapshotTimeoutTicks;
        }
    }

    public enum ShooterClientRecoveryState
    {
        Normal = 0,
        CatchUp = 1,
        AwaitingFullSnapshot = 2,
        ApplyingFullSnapshot = 3,
        Recovered = 4
    }
}

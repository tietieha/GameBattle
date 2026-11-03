#nullable enable

namespace AbilityKit.Demo.Shooter.View
{
    public sealed class ShooterNullSnapshotViewSink : IShooterSnapshotViewSink
    {
        public static readonly ShooterNullSnapshotViewSink Instance = new ShooterNullSnapshotViewSink();

        private ShooterNullSnapshotViewSink()
        {
        }

        public void ApplySnapshot(in ShooterSnapshotViewBatch batch)
        {
        }

        public void Clear()
        {
        }
    }
}

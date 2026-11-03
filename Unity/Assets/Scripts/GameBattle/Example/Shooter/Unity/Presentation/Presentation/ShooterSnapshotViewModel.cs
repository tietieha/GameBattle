using AbilityKit.Game.View.Presentation;

namespace AbilityKit.Demo.Shooter.View
{
    public sealed class ShooterSnapshotViewModel : IViewModelStore<ShooterSnapshotViewBatch>
    {
        private ShooterSnapshotViewBatch _current = ShooterSnapshotViewBatch.Empty;

        public ulong WorldId => _current.WorldId;

        public int Frame => _current.Frame;

        public ulong Sequence => _current.Sequence;

        public ShooterViewSnapshotKind SnapshotKind => _current.SnapshotKind;

        public ShooterSnapshotViewBatch Current => _current;

        public void Apply(in ShooterSnapshotViewBatch batch)
        {
            _current = batch;
        }

        public void Clear()
        {
            _current = ShooterSnapshotViewBatch.Empty;
        }
    }
}

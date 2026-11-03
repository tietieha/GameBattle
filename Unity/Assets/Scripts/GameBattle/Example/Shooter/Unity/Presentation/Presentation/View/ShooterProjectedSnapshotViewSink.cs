#nullable enable

using System;

namespace AbilityKit.Demo.Shooter.View
{
    public sealed class ShooterProjectedSnapshotViewSink : IShooterSnapshotViewSink
    {
        private readonly ShooterSnapshotViewProjection _projection;
        private readonly IShooterProjectedViewSink _sink;

        public ShooterProjectedSnapshotViewSink(IShooterProjectedViewSink sink)
            : this(new ShooterSnapshotViewProjection(), sink)
        {
        }

        public ShooterProjectedSnapshotViewSink(ShooterSnapshotViewProjection projection, IShooterProjectedViewSink sink)
        {
            _projection = projection ?? throw new ArgumentNullException(nameof(projection));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        }

        public ShooterSnapshotViewProjection Projection => _projection;

        public ShooterViewEntityStore Store => _projection.Store;

        public ShooterViewProjectionApplyResult LastApplyResult => _projection.LastApplyResult;

        public void ApplySnapshot(in ShooterSnapshotViewBatch batch)
        {
            var result = _projection.Apply(in batch);
            _sink.ApplyViewState(_projection.Store, in batch, in result);
        }

        public void Clear()
        {
            _projection.Clear();
            _sink.Clear();
        }
    }

    public interface IShooterProjectedViewSink
    {
        void ApplyViewState(
            ShooterViewEntityStore store,
            in ShooterSnapshotViewBatch sourceBatch,
            in ShooterViewProjectionApplyResult applyResult);

        void Clear();
    }
}

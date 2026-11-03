#nullable enable

using AbilityKit.Game.View.Presentation;

namespace AbilityKit.Demo.Shooter.View
{
    public interface IShooterSnapshotViewSink : IViewSink<ShooterSnapshotViewBatch>
    {
        void ApplySnapshot(in ShooterSnapshotViewBatch batch);

        new void Clear();

        // 将框架的 ApplyBatch 契约桥接到 Shooter 专用的 ApplySnapshot 命名上，
        // 这样现有 sink 无需重命名就能满足 IViewSink<ShooterSnapshotViewBatch>。
        void IViewSink<ShooterSnapshotViewBatch>.ApplyBatch(in ShooterSnapshotViewBatch batch)
        {
            ApplySnapshot(in batch);
        }
    }
}

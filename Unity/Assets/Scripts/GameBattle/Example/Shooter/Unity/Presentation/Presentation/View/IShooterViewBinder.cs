using AbilityKit.Game.View.Presentation;

namespace AbilityKit.Demo.Shooter.View
{
    public interface IShooterViewBinder : IViewBinder<ShooterSnapshotViewBatch>
    {
        new bool InterpolationEnabled { get; set; }
        void Sync(in ShooterSnapshotViewBatch batch);
        new void TickInterpolation(float deltaTime);
        new void Clear();
        new void RebindAll();

        // 将框架的 ApplyBatch 契约桥接到 Shooter 专用的 Sync 命名上。
        void IViewSink<ShooterSnapshotViewBatch>.ApplyBatch(in ShooterSnapshotViewBatch batch)
        {
            Sync(in batch);
        }
    }
}
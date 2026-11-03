namespace AbilityKit.Demo.Shooter.View.EntityViewModel
{
    public interface IShooterEntityQuery
    {
        bool TryResolve(uint entityId, out object entity);
        bool TryGetPlayer(uint entityId, out ShooterPlayerEntity player);
        bool TryGetBullet(uint entityId, out ShooterBulletEntity bullet);
        bool TryGetTransform(uint entityId, out ShooterTransformComponent transform);
    }
}
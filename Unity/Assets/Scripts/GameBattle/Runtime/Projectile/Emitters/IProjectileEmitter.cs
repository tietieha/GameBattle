using AbilityKit.Core.Mathematics;

namespace AbilityKit.Combat.Projectile
{
    public interface IProjectileEmitter
    {
        ProjectileId Emit(in ProjectileSpawnParams spawn);
    }
}

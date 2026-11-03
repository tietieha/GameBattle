#if false
using AbilityKit.Core.Mathematics;
using AbilityKit.Ability.World.Services;

namespace AbilityKit.Combat.Projectile
{
    public interface IProjectileReturnTargetProvider : IService
    {
        bool TryGetReturnTargetPosition(int launcherActorId, out Vec3 position);
    }
}
#endif

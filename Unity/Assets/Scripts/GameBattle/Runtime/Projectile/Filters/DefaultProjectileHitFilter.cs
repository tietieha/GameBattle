using AbilityKit.Core.Mathematics;

namespace AbilityKit.Combat.Projectile
{
    public sealed class DefaultProjectileHitFilter : IProjectileHitFilter
    {
        public static readonly DefaultProjectileHitFilter Instance = new DefaultProjectileHitFilter();

        private DefaultProjectileHitFilter() { }

        public bool ShouldHit(int ownerId, ColliderId collider, int frame)
        {
            return collider.Value != 0;
        }
    }
}

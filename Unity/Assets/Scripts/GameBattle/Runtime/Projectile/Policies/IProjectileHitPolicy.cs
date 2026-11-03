namespace AbilityKit.Combat.Projectile
{
    public interface IProjectileHitPolicy
    {
        bool ShouldExitOnHit(in ProjectileHitEvent hit, ref int hitsRemaining);
    }
}

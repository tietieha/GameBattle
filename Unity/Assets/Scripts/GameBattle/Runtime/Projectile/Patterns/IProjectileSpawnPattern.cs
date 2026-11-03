using System.Collections.Generic;

namespace AbilityKit.Combat.Projectile
{
    public interface IProjectileSpawnPattern
    {
        void Build(in ProjectileSpawnParams baseSpawn, List<ProjectileSpawnParams> results);
    }

    public interface IProjectileSpawnPatternProvider
    {
        IProjectileSpawnPattern GetPattern(in ProjectileSpawnParams baseSpawn, int frame);
    }
}

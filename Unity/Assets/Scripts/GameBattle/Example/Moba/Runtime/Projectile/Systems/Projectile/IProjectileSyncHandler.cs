using System.Collections.Generic;
using AbilityKit.Combat.Projectile;

namespace AbilityKit.Demo.Moba.Runtime.Application.Systems.Projectile
{
    internal interface IProjectileSyncHandler
    {
        void HandleSpawns(List<ProjectileSpawnEvent> spawns);
        void HandleTicks(List<ProjectileTickEvent> ticks);
        void HandleExits(List<ProjectileExitEvent> exits);
        void HandleHits(List<ProjectileHitEvent> hits);
    }
}

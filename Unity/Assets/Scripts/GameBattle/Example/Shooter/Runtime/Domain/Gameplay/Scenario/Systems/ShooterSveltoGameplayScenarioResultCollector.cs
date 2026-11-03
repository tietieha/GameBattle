#nullable enable

using System;
using AbilityKit.World.Svelto;
using Svelto.DataStructures;
using Svelto.ECS;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal readonly struct ShooterSveltoGameplayScenarioCounters
    {
        public ShooterSveltoGameplayScenarioCounters(
            int projectilesSpawned,
            int projectilesExpired,
            int hits,
            int defeatedTargets,
            int enemyHits)
        {
            ProjectilesSpawned = projectilesSpawned;
            ProjectilesExpired = projectilesExpired;
            Hits = hits;
            DefeatedTargets = defeatedTargets;
            EnemyHits = enemyHits;
        }

        public int ProjectilesSpawned { get; }
        public int ProjectilesExpired { get; }
        public int Hits { get; }
        public int DefeatedTargets { get; }
        public int EnemyHits { get; }
    }

    internal sealed class ShooterSveltoGameplayScenarioResultCollector
    {
        private readonly ISveltoWorldContext _context;

        public ShooterSveltoGameplayScenarioResultCollector(ISveltoWorldContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ShooterSveltoGameplayScenarioResult BuildResult(
            in ShooterSveltoGameplayScenarioConfig config,
            in ShooterSveltoGameplayScenarioCounters counters)
        {
            var remainingHp = 0;
            var healthCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayTargets);
            healthCollection.Deconstruct(out NB<ShooterSveltoHealthComponent> healths, out _, out var targetCount);
            for (var i = 0; i < targetCount; i++)
            {
                remainingHp += healths[i].Current;
            }

            var activeProjectiles = _context.EntitiesDB.Count<ShooterSveltoProjectileComponent>(ShooterSveltoGroups.GameplayProjectiles);
            return new ShooterSveltoGameplayScenarioResult(
                config.Id,
                config.TickCount,
                config.ShooterCount,
                targetCount,
                counters.ProjectilesSpawned,
                counters.ProjectilesExpired,
                counters.Hits,
                counters.DefeatedTargets,
                activeProjectiles,
                remainingHp,
                counters.EnemyHits,
                ComputeStateHash());
        }

        private uint ComputeStateHash()
        {
            var hash = 2166136261u;
            var targetCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoTransformComponent, ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayTargets);
            targetCollection.Deconstruct(out NB<ShooterSveltoTransformComponent> targetTransforms, out NB<ShooterSveltoHealthComponent> targetHealths, out _, out var targetCount);
            for (var i = 0; i < targetCount; i++)
            {
                Mix(ref hash, Quantize(targetTransforms[i].X));
                Mix(ref hash, Quantize(targetTransforms[i].Y));
                Mix(ref hash, targetHealths[i].Current);
                Mix(ref hash, targetHealths[i].Alive);
            }

            var shooterCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayShooters);
            shooterCollection.Deconstruct(out NB<ShooterSveltoHealthComponent> shooterHealths, out _, out var shooterCount);
            for (var i = 0; i < shooterCount; i++)
            {
                Mix(ref hash, shooterHealths[i].Current);
                Mix(ref hash, shooterHealths[i].Alive);
            }

            var projectileCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoTransformComponent, ShooterSveltoProjectileComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayProjectiles);
            projectileCollection.Deconstruct(out NB<ShooterSveltoTransformComponent> projectileTransforms, out NB<ShooterSveltoProjectileComponent> projectileComponents, out _, out var projectileCount);
            for (var i = 0; i < projectileCount; i++)
            {
                Mix(ref hash, Quantize(projectileTransforms[i].X));
                Mix(ref hash, Quantize(projectileTransforms[i].Y));
                Mix(ref hash, projectileComponents[i].RemainingFrames);
            }

            return hash;
        }

        private static void Mix(ref uint hash, int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 16777619u;
            }
        }

        private static int Quantize(float value)
        {
            return (int)MathF.Round(value * 1000f);
        }
    }
}

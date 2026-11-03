#nullable enable

using System;
using System.Collections.Generic;
using AbilityKit.Demo.Shooter.Runtime.Infrastructure.Ecs;
using AbilityKit.World.Svelto;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.Internal;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal sealed class ShooterSveltoGameplayScenarioEnemyDecisionSystem
    {
        private const float Pi = 3.14159265358979323846f;

        private readonly ISveltoWorldContext _context;
        private readonly ShooterSveltoGameplayScenarioProjectileSystem _projectileSystem;
        private readonly Dictionary<uint, int> _shooterIndexByEntityId = new();

        public ShooterSveltoGameplayScenarioEnemyDecisionSystem(
            ISveltoWorldContext context,
            ShooterSveltoGameplayScenarioProjectileSystem projectileSystem)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _projectileSystem = projectileSystem ?? throw new ArgumentNullException(nameof(projectileSystem));
        }

        public void Reset()
        {
            _shooterIndexByEntityId.Clear();
        }

        public void Tick(in ShooterSveltoGameplayScenarioConfig config, int frame)
        {
            var battleFlow = config.BattleFlow;
            if (config.ShooterCount <= 0 || frame % battleFlow.EnemyAttackIntervalFrames != 0)
            {
                return;
            }

            var enemyWeapon = new ShooterSveltoWeaponComponent
            {
                LoadoutId = battleFlow.EnemyLoadoutId,
                ProjectileSpeed = config.Loadout.ProjectileSpeed * battleFlow.EnemyProjectileSpeedScale,
                ProjectileLifeFrames = config.Loadout.ProjectileLifeFrames,
                Damage = battleFlow.EnemyAttackDamage,
                CooldownFrames = battleFlow.EnemyAttackIntervalFrames,
                ProjectilesPerShot = battleFlow.EnemyProjectilesPerShot,
                SpreadRadians = battleFlow.EnemySpreadDegrees * Pi / 180f
            };

            var enemyCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoTransformComponent, ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayTargets);
            enemyCollection.Deconstruct(out NB<ShooterSveltoTransformComponent> enemyTransforms, out NB<ShooterSveltoHealthComponent> enemyHealths, out NativeEntityIDs enemyIds, out var enemyCount);
            var shooterCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoTransformComponent, ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayShooters);
            shooterCollection.Deconstruct(out NB<ShooterSveltoTransformComponent> shooterTransforms, out NB<ShooterSveltoHealthComponent> shooterHealths, out NativeEntityIDs shooterIds, out var shooterCount);
            ShooterSveltoGameplayScenarioEcsUtility.RebuildIndex(_shooterIndexByEntityId, shooterIds, shooterCount);

            var spawned = false;
            for (var i = 0; i < enemyCount; i++)
            {
                if (enemyHealths[i].Alive == 0)
                {
                    continue;
                }

                var shooterId = (uint)((enemyIds[i] % (uint)config.ShooterCount) + 1u);
                if (!ShooterSveltoGameplayScenarioEcsUtility.TryGetLiveTarget(shooterId, shooterTransforms, shooterHealths, shooterCount, _shooterIndexByEntityId, out var shooterTransform))
                {
                    continue;
                }

                var enemyTransform = enemyTransforms[i];
                var dx = shooterTransform.X - enemyTransform.X;
                var dy = shooterTransform.Y - enemyTransform.Y;
                ShooterSveltoGameplayScenarioEcsUtility.Normalize(ref dx, ref dy);
                enemyTransform.DirectionX = dx;
                enemyTransform.DirectionY = dy;
                spawned |= _projectileSystem.FireBurst(in enemyTransform, in enemyWeapon, shooterId, shooterId, ShooterSveltoGameplayFaction.Shooter, config.ShooterCount);
            }

            if (spawned)
            {
                _context.SubmitEntities();
            }
        }
    }
}

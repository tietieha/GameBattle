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
    internal sealed class ShooterSveltoGameplayScenarioShooterDecisionSystem
    {
        private readonly ISveltoWorldContext _context;
        private readonly ShooterSveltoGameplayScenarioProjectileSystem _projectileSystem;
        private readonly Dictionary<uint, int> _targetIndexByEntityId = new();

        public ShooterSveltoGameplayScenarioShooterDecisionSystem(
            ISveltoWorldContext context,
            ShooterSveltoGameplayScenarioProjectileSystem projectileSystem)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _projectileSystem = projectileSystem ?? throw new ArgumentNullException(nameof(projectileSystem));
        }

        public void Reset()
        {
            _targetIndexByEntityId.Clear();
        }

        public void Tick(in ShooterSveltoGameplayScenarioConfig config)
        {
            var shooterCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoTransformComponent, ShooterSveltoWeaponComponent, ShooterSveltoCooldownComponent, ShooterSveltoTargetComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayShooters);
            shooterCollection.Deconstruct(out NB<ShooterSveltoTransformComponent> transforms, out NB<ShooterSveltoWeaponComponent> weapons, out NB<ShooterSveltoCooldownComponent> cooldowns, out NB<ShooterSveltoTargetComponent> targets, out NativeEntityIDs shooterIds, out var count);
            var healthCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayShooters);
            healthCollection.Deconstruct(out NB<ShooterSveltoHealthComponent> healths, out _, out var healthCount);
            var enemyCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoTransformComponent, ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayTargets);
            enemyCollection.Deconstruct(out NB<ShooterSveltoTransformComponent> enemyTransforms, out NB<ShooterSveltoHealthComponent> enemyHealths, out NativeEntityIDs enemyIds, out var enemyCount);
            ShooterSveltoGameplayScenarioEcsUtility.RebuildIndex(_targetIndexByEntityId, enemyIds, enemyCount);

            var spawned = false;
            var shooterCount = Math.Min(count, healthCount);
            for (var i = 0; i < shooterCount; i++)
            {
                if (healths[i].Alive == 0)
                {
                    continue;
                }

                ref var cooldown = ref cooldowns[i];
                if (cooldown.RemainingFrames > 0)
                {
                    cooldown.RemainingFrames--;
                    continue;
                }

                ref var transform = ref transforms[i];
                ref var weapon = ref weapons[i];
                ref var target = ref targets[i];
                var targetId = AcquireLiveEnemyTarget(target.TargetEntityId, enemyHealths, enemyIds, enemyCount, _targetIndexByEntityId);
                if (targetId == 0 || !ShooterSveltoGameplayScenarioEcsUtility.TryGetTransform(targetId, enemyTransforms, enemyCount, _targetIndexByEntityId, out var targetTransform))
                {
                    cooldown.RemainingFrames = 1;
                    continue;
                }

                var dx = targetTransform.X - transform.X;
                var dy = targetTransform.Y - transform.Y;
                ShooterSveltoGameplayScenarioEcsUtility.Normalize(ref dx, ref dy);
                transform.DirectionX = dx;
                transform.DirectionY = dy;
                target.TargetEntityId = targetId;

                spawned |= _projectileSystem.FireBurst(in transform, in weapon, shooterIds[i], targetId, ShooterSveltoGameplayFaction.Target, config.TargetCount);
                cooldown.RemainingFrames = weapon.CooldownFrames;
            }

            if (spawned)
            {
                _context.SubmitEntities();
            }
        }

        private static uint AcquireLiveEnemyTarget(uint currentTargetId, NB<ShooterSveltoHealthComponent> healths, NativeEntityIDs ids, int count, Dictionary<uint, int> indexByEntityId)
        {
            if (currentTargetId != 0 && ShooterSveltoGameplayScenarioEcsUtility.IsAlive(currentTargetId, healths, count, indexByEntityId))
            {
                return currentTargetId;
            }

            for (var i = 0; i < count; i++)
            {
                if (healths[i].Alive != 0)
                {
                    return ids[i];
                }
            }

            return 0;
        }
    }
}

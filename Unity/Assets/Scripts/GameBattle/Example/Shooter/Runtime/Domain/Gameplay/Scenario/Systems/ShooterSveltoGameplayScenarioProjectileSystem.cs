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
    internal sealed class ShooterSveltoGameplayScenarioProjectileSystem
    {
        private const float HitRadiusSquared = 0.64f;

        private readonly ISveltoWorldContext _context;
        private readonly Dictionary<uint, int> _targetIndexByEntityId = new();
        private readonly Dictionary<uint, int> _shooterIndexByEntityId = new();
        private readonly List<uint> _projectileRemovalBuffer = new(1024);
        private uint _nextProjectileId;
        private int _projectilesSpawned;
        private int _projectilesExpired;
        private int _hits;
        private int _defeatedTargets;
        private int _enemyHits;

        public ShooterSveltoGameplayScenarioProjectileSystem(ISveltoWorldContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Reset()
        {
            _nextProjectileId = 1;
            _projectilesSpawned = 0;
            _projectilesExpired = 0;
            _hits = 0;
            _defeatedTargets = 0;
            _enemyHits = 0;
            _projectileRemovalBuffer.Clear();
            _targetIndexByEntityId.Clear();
            _shooterIndexByEntityId.Clear();
        }

        public bool FireBurst(
            in ShooterSveltoTransformComponent shooter,
            in ShooterSveltoWeaponComponent weapon,
            uint ownerEntityId,
            uint targetEntityId,
            ShooterSveltoGameplayFaction targetFaction,
            int targetCount)
        {
            var baseX = shooter.DirectionX;
            var baseY = shooter.DirectionY;
            ShooterSveltoGameplayScenarioEcsUtility.Normalize(ref baseX, ref baseY);

            for (var i = 0; i < weapon.ProjectilesPerShot; i++)
            {
                var offset = weapon.ProjectilesPerShot == 1
                    ? 0f
                    : ((float)i / (weapon.ProjectilesPerShot - 1) - 0.5f) * weapon.SpreadRadians;
                var dirX = baseX * MathF.Cos(offset) - baseY * MathF.Sin(offset);
                var dirY = baseX * MathF.Sin(offset) + baseY * MathF.Cos(offset);
                var projectileId = _nextProjectileId++;
                var targetId = targetEntityId == 0 ? (projectileId % (uint)targetCount + 1u) : targetEntityId;
                var initializer = _context.EntityFactory.BuildEntity<ShooterSveltoGameplayProjectileDescriptor>(projectileId, ShooterSveltoGroups.GameplayProjectiles);
                initializer.Init(new ShooterSveltoTransformComponent
                {
                    X = shooter.X,
                    Y = shooter.Y,
                    DirectionX = dirX,
                    DirectionY = dirY
                });
                initializer.Init(new ShooterSveltoProjectileComponent
                {
                    BulletId = (int)projectileId,
                    OwnerPlayerId = 0,
                    X = shooter.X,
                    Y = shooter.Y,
                    VelocityX = dirX * weapon.ProjectileSpeed,
                    VelocityY = dirY * weapon.ProjectileSpeed,
                    RemainingFrames = weapon.ProjectileLifeFrames
                });
                initializer.Init(new ShooterSveltoProjectileDamageComponent
                {
                    Damage = weapon.Damage,
                    OwnerEntityId = ownerEntityId,
                    TargetFaction = targetFaction,
                    TargetEntityId = targetId
                });
                _projectilesSpawned++;
            }

            return weapon.ProjectilesPerShot > 0;
        }

        public void Tick(float deltaTime)
        {
            Tick(deltaTime, 0f);
        }

        public void Tick(float deltaTime, float arenaRadius)
        {
            var projectileCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoTransformComponent, ShooterSveltoProjectileComponent, ShooterSveltoProjectileDamageComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayProjectiles);
            projectileCollection.Deconstruct(out NB<ShooterSveltoTransformComponent> transforms, out NB<ShooterSveltoProjectileComponent> projectiles, out NB<ShooterSveltoProjectileDamageComponent> damageComponents, out NativeEntityIDs ids, out var count);
            var targetCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoTransformComponent, ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayTargets);
            targetCollection.Deconstruct(out NB<ShooterSveltoTransformComponent> targetTransforms, out NB<ShooterSveltoHealthComponent> targetHealths, out NativeEntityIDs targetIds, out var targetCount);
            var shooterCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoTransformComponent, ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayShooters);
            shooterCollection.Deconstruct(out NB<ShooterSveltoTransformComponent> shooterTransforms, out NB<ShooterSveltoHealthComponent> shooterHealths, out NativeEntityIDs shooterIds, out var shooterCount);
            ShooterSveltoGameplayScenarioEcsUtility.RebuildIndex(_targetIndexByEntityId, targetIds, targetCount);
            ShooterSveltoGameplayScenarioEcsUtility.RebuildIndex(_shooterIndexByEntityId, shooterIds, shooterCount);
            _projectileRemovalBuffer.Clear();
            for (var i = count - 1; i >= 0; i--)
            {
                ref var transform = ref transforms[i];
                ref var projectile = ref projectiles[i];
                ref var damage = ref damageComponents[i];
                transform.X += projectile.VelocityX * deltaTime;
                transform.Y += projectile.VelocityY * deltaTime;
                projectile.X = transform.X;
                projectile.Y = transform.Y;
                projectile.RemainingFrames--;

                if (arenaRadius > 0f && transform.X * transform.X + transform.Y * transform.Y > arenaRadius * arenaRadius)
                {
                    _projectileRemovalBuffer.Add(ids[i]);
                    _projectilesExpired++;
                    continue;
                }

                var hit = damage.TargetFaction == ShooterSveltoGameplayFaction.Target
                    ? TryApplyHit(in transform, in damage, ShooterSveltoGameplayFaction.Target, targetTransforms, targetHealths, _targetIndexByEntityId)
                    : TryApplyHit(in transform, in damage, ShooterSveltoGameplayFaction.Shooter, shooterTransforms, shooterHealths, _shooterIndexByEntityId);
                if (hit)
                {
                    _projectileRemovalBuffer.Add(ids[i]);
                    _hits++;
                    continue;
                }

                if (projectile.RemainingFrames <= 0)
                {
                    _projectileRemovalBuffer.Add(ids[i]);
                    _projectilesExpired++;
                }
            }

            FlushProjectileRemovals();
        }

        public ShooterSveltoGameplayScenarioCounters CreateCounters()
        {
            return new ShooterSveltoGameplayScenarioCounters(
                _projectilesSpawned,
                _projectilesExpired,
                _hits,
                _defeatedTargets,
                _enemyHits);
        }

        private bool TryApplyHit(
            in ShooterSveltoTransformComponent projectile,
            in ShooterSveltoProjectileDamageComponent damage,
            ShooterSveltoGameplayFaction targetFaction,
            NB<ShooterSveltoTransformComponent> targetTransforms,
            NB<ShooterSveltoHealthComponent> targetHealths,
            Dictionary<uint, int> targetIndexByEntityId)
        {
            if (damage.TargetEntityId == 0 || !targetIndexByEntityId.TryGetValue(damage.TargetEntityId, out var targetIndex) || targetHealths[targetIndex].Alive == 0)
            {
                return false;
            }

            var dx = targetTransforms[targetIndex].X - projectile.X;
            var dy = targetTransforms[targetIndex].Y - projectile.Y;
            if (dx * dx + dy * dy > HitRadiusSquared)
            {
                return false;
            }

            targetHealths[targetIndex].Current = Math.Max(0, targetHealths[targetIndex].Current - damage.Damage);
            if (targetHealths[targetIndex].Current == 0)
            {
                targetHealths[targetIndex].Alive = 0;
                if (targetFaction == ShooterSveltoGameplayFaction.Target)
                {
                    _defeatedTargets++;
                }
            }

            if (targetFaction == ShooterSveltoGameplayFaction.Shooter)
            {
                _enemyHits++;
            }

            return true;
        }

        private void FlushProjectileRemovals()
        {
            if (_projectileRemovalBuffer.Count == 0)
            {
                return;
            }

            for (var i = 0; i < _projectileRemovalBuffer.Count; i++)
            {
                _context.EntityFunctions.RemoveEntity<ShooterSveltoGameplayProjectileDescriptor>(_projectileRemovalBuffer[i], ShooterSveltoGroups.GameplayProjectiles);
            }

            _context.SubmitEntities();
        }
    }
}

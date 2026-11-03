#nullable enable

using System;
using System.Collections.Generic;
using AbilityKit.Protocol.Shooter;
using AbilityKit.World.Svelto;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.Internal;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal sealed class ShooterPlayerCommandBattleModule
    {
        private const float SpreadProjectileExplosionRadius = 2.4f;
        private const int TwinProjectilePenetrationRemaining = 5;

        private readonly ShooterBattleState _state;
        private readonly IShooterEntityManager _entities;
        private readonly IShooterBattleRules _rules;
        private readonly ShooterCombatEventBuffer _events;
        private readonly ISveltoWorldContext _context;
        private readonly ShooterArenaGameplayOptions _arenaOptions;

        public ShooterPlayerCommandBattleModule(ShooterBattleState state, IShooterEntityManager entities, IShooterBattleRules rules, ShooterCombatEventBuffer events)
            : this(state, entities, rules, events, ShooterArenaGameplayOptions.Disabled)
        {
        }

        public ShooterPlayerCommandBattleModule(ShooterBattleState state, IShooterEntityManager entities, IShooterBattleRules rules, ShooterCombatEventBuffer events, ShooterArenaGameplayOptions arenaOptions)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _entities = entities ?? throw new ArgumentNullException(nameof(entities));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _context = _entities.SveltoContext;
            _arenaOptions = arenaOptions ?? ShooterArenaGameplayOptions.Disabled;
        }

        public void Tick(float deltaTime)
        {
            var playerCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoPlayerComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.Players);
            playerCollection.Deconstruct(out NB<ShooterSveltoPlayerComponent> players, out _, out var count);
            for (var i = 0; i < count; i++)
            {
                if (!players[i].Alive)
                {
                    continue;
                }

                _state.InputBuffer.TryGetLatestCommand(players[i].PlayerId, out var command);
                var moveLength = ShooterBattleMath.Normalize(ref command.MoveX, ref command.MoveY);
                if (moveLength > 0f)
                {
                    players[i].X += command.MoveX * _rules.PlayerSpeed * deltaTime;
                    players[i].Y += command.MoveY * _rules.PlayerSpeed * deltaTime;
                    ShooterCircularArenaMath.Clamp(ref players[i].X, ref players[i].Y, _arenaOptions);
                }

                var aimLength = ShooterBattleMath.Normalize(ref command.AimX, ref command.AimY);
                if (aimLength > 0f)
                {
                    players[i].AimX = command.AimX;
                    players[i].AimY = command.AimY;
                }

                if (command.Fire)
                {
                    SpawnBullets(in players[i], command.AttackSlot);
                    _state.InputBuffer.TryConsumeLatestFire(_state.CurrentFrame, players[i].PlayerId, out _);
                }
            }
        }

        private void SpawnBullets(in ShooterSveltoPlayerComponent player, int attackSlot)
        {
            switch (ShooterPlayerAttackSlots.Normalize(attackSlot))
            {
                case ShooterPlayerAttackSlots.Spread:
                    SpawnBullet(in player, player.AimX, player.AimY, 1f, 1f, explosionRadius: SpreadProjectileExplosionRadius, explosionDamage: _rules.HitDamage);
                    SpawnBullet(in player, RotateX(player.AimX, player.AimY, 0.28f), RotateY(player.AimX, player.AimY, 0.28f), 0.92f, 0.85f, explosionRadius: SpreadProjectileExplosionRadius, explosionDamage: _rules.HitDamage);
                    SpawnBullet(in player, RotateX(player.AimX, player.AimY, -0.28f), RotateY(player.AimX, player.AimY, -0.28f), 0.92f, 0.85f, explosionRadius: SpreadProjectileExplosionRadius, explosionDamage: _rules.HitDamage);
                    return;
                case ShooterPlayerAttackSlots.Twin:
                    SpawnBullet(in player, player.AimX, player.AimY, 1.15f, 0.95f, -0.28f, penetrationRemaining: TwinProjectilePenetrationRemaining);
                    SpawnBullet(in player, player.AimX, player.AimY, 1.15f, 0.95f, 0.28f, penetrationRemaining: TwinProjectilePenetrationRemaining);
                    return;
                default:
                    SpawnBullet(in player, player.AimX, player.AimY, 1f, 1f);
                    return;
            }
        }

        private void SpawnBullet(in ShooterSveltoPlayerComponent player, float directionX, float directionY, float speedScale, float lifeScale, float lateralOffset = 0f, int penetrationRemaining = 0, float explosionRadius = 0f, int explosionDamage = 0)
        {
            ShooterBattleMath.Normalize(ref directionX, ref directionY);
            var sideX = -directionY;
            var sideY = directionX;
            var bulletX = player.X + directionX * 0.5f + sideX * lateralOffset;
            var bulletY = player.Y + directionY * 0.5f + sideY * lateralOffset;
            if (!ShooterCircularArenaMath.IsInside(bulletX, bulletY, _arenaOptions))
            {
                return;
            }

            var bullet = new ShooterSveltoProjectileComponent
            {
                BulletId = _state.AllocateBulletId(),
                OwnerPlayerId = player.PlayerId,
                X = bulletX,
                Y = bulletY,
                VelocityX = directionX * _rules.BulletSpeed * speedScale,
                VelocityY = directionY * _rules.BulletSpeed * speedScale,
                RemainingFrames = Math.Max(1, (int)MathF.Round(_rules.BulletLifeFrames * lifeScale)),
                PenetrationRemaining = Math.Max(0, penetrationRemaining),
                ExplosionRadius = Math.Max(0f, explosionRadius),
                ExplosionDamage = Math.Max(0, explosionDamage)
            };

            _entities.AddProjectile(in bullet);
            _events.AddFire(player.PlayerId, bullet.BulletId, bullet.X, bullet.Y);
        }

        private static float RotateX(float x, float y, float radians)
        {
            return x * MathF.Cos(radians) - y * MathF.Sin(radians);
        }

        private static float RotateY(float x, float y, float radians)
        {
            return x * MathF.Sin(radians) + y * MathF.Cos(radians);
        }
    }

    internal sealed class ShooterProjectileCombatBattleModule
    {
        private readonly ShooterBattleState _state;
        private readonly IShooterEntityManager _entities;
        private readonly IShooterBattleRules _rules;
        private readonly ShooterCombatEventBuffer _events;
        private readonly List<int> _projectileRemovalBuffer = new(32);
        private readonly List<ShooterPendingEnemyHit> _pendingEnemyHits = new(32);
        private readonly List<int> _explosionCandidateBuffer = new(32);
        private readonly ShooterSpatialHitIndex _enemyHitIndex;
        private readonly ShooterSpatialPlayerHitIndex _playerHitIndex;
        private readonly ISveltoWorldContext _context;
        private readonly ShooterArenaGameplayOptions _arenaOptions;

        public ShooterProjectileCombatBattleModule(ShooterBattleState state, IShooterEntityManager entities, IShooterBattleRules rules, ShooterCombatEventBuffer events)
            : this(state, entities, rules, events, ShooterArenaGameplayOptions.Disabled)
        {
        }

        public ShooterProjectileCombatBattleModule(ShooterBattleState state, IShooterEntityManager entities, IShooterBattleRules rules, ShooterCombatEventBuffer events, ShooterArenaGameplayOptions arenaOptions)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _entities = entities ?? throw new ArgumentNullException(nameof(entities));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _context = _entities.SveltoContext;
            _arenaOptions = arenaOptions ?? ShooterArenaGameplayOptions.Disabled;
            var hitCellSize = Math.Max(_rules.HitRadius * 2f, 1f);
            _enemyHitIndex = new ShooterSpatialHitIndex(hitCellSize);
            _playerHitIndex = new ShooterSpatialPlayerHitIndex(hitCellSize);
        }

        public void Tick(float deltaTime)
        {
            _projectileRemovalBuffer.Clear();
            _pendingEnemyHits.Clear();

            var projectileCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoProjectileComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.Projectiles);
            projectileCollection.Deconstruct(out NB<ShooterSveltoProjectileComponent> bullets, out _, out var count);
            if (count == 0)
            {
                return;
            }

            var canHitEnemies = _entities.EnemyCount > 0;
            var canHitPlayers = _entities.PlayerCount > 1;
            NB<ShooterSveltoPlayerComponent> players = default;
            var playerCount = 0;
            if (canHitPlayers || canHitEnemies)
            {
                var playerCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoPlayerComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.Players);
                playerCollection.Deconstruct(out players, out _, out playerCount);
                canHitPlayers = playerCount > 1;
                if (canHitPlayers)
                {
                    _playerHitIndex.Rebuild(players, playerCount);
                }
            }

            NB<ShooterSveltoTransformComponent> enemyTransforms = default;
            NB<ShooterSveltoHealthComponent> enemyHealths = default;
            NativeEntityIDs enemyIds = default;
            if (canHitEnemies)
            {
                var enemyCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoTransformComponent, ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayTargets);
                enemyCollection.Deconstruct(out enemyTransforms, out enemyHealths, out enemyIds, out var enemyCount);
                canHitEnemies = enemyCount > 0;
                if (canHitEnemies)
                {
                    _enemyHitIndex.Rebuild(enemyTransforms, enemyHealths, enemyCount);
                }
            }

            for (var i = count - 1; i >= 0; i--)
            {
                bullets[i].X += bullets[i].VelocityX * deltaTime;
                bullets[i].Y += bullets[i].VelocityY * deltaTime;
                bullets[i].RemainingFrames--;

                if (!ShooterCircularArenaMath.IsInside(bullets[i].X, bullets[i].Y, _arenaOptions))
                {
                    _projectileRemovalBuffer.Add(bullets[i].BulletId);
                    continue;
                }

                if (canHitPlayers && TryCollectPlayerHit(in bullets[i], players, out var targetIndex))
                {
                    ApplyPlayerHit(in bullets[i], players, playerCount, targetIndex);
                    _projectileRemovalBuffer.Add(bullets[i].BulletId);
                    continue;
                }

                if (canHitEnemies && TryCollectEnemyHit(in bullets[i], i, enemyTransforms, enemyHealths, enemyIds, out var enemyHit))
                {
                    _pendingEnemyHits.Add(enemyHit);
                    if (IsExplosive(in bullets[i]))
                    {
                        CollectExplosionEnemyHits(in bullets[i], enemyHit.EnemyIndex, enemyTransforms, enemyHealths, enemyIds);
                        _projectileRemovalBuffer.Add(bullets[i].BulletId);
                        continue;
                    }

                    if (bullets[i].PenetrationRemaining <= 0)
                    {
                        _projectileRemovalBuffer.Add(bullets[i].BulletId);
                        continue;
                    }

                    bullets[i].PenetrationRemaining--;
                    AdvancePenetratingBullet(ref bullets[i]);
                }

                if (bullets[i].RemainingFrames <= 0)
                {
                    _projectileRemovalBuffer.Add(bullets[i].BulletId);
                }
            }

            if (_pendingEnemyHits.Count > 0)
            {
                ResolveEnemyHits(enemyTransforms, enemyHealths, players, playerCount);
            }

            RemoveCollectedProjectiles();
        }

        private bool TryCollectPlayerHit(in ShooterSveltoProjectileComponent bullet, NB<ShooterSveltoPlayerComponent> players, out int targetIndex)
        {
            return _playerHitIndex.TryFindFirstHit(bullet.X, bullet.Y, _rules.HitRadius, bullet.OwnerPlayerId, players, out targetIndex);
        }

        private void ApplyPlayerHit(in ShooterSveltoProjectileComponent bullet, NB<ShooterSveltoPlayerComponent> players, int playerCount, int targetIndex)
        {
            players[targetIndex].Hp = Math.Max(0, players[targetIndex].Hp - _rules.HitDamage);
            if (players[targetIndex].Hp == 0)
            {
                players[targetIndex].Alive = false;
            }

            IncrementPlayerScore(bullet.OwnerPlayerId, players, playerCount);
            var target = players[targetIndex];
            _events.AddPlayerHit(bullet.OwnerPlayerId, target.PlayerId, bullet.BulletId, target.X, target.Y, _rules.HitDamage);
        }

        private bool TryCollectEnemyHit(
            in ShooterSveltoProjectileComponent bullet,
            int bulletIndex,
            NB<ShooterSveltoTransformComponent> transforms,
            NB<ShooterSveltoHealthComponent> healths,
            NativeEntityIDs ids,
            out ShooterPendingEnemyHit hit)
        {
            if (_enemyHitIndex.TryFindFirstHit(bullet.X, bullet.Y, _rules.HitRadius, transforms, healths, ids, out var targetIndex, out var enemyId))
            {
                hit = new ShooterPendingEnemyHit(bullet.BulletId, bullet.OwnerPlayerId, bulletIndex, targetIndex, enemyId);
                return true;
            }

            hit = default;
            return false;
        }

        private void CollectExplosionEnemyHits(
            in ShooterSveltoProjectileComponent bullet,
            int directHitEnemyIndex,
            NB<ShooterSveltoTransformComponent> transforms,
            NB<ShooterSveltoHealthComponent> healths,
            NativeEntityIDs ids)
        {
            _enemyHitIndex.CollectCandidates(bullet.X, bullet.Y, bullet.ExplosionRadius, _explosionCandidateBuffer);
            if (_explosionCandidateBuffer.Count == 0)
            {
                return;
            }

            _explosionCandidateBuffer.Sort();
            var radiusSquared = bullet.ExplosionRadius * bullet.ExplosionRadius;
            for (var candidate = 0; candidate < _explosionCandidateBuffer.Count; candidate++)
            {
                var i = _explosionCandidateBuffer[candidate];
                if (i == directHitEnemyIndex || healths[i].Alive == 0)
                {
                    continue;
                }

                var dx = transforms[i].X - bullet.X;
                var dy = transforms[i].Y - bullet.Y;
                if (dx * dx + dy * dy > radiusSquared)
                {
                    continue;
                }

                _pendingEnemyHits.Add(new ShooterPendingEnemyHit(bullet.BulletId, bullet.OwnerPlayerId, 0, i, ids[i], bullet.ExplosionDamage));
            }
        }

        private void ResolveEnemyHits(
            NB<ShooterSveltoTransformComponent> enemyTransforms,
            NB<ShooterSveltoHealthComponent> enemyHealths,
            NB<ShooterSveltoPlayerComponent> players,
            int playerCount)
        {
            for (var i = 0; i < _pendingEnemyHits.Count; i++)
            {
                var hit = _pendingEnemyHits[i];
                if (enemyHealths[hit.EnemyIndex].Alive == 0)
                {
                    continue;
                }

                var damage = hit.Damage > 0 ? hit.Damage : _rules.HitDamage;
                enemyHealths[hit.EnemyIndex].Current = Math.Max(0, enemyHealths[hit.EnemyIndex].Current - damage);
                var defeated = enemyHealths[hit.EnemyIndex].Current == 0;
                if (defeated)
                {
                    enemyHealths[hit.EnemyIndex].Alive = 0;
                    _state.QueueDefeatedEnemyRemoval(checked((int)hit.EnemyId));
                    _state.DefeatedEnemies++;
                    IncrementPlayerScore(hit.OwnerPlayerId, players, playerCount);
                }

                _events.AddEnemyHit(hit.OwnerPlayerId, hit.EnemyId, hit.BulletId, enemyTransforms[hit.EnemyIndex].X, enemyTransforms[hit.EnemyIndex].Y, damage);
            }
        }

        private void RemoveCollectedProjectiles()
        {
            if (_projectileRemovalBuffer.Count == 0)
            {
                return;
            }

            _entities.BeginStructuralChanges();
            try
            {
                for (var i = 0; i < _projectileRemovalBuffer.Count; i++)
                {
                    _entities.RemoveProjectile(_projectileRemovalBuffer[i]);
                }
            }
            finally
            {
                _entities.EndStructuralChanges();
            }
        }

        private static bool IsExplosive(in ShooterSveltoProjectileComponent bullet)
        {
            return bullet.ExplosionRadius > 0f && bullet.ExplosionDamage > 0;
        }

        private void AdvancePenetratingBullet(ref ShooterSveltoProjectileComponent bullet)
        {
            var directionX = bullet.VelocityX;
            var directionY = bullet.VelocityY;
            ShooterBattleMath.Normalize(ref directionX, ref directionY);
            var step = Math.Max(_rules.HitRadius * 2f, 0.01f);
            bullet.X += directionX * step;
            bullet.Y += directionY * step;
        }

        private static void IncrementPlayerScore(int playerId, NB<ShooterSveltoPlayerComponent> players, int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (players[i].PlayerId == playerId)
                {
                    players[i].Score++;
                    return;
                }
            }
        }

        private readonly struct ShooterPendingEnemyHit
        {
            public ShooterPendingEnemyHit(int bulletId, int ownerPlayerId, int bulletIndex, int enemyIndex, uint enemyId, int damage = 0)
            {
                BulletId = bulletId;
                OwnerPlayerId = ownerPlayerId;
                BulletIndex = bulletIndex;
                EnemyIndex = enemyIndex;
                EnemyId = enemyId;
                Damage = damage;
            }

            public int BulletId { get; }
            public int OwnerPlayerId { get; }
            public int BulletIndex { get; }
            public int EnemyIndex { get; }
            public uint EnemyId { get; }
            public int Damage { get; }
        }
    }
}

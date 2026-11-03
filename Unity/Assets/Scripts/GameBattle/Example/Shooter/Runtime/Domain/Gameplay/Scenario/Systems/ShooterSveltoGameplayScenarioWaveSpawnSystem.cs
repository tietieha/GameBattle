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
    internal sealed class ShooterSveltoGameplayScenarioWaveSpawnSystem
    {
        private const float Pi = 3.14159265358979323846f;

        private readonly ISveltoWorldContext _context;
        private readonly List<PendingEnemySpawn> _enemySpawnBuffer = new(64);
        private uint _nextTargetId;
        private int[] _waveSpawned = Array.Empty<int>();

        public ShooterSveltoGameplayScenarioWaveSpawnSystem(ISveltoWorldContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Reset(in ShooterSveltoGameplayScenarioConfig config)
        {
            _nextTargetId = 1;
            _waveSpawned = new int[config.BattleFlow.Waves.Length];
            _enemySpawnBuffer.Clear();
        }

        public void Tick(in ShooterSveltoGameplayScenarioConfig config, int frame)
        {
            var activeEnemies = CountAliveEnemies();
            var waves = config.BattleFlow.Waves;
            _enemySpawnBuffer.Clear();
            for (var i = 0; i < waves.Length; i++)
            {
                var wave = waves[i];
                if (frame < wave.StartFrame || _waveSpawned[i] >= wave.EnemyCount || activeEnemies >= config.BattleFlow.MaxActiveEnemies)
                {
                    continue;
                }

                var framesSinceStart = frame - wave.StartFrame;
                if (framesSinceStart % wave.SpawnFrameInterval != 0)
                {
                    continue;
                }

                QueueEnemySpawn(in config, in wave, _waveSpawned[i]);
                _waveSpawned[i]++;
                activeEnemies++;
            }

            FlushEnemySpawns();
        }

        private void QueueEnemySpawn(in ShooterSveltoGameplayScenarioConfig config, in ShooterSveltoGameplayWaveConfig wave, int spawnIndex)
        {
            var targetId = _nextTargetId++;
            var angle = (wave.WaveId * 97 + spawnIndex * 37) * Pi / 180f;
            var spawnRadius = MathF.Min(wave.SpawnRadius, config.ArenaRadius);
            var x = MathF.Cos(angle) * spawnRadius;
            var y = MathF.Sin(angle) * spawnRadius;
            var dx = -x;
            var dy = -y;
            ShooterSveltoGameplayScenarioEcsUtility.Normalize(ref dx, ref dy);

            _enemySpawnBuffer.Add(new PendingEnemySpawn(
                targetId,
                new ShooterSveltoTransformComponent
                {
                    X = x,
                    Y = y,
                    DirectionX = dx,
                    DirectionY = dy
                },
                new ShooterSveltoHealthComponent
                {
                    Current = wave.EnemyHp,
                    Max = wave.EnemyHp,
                    Alive = 1
                }));
        }

        private void FlushEnemySpawns()
        {
            if (_enemySpawnBuffer.Count == 0)
            {
                return;
            }

            for (var i = 0; i < _enemySpawnBuffer.Count; i++)
            {
                var spawn = _enemySpawnBuffer[i];
                var transform = spawn.Transform;
                var health = spawn.Health;
                ShooterSveltoEntityLayout.BuildGameplayTarget(_context, spawn.EntityId, in transform, in health);
            }

            _context.SubmitEntities();
        }

        private int CountAliveEnemies()
        {
            var alive = 0;
            var healthCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoHealthComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.GameplayTargets);
            healthCollection.Deconstruct(out NB<ShooterSveltoHealthComponent> healths, out _, out var count);
            for (var i = 0; i < count; i++)
            {
                if (healths[i].Alive != 0)
                {
                    alive++;
                }
            }

            return alive;
        }

        private readonly struct PendingEnemySpawn
        {
            public PendingEnemySpawn(uint entityId, ShooterSveltoTransformComponent transform, ShooterSveltoHealthComponent health)
            {
                EntityId = entityId;
                Transform = transform;
                Health = health;
            }

            public uint EntityId { get; }
            public ShooterSveltoTransformComponent Transform { get; }
            public ShooterSveltoHealthComponent Health { get; }
        }
    }
}

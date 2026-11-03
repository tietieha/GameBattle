#nullable enable

using System;
using AbilityKit.Demo.Shooter.Runtime.Infrastructure.Ecs;
using AbilityKit.World.Svelto;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal sealed class ShooterSveltoGameplayScenarioInitializer
    {
        private const float Pi = 3.14159265358979323846f;

        private readonly ISveltoWorldContext _context;
        private readonly ShooterSveltoGameplayScenarioWaveSpawnSystem _waveSpawnSystem;
        private readonly ShooterSveltoGameplayScenarioProjectileSystem _projectileSystem;
        private readonly ShooterSveltoGameplayScenarioShooterDecisionSystem _shooterDecisionSystem;
        private readonly ShooterSveltoGameplayScenarioEnemyDecisionSystem _enemyDecisionSystem;

        public ShooterSveltoGameplayScenarioInitializer(
            ISveltoWorldContext context,
            ShooterSveltoGameplayScenarioWaveSpawnSystem waveSpawnSystem,
            ShooterSveltoGameplayScenarioProjectileSystem projectileSystem,
            ShooterSveltoGameplayScenarioShooterDecisionSystem shooterDecisionSystem,
            ShooterSveltoGameplayScenarioEnemyDecisionSystem enemyDecisionSystem)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _waveSpawnSystem = waveSpawnSystem ?? throw new ArgumentNullException(nameof(waveSpawnSystem));
            _projectileSystem = projectileSystem ?? throw new ArgumentNullException(nameof(projectileSystem));
            _shooterDecisionSystem = shooterDecisionSystem ?? throw new ArgumentNullException(nameof(shooterDecisionSystem));
            _enemyDecisionSystem = enemyDecisionSystem ?? throw new ArgumentNullException(nameof(enemyDecisionSystem));
        }

        public void Prepare(in ShooterSveltoGameplayScenarioConfig config)
        {
            _context.EntityFunctions.RemoveEntitiesFromGroup(ShooterSveltoGroups.GameplayShooters);
            _context.EntityFunctions.RemoveEntitiesFromGroup(ShooterSveltoGroups.GameplayTargets);
            _context.EntityFunctions.RemoveEntitiesFromGroup(ShooterSveltoGroups.GameplayProjectiles);
            _context.SubmitEntities();
            _waveSpawnSystem.Reset(in config);
            _projectileSystem.Reset();
            _shooterDecisionSystem.Reset();
            _enemyDecisionSystem.Reset();

            BuildShooters(in config);
            _context.SubmitEntities();
        }

        private void BuildShooters(in ShooterSveltoGameplayScenarioConfig config)
        {
            var loadout = config.Loadout;
            var spreadRadians = loadout.SpreadDegrees * Pi / 180f;
            var radius = MathF.Max(0.5f, config.ArenaRadius * 0.25f);

            for (var i = 0; i < config.ShooterCount; i++)
            {
                var entityId = (uint)(i + 1);
                var angle = config.ShooterCount == 1 ? 0f : i * 2f * Pi / config.ShooterCount;
                var x = MathF.Cos(angle) * radius;
                var y = MathF.Sin(angle) * radius;
                var dx = -x;
                var dy = -y;
                ShooterSveltoGameplayScenarioEcsUtility.Normalize(ref dx, ref dy);

                var initializer = _context.EntityFactory.BuildEntity<ShooterSveltoGameplayShooterDescriptor>(entityId, ShooterSveltoGroups.GameplayShooters);
                initializer.Init(new ShooterSveltoTransformComponent
                {
                    X = x,
                    Y = y,
                    DirectionX = dx,
                    DirectionY = dy
                });
                initializer.Init(new ShooterSveltoHealthComponent
                {
                    Current = 10,
                    Max = 10,
                    Alive = 1
                });
                initializer.Init(new ShooterSveltoWeaponComponent
                {
                    LoadoutId = loadout.LoadoutId,
                    ProjectileSpeed = loadout.ProjectileSpeed,
                    ProjectileLifeFrames = loadout.ProjectileLifeFrames,
                    Damage = loadout.Damage,
                    CooldownFrames = loadout.CooldownFrames,
                    ProjectilesPerShot = loadout.ProjectilesPerShot,
                    SpreadRadians = spreadRadians
                });
                initializer.Init(new ShooterSveltoCooldownComponent
                {
                    RemainingFrames = 0
                });
                initializer.Init(new ShooterSveltoTargetComponent
                {
                    TargetEntityId = 0
                });
            }
        }
    }
}

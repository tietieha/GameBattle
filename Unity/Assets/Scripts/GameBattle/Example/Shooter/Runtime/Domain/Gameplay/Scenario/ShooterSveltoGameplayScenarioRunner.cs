#nullable enable

using System;
using AbilityKit.Ability.World.DI;
using AbilityKit.Ability.World.Services.Attributes;
using AbilityKit.World.Svelto;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public interface IShooterSveltoGameplayScenarioRunner
    {
        ShooterSveltoGameplayScenarioResult Run(in ShooterSveltoGameplayScenarioConfig config);
    }

    [WorldService(typeof(ShooterSveltoGameplayScenarioRunner), WorldLifetime.Singleton)]
    [WorldService(typeof(IShooterSveltoGameplayScenarioRunner), WorldLifetime.Singleton)]
    public sealed class ShooterSveltoGameplayScenarioRunner : IShooterSveltoGameplayScenarioRunner
    {
        private readonly ISveltoWorldContext _context;

        public ShooterSveltoGameplayScenarioRunner(ISveltoWorldContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ShooterSveltoGameplayScenarioResult Run(in ShooterSveltoGameplayScenarioConfig config)
        {
            var resultCollector = new ShooterSveltoGameplayScenarioResultCollector(_context);
            var projectileSystem = new ShooterSveltoGameplayScenarioProjectileSystem(_context);
            var waveSpawnSystem = new ShooterSveltoGameplayScenarioWaveSpawnSystem(_context);
            var shooterDecisionSystem = new ShooterSveltoGameplayScenarioShooterDecisionSystem(_context, projectileSystem);
            var enemyDecisionSystem = new ShooterSveltoGameplayScenarioEnemyDecisionSystem(_context, projectileSystem);
            var initializer = new ShooterSveltoGameplayScenarioInitializer(_context, waveSpawnSystem, projectileSystem, shooterDecisionSystem, enemyDecisionSystem);

            initializer.Prepare(in config);
            for (var frame = 0; frame < config.TickCount; frame++)
            {
                waveSpawnSystem.Tick(in config, frame);
                shooterDecisionSystem.Tick(in config);
                enemyDecisionSystem.Tick(in config, frame);
                projectileSystem.Tick(config.TickDeltaTime, config.ArenaRadius);
            }

            return resultCollector.BuildResult(in config, projectileSystem.CreateCounters());
        }
    }
}

using System;
using AbilityKit.Ability.Host.Framework;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Ability.World.Management;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public sealed class ShooterWorldHost
    {
        private readonly HostRuntime _hostRuntime;

        public ShooterWorldHost()
            : this(CreateDefaultHostRuntime())
        {
        }

        public ShooterWorldHost(Action<WorldCreateOptions> configureWorldOptions)
            : this(CreateDefaultHostRuntime(configureWorldOptions))
        {
        }

        public ShooterWorldHost(HostRuntime hostRuntime)
        {
            _hostRuntime = hostRuntime ?? throw new ArgumentNullException(nameof(hostRuntime));
        }

        public HostRuntime HostRuntime => _hostRuntime;

        public IWorld CreateBattleWorld(string worldId)
        {
            return CreateBattleWorld(new WorldId(string.IsNullOrWhiteSpace(worldId) ? "shooter-battle" : worldId));
        }

        public IWorld CreateBattleWorld(WorldId worldId)
        {
            return _hostRuntime.CreateWorld(new WorldCreateOptions(worldId, ShooterGameplay.WorldType));
        }

        public bool TryGetBattleWorld(string worldId, out IWorld world)
        {
            return _hostRuntime.TryGetWorld(new WorldId(worldId), out world);
        }

        public bool DestroyBattleWorld(string worldId)
        {
            return _hostRuntime.DestroyWorld(new WorldId(worldId));
        }

        public void Tick(float deltaTime)
        {
            _hostRuntime.Tick(deltaTime);
        }

        public static HostRuntime CreateDefaultHostRuntime(Action<WorldCreateOptions>? configureWorldOptions = null)
        {
            var registry = new WorldTypeRegistry();
            ShooterWorldBlueprintsRegistration.RegisterAll(registry);

            var options = new HostRuntimeOptions();
            if (configureWorldOptions != null)
            {
                options.OnBeforeCreateWorld = configureWorldOptions;
            }

            return new HostRuntime(new WorldManager(new RegistryWorldFactory(registry)), options);
        }
    }
}

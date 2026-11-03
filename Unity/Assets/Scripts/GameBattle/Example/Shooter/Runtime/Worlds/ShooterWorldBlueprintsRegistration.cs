using System;
using AbilityKit.Ability.Host.Builder;
using AbilityKit.Ability.Host.WorldBlueprints;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Ability.World.Management;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public static class ShooterWorldBlueprintsRegistration
    {
        public static readonly string[] DefaultWorldTypes =
        {
            ShooterGameplay.WorldType,
        };

        public static WorldBlueprintRegistry CreateDefaultRegistry()
        {
            var registry = new WorldBlueprintRegistry();
            RegisterAll(registry);
            return registry;
        }

        public static void RegisterAll(WorldBlueprintRegistry registry)
        {
            RegisterDefaults(registry);
        }

        public static void RegisterDefaults(WorldBlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));

            registry.Register(new ShooterBattleWorldBlueprint());
        }

        public static void RegisterAll(WorldTypeRegistry registry)
        {
            RegisterAll(registry, options => new ShooterLogicWorld(options));
        }

        public static void RegisterAll(WorldTypeRegistry registry, Func<WorldCreateOptions, IWorld> baseFactory)
        {
            RegisterAll(registry, baseFactory, configureBlueprints: null);
        }

        public static void RegisterAll(
            WorldTypeRegistry registry,
            Func<WorldCreateOptions, IWorld> baseFactory,
            Action<WorldBlueprintRegistry>? configureBlueprints)
        {
            var blueprintRegistry = CreateDefaultRegistry();
            configureBlueprints?.Invoke(blueprintRegistry);
            RegisterAll(registry, baseFactory, blueprintRegistry, DefaultWorldTypes);
        }

        public static void RegisterAll(
            WorldTypeRegistry registry,
            Func<WorldCreateOptions, IWorld> baseFactory,
            WorldBlueprintRegistry blueprintRegistry,
            params string[] worldTypes)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (baseFactory == null) throw new ArgumentNullException(nameof(baseFactory));
            if (blueprintRegistry == null) throw new ArgumentNullException(nameof(blueprintRegistry));
            if (worldTypes == null || worldTypes.Length == 0) throw new ArgumentException("worldTypes is required", nameof(worldTypes));

            var fallbackRegistry = new WorldTypeRegistry();
            for (int i = 0; i < worldTypes.Length; i++)
            {
                var worldType = worldTypes[i];
                if (string.IsNullOrEmpty(worldType)) continue;
                fallbackRegistry.Register(worldType, baseFactory);
            }

            var factory = new DefaultWorldFactory(blueprintRegistry, new RegistryWorldFactory(fallbackRegistry));
            for (int i = 0; i < worldTypes.Length; i++)
            {
                var worldType = worldTypes[i];
                if (string.IsNullOrEmpty(worldType)) continue;
                registry.Register(worldType, factory.Create);
            }
        }
    }
}

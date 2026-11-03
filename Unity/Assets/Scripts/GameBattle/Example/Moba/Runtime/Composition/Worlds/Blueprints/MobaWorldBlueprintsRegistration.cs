using System;
using AbilityKit.Ability.Host.WorldBlueprints;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Ability.World.Management;

namespace AbilityKit.Demo.Moba.Worlds.Blueprints
{
    public static class MobaWorldBlueprintsRegistration
    {
        public static readonly string[] DefaultWorldTypes =
        {
            MobaBattleWorldBlueprint.Type,
            MobaLobbyWorldBlueprint.Type,
        };

        /// <summary>
        /// 创建默认的 MOBA 逻辑世界蓝图注册表。
        /// </summary>
        public static WorldBlueprintRegistry CreateDefaultRegistry()
        {
            var registry = new WorldBlueprintRegistry();
            RegisterAll(registry);
            return registry;
        }

        /// <summary>
        /// 注册到 WorldBlueprintRegistry。
        /// </summary>
        public static void RegisterAll(WorldBlueprintRegistry registry)
        {
            RegisterDefaults(registry);
        }

        public static void RegisterDefaults(WorldBlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));

            registry
                .Register(new MobaLobbyWorldBlueprint())
                .Register(new MobaBattleWorldBlueprint());
        }

        /// <summary>
        /// 注册到 WorldTypeRegistry，使用默认蓝图注册表。
        /// </summary>
        public static void RegisterAll(WorldTypeRegistry registry, Func<WorldCreateOptions, IWorld> baseFactory)
        {
            RegisterAll(registry, baseFactory, configureBlueprints: null);
        }

        /// <summary>
        /// 注册到 WorldTypeRegistry，并允许宿主追加或覆盖蓝图。
        /// </summary>
        public static void RegisterAll(
            WorldTypeRegistry registry,
            Func<WorldCreateOptions, IWorld> baseFactory,
            Action<WorldBlueprintRegistry> configureBlueprints)
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

            var adapter = new BlueprintToWorldFactoryAdapter(baseFactory, blueprintRegistry);
            for (int i = 0; i < worldTypes.Length; i++)
            {
                var worldType = worldTypes[i];
                if (string.IsNullOrEmpty(worldType)) continue;
                registry.Register(worldType, options => adapter.Create(options));
            }
        }
    }
}

using System;
using AbilityKit.Ability.Host.Builder.Components;
using AbilityKit.Ability.Host.Framework;
using AbilityKit.Ability.Host.WorldBlueprints;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Ability.World.DI;
using AbilityKit.Ability.World.Management;

namespace AbilityKit.Ability.Host.Builder
{
    /// <summary>
    /// 默认世界工厂
    /// 支持 Blueprint 配置和基础世界创建
    /// </summary>
    public sealed class DefaultWorldFactory : IWorldFactory
    {
        private readonly IWorldBlueprintRegistry _blueprintRegistry;
        private readonly IWorldFactory _fallbackFactory;

        public DefaultWorldFactory()
            : this(null, null)
        {
        }

        public DefaultWorldFactory(IWorldBlueprintRegistry blueprintRegistry)
            : this(blueprintRegistry, null)
        {
        }

        public DefaultWorldFactory(IWorldBlueprintRegistry blueprintRegistry, IWorldFactory fallbackFactory)
        {
            _blueprintRegistry = blueprintRegistry;
            _fallbackFactory = fallbackFactory ?? CreateDefaultFallbackFactory();
        }

        public IWorld Create(WorldCreateOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (_blueprintRegistry != null && _blueprintRegistry.TryGet(options.WorldType, out var blueprint))
            {
                blueprint.Configure(options);
            }

            return _fallbackFactory.Create(options);
        }

        private static IWorldFactory CreateDefaultFallbackFactory()
        {
            var registry = new WorldTypeRegistry();
            registry.Register("default", options => throw new InvalidOperationException("No world factory registered. Please use BlueprintRegistry or register a world factory."));
            return new RegistryWorldFactory(registry);
        }
    }
}

using System;
using AbilityKit.Ability.Host.WorldBlueprints;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Ability.World.Management;

namespace AbilityKit.Demo.Moba.Worlds.Blueprints
{
    /// <summary>
    /// 将 WorldBlueprintRegistry 和基础工厂适配为 IWorldFactory 的包装器
    /// </summary>
    public sealed class BlueprintToWorldFactoryAdapter : IWorldFactory
    {
        private readonly Func<WorldCreateOptions, IWorld> _baseFactory;
        private readonly WorldBlueprintRegistry _blueprints;

        public BlueprintToWorldFactoryAdapter(Func<WorldCreateOptions, IWorld> baseFactory, WorldBlueprintRegistry blueprints)
        {
            _baseFactory = baseFactory ?? throw new ArgumentNullException(nameof(baseFactory));
            _blueprints = blueprints ?? throw new ArgumentNullException(nameof(blueprints));
        }

        public IWorld Create(WorldCreateOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _blueprints.Configure(options);
            return _baseFactory(options);
        }
    }
}

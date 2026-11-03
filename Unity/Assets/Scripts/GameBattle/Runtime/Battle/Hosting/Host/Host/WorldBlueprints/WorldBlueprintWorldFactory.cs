using System;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Ability.World.Management;

namespace AbilityKit.Ability.Host.WorldBlueprints
{
    public sealed class WorldBlueprintWorldFactory : IWorldFactory
    {
        private readonly IWorldFactory _inner;
        private readonly IWorldBlueprintRegistry _blueprints;

        public WorldBlueprintWorldFactory(IWorldFactory inner, IWorldBlueprintRegistry blueprints)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _blueprints = blueprints;
        }

        public IWorld Create(WorldCreateOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (_blueprints != null && _blueprints.TryGet(options.WorldType, out var blueprint))
            {
                blueprint.Configure(options);
            }

            return _inner.Create(options);
        }
    }
}

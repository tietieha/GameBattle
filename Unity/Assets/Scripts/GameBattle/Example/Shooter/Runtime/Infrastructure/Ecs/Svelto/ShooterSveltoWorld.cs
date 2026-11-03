using System;
using AbilityKit.Ability.World.DI;
using AbilityKit.Ability.World.Services.Attributes;
using AbilityKit.World.Svelto;

namespace AbilityKit.Demo.Shooter.Runtime
{
    [WorldService(typeof(ShooterSveltoWorld), WorldLifetime.Singleton)]
    [WorldService(typeof(IShooterSveltoWorld), WorldLifetime.Singleton)]
    public sealed class ShooterSveltoWorld : IShooterSveltoWorld
    {
        public ShooterSveltoWorld(ISveltoWorldContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ISveltoWorldContext Context { get; }
    }
}

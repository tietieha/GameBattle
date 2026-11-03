using System;
using AbilityKit.Ability.Host.Extensions.WorldStart;
using AbilityKit.Core.Logging;
using AbilityKit.Demo.Moba.Services;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Ability.World.DI;
using AbilityKit.Ability.World.Services.Attributes;

namespace AbilityKit.Game.Battle.WorldStart
{
    [WorldService(typeof(IWorldAutoStartHandler), WorldLifetime.Scoped)]
    public sealed class MobaWorldAutoStartHandler : IWorldAutoStartHandler
    {
        private readonly MobaLogicWorldRunGateService _phase;

        public MobaWorldAutoStartHandler(MobaLogicWorldRunGateService phase)
        {
            _phase = phase ?? throw new ArgumentNullException(nameof(phase));
        }

        public bool TryAutoStart(IWorld world, float deltaTime)
        {
            return _phase.InGame;
        }

        public void Dispose()
        {
        }
    }
}

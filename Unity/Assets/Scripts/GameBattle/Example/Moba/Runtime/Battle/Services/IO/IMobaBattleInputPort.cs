using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;
using AbilityKit.Ability.Host.Extensions.Moba.Runtime;

namespace AbilityKit.Demo.Moba.Services
{
    public interface IMobaBattleInputPort
    {
        MobaInputSubmitResult Submit(FrameIndex frame, IReadOnlyList<PlayerInputCommand> inputs);
    }
}

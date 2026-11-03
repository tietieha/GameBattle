using AbilityKit.Demo.Moba.Services;

namespace AbilityKit.Game.Flow
{
    public sealed partial class BattleViewFeature
    {
        protected override BattleContext RuntimeContext => _ctx;
        protected override bool RuntimeIsConfirmed => false;
    }
}

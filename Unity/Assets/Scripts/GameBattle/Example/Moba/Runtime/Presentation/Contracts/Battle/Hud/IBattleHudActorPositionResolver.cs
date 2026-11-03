using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.Hud
{
    public interface IBattleHudActorPositionResolver
    {
        bool TryGetActorWorldPos(int actorId, out MobaFloat3 pos);
    }
}

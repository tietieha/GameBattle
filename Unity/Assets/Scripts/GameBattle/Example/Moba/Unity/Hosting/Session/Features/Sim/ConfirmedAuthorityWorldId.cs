using AbilityKit.Ability.World.Abstractions;

namespace AbilityKit.Game.Flow
{
    internal static class ConfirmedAuthorityWorldId
    {
        private const string DefaultWorldId = "room_1";
        private const string Suffix = "__confirmed";

        public static WorldId Create(BattleStartPlan plan)
        {
            return new WorldId((plan.World.WorldId ?? DefaultWorldId) + Suffix);
        }
    }
}

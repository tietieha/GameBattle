using System.Collections.Generic;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Game.Battle.Agent;
using AbilityKit.Network.Abstractions;

namespace AbilityKit.Game.Flow
{
    internal static class GatewayRoomCleanupHelper
    {
        public static bool RemoveGatewayReliableConnection(IAbilityKitConnectionRegistry connectionRegistry)
        {
            return connectionRegistry != null && connectionRegistry.Remove(AbilityKitConnectionRole.GatewayReliable);
        }

        public static void ClearWorldStartAnchors(IDictionary<WorldId, GatewayWorldStartAnchor> anchors)
        {
            anchors?.Clear();
        }
    }
}

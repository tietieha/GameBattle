#nullable enable

using Svelto.DataStructures;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal static class ShooterSveltoPlayerTargetSelector
    {
        public static bool TryGetOnlyLivePlayer(
            NB<ShooterSveltoPlayerComponent> players,
            int playerCount,
            out int playerIndex,
            out ShooterSveltoPlayerComponent player)
        {
            playerIndex = -1;
            player = default;
            for (var i = 0; i < playerCount; i++)
            {
                if (!IsLive(in players[i]))
                {
                    continue;
                }

                if (playerIndex >= 0)
                {
                    playerIndex = -1;
                    player = default;
                    return false;
                }

                playerIndex = i;
                player = players[i];
            }

            return playerIndex >= 0;
        }

        public static bool TryFindNearestLivePlayer(
            NB<ShooterSveltoPlayerComponent> players,
            int playerCount,
            float selfX,
            float selfY,
            out int playerIndex,
            out ShooterSveltoPlayerComponent player,
            out float distanceSquared)
        {
            playerIndex = -1;
            player = default;
            distanceSquared = float.MaxValue;
            for (var i = 0; i < playerCount; i++)
            {
                if (!IsLive(in players[i]))
                {
                    continue;
                }

                var dx = players[i].X - selfX;
                var dy = players[i].Y - selfY;
                var currentDistanceSquared = dx * dx + dy * dy;
                if (currentDistanceSquared >= distanceSquared)
                {
                    continue;
                }

                playerIndex = i;
                player = players[i];
                distanceSquared = currentDistanceSquared;
            }

            return playerIndex >= 0;
        }

        private static bool IsLive(in ShooterSveltoPlayerComponent player)
        {
            return player.Alive && player.Hp > 0;
        }
    }
}

#nullable enable

using System.Collections.Generic;
using Svelto.DataStructures;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal sealed class ShooterSpatialPlayerHitIndex
    {
        private readonly ShooterSpatialHashGrid _grid;
        private readonly List<int> _candidateBuffer = new(32);

        public ShooterSpatialPlayerHitIndex(float cellSize)
        {
            _grid = new ShooterSpatialHashGrid(cellSize);
        }

        public int CellCount => _grid.CellCount;

        public int IndexedPlayerCount => _grid.TotalEntries;

        public int LargestCellOccupancy => _grid.LargestCellOccupancy;

        public void Rebuild(NB<ShooterSveltoPlayerComponent> players, int count)
        {
            _grid.Clear();
            for (var i = 0; i < count; i++)
            {
                if (!players[i].Alive)
                {
                    continue;
                }

                _grid.Add(players[i].X, players[i].Y, i);
            }
        }

        public bool TryFindFirstHit(
            float x,
            float y,
            float radius,
            int ownerPlayerId,
            NB<ShooterSveltoPlayerComponent> players,
            out int targetIndex)
        {
            targetIndex = -1;
            if (_grid.CellCount == 0)
            {
                return false;
            }

            _candidateBuffer.Clear();
            _grid.CollectAabb(x, y, radius, _candidateBuffer);
            if (_candidateBuffer.Count == 0)
            {
                return false;
            }

            var bestIndex = int.MaxValue;
            var radiusSq = radius * radius;
            for (var i = 0; i < _candidateBuffer.Count; i++)
            {
                var candidateIndex = _candidateBuffer[i];
                if (candidateIndex >= bestIndex || !players[candidateIndex].Alive || players[candidateIndex].PlayerId == ownerPlayerId)
                {
                    continue;
                }

                var dx = players[candidateIndex].X - x;
                var dy = players[candidateIndex].Y - y;
                if (dx * dx + dy * dy > radiusSq)
                {
                    continue;
                }

                bestIndex = candidateIndex;
            }

            if (bestIndex == int.MaxValue)
            {
                return false;
            }

            targetIndex = bestIndex;
            return true;
        }
    }
}

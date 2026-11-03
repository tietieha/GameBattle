#nullable enable

using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.Internal;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal sealed class ShooterSpatialHitIndex
    {
        private readonly ShooterSpatialHashGrid _grid;
        private readonly List<int> _candidateBuffer = new(32);

        public ShooterSpatialHitIndex(float cellSize)
        {
            _grid = new ShooterSpatialHashGrid(cellSize);
        }

        public int CellCount => _grid.CellCount;

        public int IndexedTargetCount => _grid.TotalEntries;

        public int LargestCellOccupancy => _grid.LargestCellOccupancy;

        public void Rebuild(NB<ShooterSveltoTransformComponent> transforms, NB<ShooterSveltoHealthComponent> healths, int count)
        {
            _grid.Clear();
            for (var i = 0; i < count; i++)
            {
                if (healths[i].Alive == 0)
                {
                    continue;
                }

                _grid.Add(transforms[i].X, transforms[i].Y, i);
            }
        }

        public bool TryFindFirstHit(
            float x,
            float y,
            float radius,
            NB<ShooterSveltoTransformComponent> transforms,
            NB<ShooterSveltoHealthComponent> healths,
            NativeEntityIDs ids,
            out int targetIndex,
            out uint targetEntityId)
        {
            targetIndex = -1;
            targetEntityId = 0u;

            if (_grid.CellCount == 0)
            {
                return false;
            }

            CollectCandidates(x, y, radius, _candidateBuffer);
            if (_candidateBuffer.Count == 0)
            {
                return false;
            }

            var bestIndex = int.MaxValue;
            var radiusSq = radius * radius;
            for (var i = 0; i < _candidateBuffer.Count; i++)
            {
                var candidateIndex = _candidateBuffer[i];
                if (candidateIndex >= bestIndex || healths[candidateIndex].Alive == 0)
                {
                    continue;
                }

                var dx = transforms[candidateIndex].X - x;
                var dy = transforms[candidateIndex].Y - y;
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
            targetEntityId = ids[bestIndex];
            return true;
        }

        public void CollectCandidates(float x, float y, float radius, List<int> target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Clear();
            if (_grid.CellCount == 0)
            {
                return;
            }

            _grid.CollectAabb(x, y, radius, target);
        }
    }
}

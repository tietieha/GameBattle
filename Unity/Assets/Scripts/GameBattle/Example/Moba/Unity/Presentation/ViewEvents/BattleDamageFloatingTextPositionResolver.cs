using AbilityKit.Game.Battle.Entity;
using UnityEngine;

namespace AbilityKit.Game.Flow.Battle.ViewEvents
{
    internal sealed class BattleDamageFloatingTextPositionResolver
    {
        private readonly BattleDamageFloatingTextActorPositionResolver _actors;
        private readonly BattleDamageFloatingTextOffsetPolicy _offsets;

        public BattleDamageFloatingTextPositionResolver(
            IBattleEntityQuery query,
            BattleDamageFloatingTextActorPositionResolver actors = null,
            BattleDamageFloatingTextOffsetPolicy offsets = null)
        {
            _actors = actors ?? new BattleDamageFloatingTextActorPositionResolver(query);
            _offsets = offsets ?? new BattleDamageFloatingTextOffsetPolicy();
        }

        public Vector3 Resolve(int targetActorId)
        {
            var position = _actors.Resolve(targetActorId);
            return _offsets.Apply(position);
        }
    }

    internal sealed class BattleDamageFloatingTextActorPositionResolver
    {
        private readonly IBattleEntityQuery _query;

        public BattleDamageFloatingTextActorPositionResolver(IBattleEntityQuery query)
        {
            _query = query;
        }

        public Vector3 Resolve(int targetActorId)
        {
            if (_query != null && _query.TryGetTransform(new BattleNetId(targetActorId), out var transform) && transform != null)
            {
                return transform.Position;
            }

            return Vector3.zero;
        }
    }

    internal sealed class BattleDamageFloatingTextOffsetPolicy
    {
        public Vector3 Apply(in Vector3 position)
        {
            return position + Vector3.up * 2f;
        }
    }
}

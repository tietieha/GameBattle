using AbilityKit.Demo.Moba.Services;
using AbilityKit.Game.Battle.Entity;
using UnityEngine;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleAreaAttachResolver
    {
        public Transform Resolve(BattleViewBinder binder, int ownerActorId, int attachMode)
        {
            if (attachMode != 1) return null;
            if (ownerActorId <= 0) return null;
            if (binder == null) return null;

            return binder.TryGetAttachRoot(new BattleNetId(ownerActorId), out var attach) ? attach : null;
        }
    }
}

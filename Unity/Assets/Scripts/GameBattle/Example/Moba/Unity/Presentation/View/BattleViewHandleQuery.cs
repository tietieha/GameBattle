using System;
using AbilityKit.Game.Battle.Entity;
using UnityEngine;
using EC = AbilityKit.World.ECS;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleViewHandleQuery
    {
        private readonly BattleViewHandleStore _handles;

        public BattleViewHandleQuery(BattleViewHandleStore handles)
        {
            _handles = handles ?? throw new ArgumentNullException(nameof(handles));
        }

        public bool TryGetShellGameObject(EC.IEntityId id, out GameObject go)
        {
            go = null;
            if (!_handles.TryGet(id, out var handle)) return false;
            if (handle.Destroyed) return false;
            if (handle.GameObject == null) return false;

            go = handle.GameObject;
            return true;
        }

        public void ForEachShellGameObject(Action<int, EC.IEntityId, GameObject> visitor)
        {
            if (visitor == null) return;

            _handles.ForEach((id, handle) =>
            {
                if (handle == null || handle.Destroyed || handle.GameObject == null) return;
                visitor(handle.ActorId, id, handle.GameObject);
            });
        }

        public bool TryGetAttachRoot(BattleNetId netId, out Transform transform)
        {
            transform = null;
            if (netId.Value <= 0) return false;

            if (!_handles.TryGetByActorId(netId.Value, out _, out var handle)) return false;
            if (handle.Destroyed || handle.GameObject == null) return false;

            var child = handle.GameObject.transform.Find("AttachRoot");
            if (child == null) return false;

            transform = child;
            return true;
        }
    }
}

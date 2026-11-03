using AbilityKit.Game.Battle.Vfx;
using UnityEngine;
using EC = AbilityKit.World.ECS;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleViewAttachedVfxLifecycle
    {
        private readonly BattleVfxManager _vfx;
        private readonly EC.IEntity _vfxNode;

        public BattleViewAttachedVfxLifecycle(BattleVfxManager vfx, in EC.IEntity vfxNode)
        {
            _vfx = vfx;
            _vfxNode = vfxNode;
        }

        public bool IsAvailable => _vfx != null && _vfxNode.IsValid;

        public bool TryCreateFollowVfx(
            EC.IECWorld world,
            EC.IEntityId target,
            BattleViewHandle handle,
            int vfxId)
        {
            if (!IsAvailable) return false;
            if (handle == null) return false;
            if (vfxId <= 0) return false;

            if (_vfx.TryCreateVfxEntity(world, _vfxNode, vfxId, target, in handle.PendingPos, out var vfxEntity))
            {
                handle.VfxId = vfxId;
                handle.VfxEntityId = vfxEntity.Id;
                return true;
            }

            return false;
        }

        public void SyncPosition(BattleViewHandle handle, in Vector3 pos)
        {
            if (!IsAvailable) return;
            if (handle == null || handle.VfxEntityId.Index == 0) return;

            _vfx.SyncFollow(_vfxNode.World, handle.VfxEntityId, in pos);
        }

        public void Destroy(BattleViewHandle handle)
        {
            Destroy(_vfxNode.World, handle);
        }

        public void Destroy(EC.IECWorld world, BattleViewHandle handle)
        {
            if (_vfx == null) return;
            if (handle == null || handle.VfxEntityId.Index == 0) return;

            _vfx.DestroyVfxEntity(world, handle.VfxEntityId);
            handle.VfxId = 0;
            handle.VfxEntityId = default;
        }
    }
}

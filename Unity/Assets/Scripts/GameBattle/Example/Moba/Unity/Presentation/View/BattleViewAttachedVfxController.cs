using AbilityKit.Game.Battle.Entity;
using AbilityKit.Game.Battle.Vfx;
using UnityEngine;
using EC = AbilityKit.World.ECS;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleViewAttachedVfxController
    {
        private readonly BattleViewProjectileAttachedVfxResolver _projectileVfx;
        private readonly BattleViewAttachedVfxLifecycle _lifecycle;

        public BattleViewAttachedVfxController(
            BattleVfxManager vfx,
            in EC.IEntity vfxNode,
            BattleViewResourceProvider resources = null,
            BattleViewAttachedVfxControllerFactory controllers = null)
        {
            controllers ??= new BattleViewAttachedVfxControllerFactory();

            _projectileVfx = controllers.CreateProjectileVfxResolver(resources);
            _lifecycle = controllers.CreateLifecycle(vfx, in vfxNode);
        }

        public void SyncProjectileVfx(EC.IEntity entity, BattleViewHandle handle, BattleEntityMetaComponent meta)
        {
            if (!_lifecycle.IsAvailable) return;
            if (handle == null) return;

            var plan = _projectileVfx.Resolve(meta);
            if (!plan.HasVfx)
            {
                _lifecycle.Destroy(entity.World, handle);
                return;
            }

            if (plan.IsSatisfiedBy(handle))
            {
                return;
            }

            _lifecycle.Destroy(entity.World, handle);
            _lifecycle.TryCreateFollowVfx(entity.World, entity.Id, handle, plan.DesiredVfxId);
        }

        public void SyncPosition(BattleViewHandle handle, in Vector3 pos)
        {
            _lifecycle.SyncPosition(handle, in pos);
        }

        public void Destroy(BattleViewHandle handle)
        {
            _lifecycle.Destroy(handle);
        }

        public void Destroy(EC.IECWorld world, BattleViewHandle handle)
        {
            _lifecycle.Destroy(world, handle);
        }
    }

    internal sealed class BattleViewAttachedVfxControllerFactory
    {
        public BattleViewProjectileAttachedVfxResolver CreateProjectileVfxResolver(BattleViewResourceProvider resources)
        {
            return new BattleViewProjectileAttachedVfxResolver(resources);
        }

        public BattleViewAttachedVfxLifecycle CreateLifecycle(BattleVfxManager vfx, in EC.IEntity vfxNode)
        {
            return new BattleViewAttachedVfxLifecycle(vfx, in vfxNode);
        }
    }
}

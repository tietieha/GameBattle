using AbilityKit.Game.Battle.Entity;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleViewProjectileAttachedVfxResolver
    {
        private readonly BattleViewResourceProvider _resources;

        public BattleViewProjectileAttachedVfxResolver(BattleViewResourceProvider resources = null)
        {
            _resources = BattleViewResourceProvider.OrDefault(resources);
        }

        public BattleViewProjectileVfxSyncPlan Resolve(BattleEntityMetaComponent meta)
        {
            return new BattleViewProjectileVfxSyncPlan(_resources.ResolveProjectileVfxId(meta));
        }
    }
}

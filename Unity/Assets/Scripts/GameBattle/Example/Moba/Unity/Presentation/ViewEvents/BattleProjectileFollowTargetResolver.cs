using AbilityKit.Game.Battle.Entity;
using EC = AbilityKit.World.ECS;

namespace AbilityKit.Game.Flow.Battle.ViewEvents
{
    internal sealed class BattleProjectileFollowTargetResolver
    {
        private readonly IBattleEntityQuery _query;

        public BattleProjectileFollowTargetResolver(IBattleEntityQuery query)
        {
            _query = query;
        }

        public EC.IEntityId Resolve(int projectileActorId)
        {
            if (projectileActorId <= 0) return default;
            if (_query != null && _query.TryResolve(new BattleNetId(projectileActorId), out var projectileEntity))
            {
                return projectileEntity.Id;
            }

            return default;
        }
    }
}

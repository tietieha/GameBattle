using AbilityKit.Ability.FrameSync.Rollback;
using AbilityKit.Ability.World.Abstractions;

namespace AbilityKit.Game.Battle
{
    public interface IBattleRollbackRegistryFactory
    {
        RollbackRegistry Create(IWorld world);
    }
}

using AbilityKit.Ability.World.DI;

namespace AbilityKit.Game.Battle
{
    public interface IBattleLogicRuntimeFactory
    {
        BattleLogicSessionRuntime CreateRuntime(
            BattleLogicSessionOptions options,
            IBattleRollbackRegistryFactory rollbackRegistryFactory);

        WorldContainerBuilder CreateWorldServices(BattleLogicSessionOptions options);
    }
}

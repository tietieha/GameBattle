using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Gameplay;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    [PlanActionModule(order: MobaPlanActionModuleOrders.AddGameplayVar)]
    public sealed class AddGameplayVarPlanActionModule : MobaPlanActionModuleBase<AddGameplayVarArgs, AddGameplayVarPlanActionModule>
    {
        protected override IActionSchema<AddGameplayVarArgs, IWorldResolver> Schema => AddGameplayVarSchema.Instance;

        protected override void Execute(object triggerArgs, AddGameplayVarArgs args, ExecCtx<IWorldResolver> ctx)
        {
            if (ctx.Context == null || args.KeyId == 0)
            {
                return;
            }

            if (!TryResolveRequired(ctx, out MobaGameplayVariableService variables))
            {
                return;
            }

            variables.Add(args.KeyId, args.Delta);
        }
    }
}

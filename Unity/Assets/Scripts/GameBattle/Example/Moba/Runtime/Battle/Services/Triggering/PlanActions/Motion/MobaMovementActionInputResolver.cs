using AbilityKit.Ability.World.DI;
using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    /// <summary>
    /// 通过组合核心动作事实与移动服务来构建移动动作输入。
    /// </summary>
    internal static class MobaMovementActionInputResolver
    {
        public static MobaMovementActionInput Resolve(object triggerArgs, ExecCtx<IWorldResolver> ctx)
        {
            var actionInput = MobaPlanActionInputResolver.Resolve(triggerArgs, ctx);
            ctx.Context.TryResolve<MobaActorRegistry>(out var actors);
            return new MobaMovementActionInput(actionInput, actors);
        }
    }
}

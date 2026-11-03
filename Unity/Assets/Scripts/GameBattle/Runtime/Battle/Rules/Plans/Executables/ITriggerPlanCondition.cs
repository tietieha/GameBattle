using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public interface ITriggerPlanCondition
    {
        bool Evaluate<TCtx>(object args, in ExecCtx<TCtx> ctx)
            where TCtx : class;
    }
}

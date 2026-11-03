using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public sealed class SelectorTriggerPlanExecutable : CompositeTriggerPlanExecutableBase
    {
        public override string Name => "Selector";
        public override ETriggerPlanExecutableKind Kind => ETriggerPlanExecutableKind.Selector;

        public SelectorTriggerPlanExecutable(ITriggerPlanExecutable[] children, ITriggerPlanCondition condition = null, float weight = 1f)
            : base(children, condition, weight)
        {
        }

        protected override TriggerPlanExecutionResult ExecuteCore<TCtx>(object args, in ExecCtx<TCtx> ctx)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (ShouldStop(in ctx))
                    return TriggerPlanExecutionResult.Skipped("Selector cancelled");

                var child = Children[i];
                if (child == null)
                    continue;

                var result = child.Execute(args, in ctx);
                if (result.IsSuccess)
                    return result;

                if (result.IsFailed)
                    return result;
            }

            return TriggerPlanExecutionResult.Skipped("Selector found no executable branch");
        }
    }
}

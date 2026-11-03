using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public sealed class ParallelTriggerPlanExecutable : CompositeTriggerPlanExecutableBase
    {
        public override string Name => "Parallel";
        public override ETriggerPlanExecutableKind Kind => ETriggerPlanExecutableKind.Parallel;

        public ParallelTriggerPlanExecutable(ITriggerPlanExecutable[] children, ITriggerPlanCondition condition = null, float weight = 1f)
            : base(children, condition, weight)
        {
        }

        protected override TriggerPlanExecutionResult ExecuteCore<TCtx>(object args, in ExecCtx<TCtx> ctx)
        {
            var result = TriggerPlanExecutionResult.None;
            for (int i = 0; i < Children.Count; i++)
            {
                if (ShouldStop(in ctx))
                    return result;

                var child = Children[i];
                if (child == null)
                    continue;

                var childResult = child.Execute(args, in ctx);
                if (childResult.IsFailed)
                    return childResult;

                result = result.Merge(childResult);
            }

            return result;
        }
    }
}

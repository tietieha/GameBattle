using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public sealed class UntilTriggerPlanExecutable : TriggerPlanExecutableBase
    {
        private readonly ITriggerPlanExecutable _child;
        private readonly ITriggerPlanCondition _untilCondition;
        private readonly int _maxIterations;

        public override string Name => "Until";
        public override ETriggerPlanExecutableKind Kind => ETriggerPlanExecutableKind.Until;
        public ITriggerPlanExecutable Child => _child;
        public ITriggerPlanCondition UntilCondition => _untilCondition;
        public int MaxIterations => _maxIterations;

        public UntilTriggerPlanExecutable(
            ITriggerPlanExecutable child,
            ITriggerPlanCondition untilCondition,
            int maxIterations,
            ITriggerPlanCondition guardCondition = null,
            float weight = 1f)
            : base(guardCondition, weight)
        {
            _child = child;
            _untilCondition = untilCondition;
            _maxIterations = maxIterations > 0 ? maxIterations : 1;
        }

        protected override TriggerPlanExecutionResult ExecuteCore<TCtx>(object args, in ExecCtx<TCtx> ctx)
        {
            if (_child == null)
                return TriggerPlanExecutionResult.Skipped("Until child is empty");

            var result = TriggerPlanExecutionResult.None;
            for (int i = 0; i < _maxIterations; i++)
            {
                if (ShouldStop(in ctx))
                    return result;

                if (_untilCondition != null && _untilCondition.Evaluate(args, in ctx))
                    return result;

                var childResult = _child.Execute(args, in ctx);
                if (childResult.IsFailed)
                    return childResult;

                result = result.Merge(childResult);
            }

            return result;
        }
    }
}

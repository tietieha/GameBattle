using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public sealed class RepeatTriggerPlanExecutable : TriggerPlanExecutableBase
    {
        private readonly ITriggerPlanExecutable _child;
        private readonly int _count;

        public override string Name => "Repeat";
        public override ETriggerPlanExecutableKind Kind => ETriggerPlanExecutableKind.Repeat;
        public ITriggerPlanExecutable Child => _child;
        public int Count => _count;

        public RepeatTriggerPlanExecutable(ITriggerPlanExecutable child, int count, ITriggerPlanCondition condition = null, float weight = 1f)
            : base(condition, weight)
        {
            _child = child;
            _count = count > 0 ? count : 1;
        }

        protected override TriggerPlanExecutionResult ExecuteCore<TCtx>(object args, in ExecCtx<TCtx> ctx)
        {
            if (_child == null)
                return TriggerPlanExecutionResult.Skipped("Repeat child is empty");

            var result = TriggerPlanExecutionResult.None;
            for (int i = 0; i < _count; i++)
            {
                if (ShouldStop(in ctx))
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

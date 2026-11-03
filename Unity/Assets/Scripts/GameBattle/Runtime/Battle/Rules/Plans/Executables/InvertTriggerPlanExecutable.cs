using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public sealed class InvertTriggerPlanExecutable : TriggerPlanExecutableBase
    {
        private readonly ITriggerPlanExecutable _child;

        public override string Name => "Invert";
        public override ETriggerPlanExecutableKind Kind => ETriggerPlanExecutableKind.Invert;
        public ITriggerPlanExecutable Child => _child;

        public InvertTriggerPlanExecutable(ITriggerPlanExecutable child, ITriggerPlanCondition condition = null, float weight = 1f)
            : base(condition, weight)
        {
            _child = child;
        }

        protected override TriggerPlanExecutionResult ExecuteCore<TCtx>(object args, in ExecCtx<TCtx> ctx)
        {
            if (_child == null)
                return TriggerPlanExecutionResult.Skipped("Invert child is empty");

            var result = _child.Execute(args, in ctx);
            if (result.IsFailed)
                return TriggerPlanExecutionResult.Success();

            if (result.IsSuccess)
                return TriggerPlanExecutionResult.Failed("Invert child succeeded");

            return TriggerPlanExecutionResult.Skipped(result.Reason);
        }
    }
}

using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public sealed class SucceedTriggerPlanExecutable : TriggerPlanExecutableBase
    {
        private readonly ITriggerPlanExecutable _child;

        public override string Name => "Succeed";
        public override ETriggerPlanExecutableKind Kind => ETriggerPlanExecutableKind.Succeed;
        public ITriggerPlanExecutable Child => _child;

        public SucceedTriggerPlanExecutable(ITriggerPlanExecutable child, ITriggerPlanCondition condition = null, float weight = 1f)
            : base(condition, weight)
        {
            _child = child;
        }

        protected override TriggerPlanExecutionResult ExecuteCore<TCtx>(object args, in ExecCtx<TCtx> ctx)
        {
            if (_child == null)
                return TriggerPlanExecutionResult.Success(0);

            var result = _child.Execute(args, in ctx);
            return TriggerPlanExecutionResult.Success(result.ExecutedCount);
        }
    }
}

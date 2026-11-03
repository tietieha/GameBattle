using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public sealed class FailTriggerPlanExecutable : TriggerPlanExecutableBase
    {
        private readonly ITriggerPlanExecutable _child;
        private readonly string _reason;

        public override string Name => "Fail";
        public override ETriggerPlanExecutableKind Kind => ETriggerPlanExecutableKind.Fail;
        public ITriggerPlanExecutable Child => _child;
        public string Reason => _reason;

        public FailTriggerPlanExecutable(ITriggerPlanExecutable child = null, string reason = null, ITriggerPlanCondition condition = null, float weight = 1f)
            : base(condition, weight)
        {
            _child = child;
            _reason = string.IsNullOrEmpty(reason) ? "Fail node executed" : reason;
        }

        protected override TriggerPlanExecutionResult ExecuteCore<TCtx>(object args, in ExecCtx<TCtx> ctx)
        {
            _child?.Execute(args, in ctx);
            return TriggerPlanExecutionResult.Failed(_reason);
        }
    }
}

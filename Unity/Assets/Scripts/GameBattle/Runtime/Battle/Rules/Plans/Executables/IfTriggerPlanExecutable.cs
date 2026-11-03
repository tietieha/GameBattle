using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public sealed class IfTriggerPlanExecutable : TriggerPlanExecutableBase
    {
        private readonly ITriggerPlanCondition _branchCondition;
        private readonly ITriggerPlanExecutable _thenBranch;
        private readonly ITriggerPlanExecutable _elseBranch;

        public override string Name => "If";
        public override ETriggerPlanExecutableKind Kind => ETriggerPlanExecutableKind.If;
        public ITriggerPlanCondition BranchCondition => _branchCondition;
        public ITriggerPlanExecutable ThenBranch => _thenBranch;
        public ITriggerPlanExecutable ElseBranch => _elseBranch;

        public IfTriggerPlanExecutable(
            ITriggerPlanCondition branchCondition,
            ITriggerPlanExecutable thenBranch,
            ITriggerPlanExecutable elseBranch = null,
            ITriggerPlanCondition guardCondition = null,
            float weight = 1f)
            : base(guardCondition, weight)
        {
            _branchCondition = branchCondition;
            _thenBranch = thenBranch;
            _elseBranch = elseBranch;
        }

        protected override TriggerPlanExecutionResult ExecuteCore<TCtx>(object args, in ExecCtx<TCtx> ctx)
        {
            var branch = _branchCondition == null || _branchCondition.Evaluate(args, in ctx)
                ? _thenBranch
                : _elseBranch;

            return branch != null
                ? branch.Execute(args, in ctx)
                : TriggerPlanExecutionResult.Skipped("If branch is empty");
        }
    }
}

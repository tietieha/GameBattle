using System;
using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public sealed class PredicateExprTriggerPlanCondition : ITriggerPlanCondition
    {
        private readonly PredicateExprPlan _predicate;

        public PredicateExprTriggerPlanCondition(PredicateExprPlan predicate)
        {
            _predicate = predicate;
        }

        public bool Evaluate<TCtx>(object args, in ExecCtx<TCtx> ctx)
            where TCtx : class
        {
            if (_predicate.Nodes == null || _predicate.Nodes.Length == 0)
                return true;

            var plan = new TriggerPlan<object>(phase: 0, priority: 0, triggerId: 0, predicateExpr: _predicate, actions: Array.Empty<ActionCallPlan>());
            var trigger = new PlannedTrigger<object, TCtx>(plan);
            return trigger.Evaluate(args, in ctx);
        }
    }
}

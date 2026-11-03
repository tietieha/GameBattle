using System;

namespace AbilityKit.Ability.Flow.Blocks
{
    public sealed class IfNode : IFlowNode
    {
        private readonly Func<FlowContext, bool> _predicate;
        private readonly IFlowNode _then;
        private readonly IFlowNode _else;

        private IFlowNode _active;
        private bool _entered;

        public IfNode(Func<FlowContext, bool> predicate, IFlowNode thenNode, IFlowNode elseNode = null)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _then = thenNode ?? throw new ArgumentNullException(nameof(thenNode));
            _else = elseNode;
        }

        public void Enter(FlowContext ctx)
        {
            _active = _predicate(ctx) ? _then : _else;
            _entered = false;
        }

        public FlowStatus Tick(FlowContext ctx, float deltaTime)
        {
            if (_active == null) return FlowStatus.Succeeded;

            if (!_entered)
            {
                FlowDiagnostics.Enter(ctx, _active);
                _entered = true;
            }

            var s = FlowDiagnostics.Tick(ctx, _active, deltaTime);
            if (s == FlowStatus.Running) return FlowStatus.Running;

            FlowDiagnostics.Exit(ctx, _active, s);
            _active = null;
            _entered = false;
            return s;
        }

        public void Exit(FlowContext ctx)
        {
            if (_active != null && _entered)
            {
                FlowDiagnostics.Exit(ctx, _active);
            }

            _active = null;
            _entered = false;
        }

        public void Interrupt(FlowContext ctx)
        {
            if (_active != null && _entered)
            {
                FlowDiagnostics.Interrupt(ctx, _active);
            }

            _active = null;
            _entered = false;
        }
    }
}

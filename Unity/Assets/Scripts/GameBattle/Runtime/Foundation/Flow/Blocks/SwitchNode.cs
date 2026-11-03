using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.Flow.Blocks
{
    public sealed class SwitchNode<TKey> : IFlowNode
    {
        private readonly Func<FlowContext, TKey> _select;
        private readonly IReadOnlyDictionary<TKey, IFlowNode> _cases;
        private readonly IFlowNode _default;

        private IFlowNode _active;
        private bool _entered;

        public SwitchNode(Func<FlowContext, TKey> select, IReadOnlyDictionary<TKey, IFlowNode> cases, IFlowNode defaultNode = null)
        {
            _select = select ?? throw new ArgumentNullException(nameof(select));
            _cases = cases ?? throw new ArgumentNullException(nameof(cases));
            _default = defaultNode;
        }

        public void Enter(FlowContext ctx)
        {
            var key = _select(ctx);
            if (!_cases.TryGetValue(key, out _active))
            {
                _active = _default;
            }
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

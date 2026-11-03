using System;

namespace AbilityKit.Ability.Flow.Blocks
{
    public sealed class TimeoutNode : IFlowNode
    {
        private readonly float _seconds;
        private readonly IFlowNode _child;

        private float _elapsed;
        private bool _entered;

        public TimeoutNode(float seconds, IFlowNode child)
        {
            if (seconds < 0f) throw new ArgumentOutOfRangeException(nameof(seconds));
            _seconds = seconds;
            _child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public void Enter(FlowContext ctx)
        {
            _elapsed = 0f;
            _entered = false;
        }

        public FlowStatus Tick(FlowContext ctx, float deltaTime)
        {
            if (!_entered)
            {
                FlowDiagnostics.Enter(ctx, _child);
                _entered = true;
            }

            _elapsed += deltaTime;
            if (_elapsed > _seconds)
            {
                FlowDiagnostics.Interrupt(ctx, _child, FlowStatus.Failed);
                _entered = false;
                return FlowStatus.Failed;
            }

            var s = FlowDiagnostics.Tick(ctx, _child, deltaTime);
            if (s == FlowStatus.Running) return FlowStatus.Running;

            FlowDiagnostics.Exit(ctx, _child, s);
            _entered = false;
            return s;
        }

        public void Exit(FlowContext ctx)
        {
            if (_entered)
            {
                FlowDiagnostics.Exit(ctx, _child);
                _entered = false;
            }
        }

        public void Interrupt(FlowContext ctx)
        {
            if (_entered)
            {
                FlowDiagnostics.Interrupt(ctx, _child);
                _entered = false;
            }
        }
    }
}

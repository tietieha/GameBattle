using System;

namespace AbilityKit.Ability.Flow.Blocks
{
    public sealed class FinallyNode : IFlowNode
    {
        private readonly IFlowNode _try;
        private readonly IFlowNode _finally;

        private bool _tryEntered;
        private bool _finallyEntered;
        private FlowStatus _tryStatus;

        public FinallyNode(IFlowNode tryNode, IFlowNode finallyNode)
        {
            _try = tryNode ?? throw new ArgumentNullException(nameof(tryNode));
            _finally = finallyNode ?? throw new ArgumentNullException(nameof(finallyNode));
        }

        public void Enter(FlowContext ctx)
        {
            _tryEntered = false;
            _finallyEntered = false;
            _tryStatus = FlowStatus.Running;
        }

        public FlowStatus Tick(FlowContext ctx, float deltaTime)
        {
            if (!_finallyEntered)
            {
                if (!_tryEntered)
                {
                    FlowDiagnostics.Enter(ctx, _try);
                    _tryEntered = true;
                }

                var s = FlowDiagnostics.Tick(ctx, _try, deltaTime);
                if (s == FlowStatus.Running) return FlowStatus.Running;

                _tryStatus = s;
                FlowDiagnostics.Exit(ctx, _try, s);
                _tryEntered = false;

                FlowDiagnostics.Enter(ctx, _finally);
                _finallyEntered = true;
            }

            var fs = FlowDiagnostics.Tick(ctx, _finally, deltaTime);
            if (fs == FlowStatus.Running) return FlowStatus.Running;

            FlowDiagnostics.Exit(ctx, _finally, fs);
            _finallyEntered = false;
            return _tryStatus;
        }

        public void Exit(FlowContext ctx)
        {
            if (_tryEntered)
            {
                FlowDiagnostics.Exit(ctx, _try);
                _tryEntered = false;
            }
            if (_finallyEntered)
            {
                FlowDiagnostics.Exit(ctx, _finally);
                _finallyEntered = false;
            }
        }

        public void Interrupt(FlowContext ctx)
        {
            if (_tryEntered)
            {
                FlowDiagnostics.Interrupt(ctx, _try);
                _tryEntered = false;
            }
            if (_finallyEntered)
            {
                FlowDiagnostics.Interrupt(ctx, _finally);
                _finallyEntered = false;
            }
        }
    }
}

using System;

namespace AbilityKit.Ability.Flow.Blocks
{
    public sealed class UsingResourceNode<T> : IFlowNode
    {
        private readonly Func<FlowContext, T> _create;
        private readonly Action<T> _dispose;
        private readonly IFlowNode _body;

        private T _value;
        private bool _created;
        private bool _bodyEntered;

        public UsingResourceNode(Func<FlowContext, T> create, Action<T> dispose, IFlowNode body)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _dispose = dispose;
        }

        public void Enter(FlowContext ctx)
        {
            _value = _create(ctx);
            _created = true;
            ctx.Set(_value);

            _bodyEntered = false;
        }

        public FlowStatus Tick(FlowContext ctx, float deltaTime)
        {
            if (!_bodyEntered)
            {
                FlowDiagnostics.Enter(ctx, _body);
                _bodyEntered = true;
            }

            var s = FlowDiagnostics.Tick(ctx, _body, deltaTime);
            if (s == FlowStatus.Running) return FlowStatus.Running;

            FlowDiagnostics.Exit(ctx, _body, s);
            _bodyEntered = false;
            return s;
        }

        public void Exit(FlowContext ctx)
        {
            if (_bodyEntered)
            {
                FlowDiagnostics.Exit(ctx, _body);
                _bodyEntered = false;
            }

            Dispose(ctx);
        }

        public void Interrupt(FlowContext ctx)
        {
            if (_bodyEntered)
            {
                FlowDiagnostics.Interrupt(ctx, _body);
                _bodyEntered = false;
            }

            Dispose(ctx);
        }

        private void Dispose(FlowContext ctx)
        {
            if (_created)
            {
                ctx.Remove<T>();
                _dispose?.Invoke(_value);
            }

            _value = default;
            _created = false;
        }
    }
}

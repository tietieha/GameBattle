using System;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime.Abstractions;
using AbilityKit.Triggering.Runtime.Context;
using AbilityKit.Triggering.Payload;
using AbilityKit.Triggering.Variables.Numeric;
using BlackboardResolver = AbilityKit.Triggering.Runtime.Abstractions.IBlackboardResolver;

namespace AbilityKit.Triggering.Runtime.Context
{
    /// <summary>
    /// ExecCtx 适配器：将 ExecCtx 中的服务适配为 ActionContext 可用的接口。
    /// 作为 IServiceProvider 为 ActionContext 提供服务。
    /// </summary>
    internal sealed class ExecCtxAdapter : IServiceProvider
    {
        private readonly ActionRegistry _actions;
        private readonly BlackboardResolver _blackboards;
        private readonly IPayloadAccessorRegistry _payloadRegistry;
        private readonly IEventBus _eventBus;
        private readonly INumericVarDomainRegistry _numericDomains;
        private readonly ExecPolicy _policy;

        private BlackboardResolver _blackboardResolver;
        private IPayloadAccessor _payloadAccessor;
        private IVariableRepository _variableRepository;
        private ITimeService _timeService;

        public ExecCtxAdapter(
            ActionRegistry actions,
            BlackboardResolver blackboards,
            IPayloadAccessorRegistry payloadRegistry,
            IEventBus eventBus,
            INumericVarDomainRegistry numericDomains,
            ExecPolicy policy)
        {
            _actions = actions;
            _blackboards = blackboards;
            _payloadRegistry = payloadRegistry;
            _eventBus = eventBus;
            _numericDomains = numericDomains;
            _policy = policy;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(BlackboardResolver)) return BlackboardResolver;
            if (serviceType == typeof(IPayloadAccessor)) return PayloadAccessor;
            if (serviceType == typeof(IVariableRepository)) return VariableRepository;
            if (serviceType == typeof(ITimeService)) return TimeService;
            if (serviceType == typeof(IEventBus)) return EventBus;
            if (serviceType == typeof(ActionRegistry)) return _actions;
            return null;
        }

        public T GetService<T>() where T : class => (T)GetService(typeof(T));

        public BlackboardResolver BlackboardResolver =>
            _blackboardResolver ??= new BlackboardResolverAdapter(_blackboards);

        public IPayloadAccessor PayloadAccessor =>
            _payloadAccessor ??= new PayloadAccessorAdapter(_payloadRegistry);

        public IVariableRepository VariableRepository =>
            _variableRepository ??= new NumericVariableRepositoryAdapter(_numericDomains);

        public ITimeService TimeService =>
            _timeService ??= new TimeServiceAdapter(_policy);

        public IEventBus EventBus => _eventBus;
    }

    internal sealed class BlackboardResolverAdapter : BlackboardResolver
    {
        private readonly BlackboardResolver _inner;

        public BlackboardResolverAdapter(BlackboardResolver inner) => _inner = inner;

        public bool TryResolve(int boardId, out IBlackboard board)
        {
            if (_inner != null)
                return _inner.TryResolve(boardId, out board);

            board = null;
            return false;
        }

        public IBlackboard GetOrCreate(int boardId) => _inner?.GetOrCreate(boardId);

        public bool TryGetDouble(int boardId, int keyId, out double value)
        {
            value = 0d;
            return _inner != null && _inner.TryGetDouble(boardId, keyId, out value);
        }
    }

    internal sealed class PayloadAccessorAdapter : IPayloadAccessor
    {
        private readonly IPayloadAccessorRegistry _registry;

        public PayloadAccessorAdapter(IPayloadAccessorRegistry registry) => _registry = registry;

        public object Target => null;

        public bool TryGetPayloadDouble(in object args, int fieldId, out double value)
        {
            if (_registry != null)
                return _registry.TryGetDouble(in args, fieldId, out value);

            value = 0;
            return false;
        }

        public bool TryGetPayloadObject(in object args, int fieldId, out object value)
        {
            value = null;
            return false;
        }

        public bool TryGetDouble(int fieldId, out double value)
        {
            value = 0d;
            return false;
        }
    }

    internal sealed class NumericVariableRepositoryAdapter : IVariableRepository
    {
        private readonly INumericVarDomainRegistry _domainRegistry;

        public NumericVariableRepositoryAdapter(INumericVarDomainRegistry domainRegistry) => _domainRegistry = domainRegistry;

        public double GetNumeric(string domainId, string key)
        {
            return TryGet(domainId, key, out var value) ? value : 0d;
        }

        public void SetNumeric(string domainId, string key, double value)
        {
            throw new NotSupportedException("ActionContext variable writes require an ExecCtx-backed numeric domain. Use ExecCtx.TrySetNumericVar for typed runtime writes.");
        }

        public bool Has(string domainId, string key) => _domainRegistry != null && _domainRegistry.TryGetDomain(domainId, out _);

        public bool TryGet(string domainId, string key, out double value)
        {
            value = 0d;
            return false;
        }
    }

    internal sealed class TimeServiceAdapter : ITimeService
    {
        private readonly ExecPolicy _policy;

        public TimeServiceAdapter(ExecPolicy policy) => _policy = policy;

        public float DeltaTimeMs => _policy.DeltaTimeMs;
        public float TotalTimeMs => 0;
        public long CurrentTimestampMs => DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
    }
}


using System;
using System.Collections.Generic;

namespace AbilityKit.Context
{
    public enum FlowContextPhase
    {
        Created = 0,
        Running = 1,
        Completed = 2,
        Cancelled = 3,
        Failed = 4
    }

    public readonly struct FlowContextInfo
    {
        public readonly long FlowId;
        public readonly long ParentFlowId;
        public readonly long OwnerEntityId;
        public readonly string Name;
        public readonly FlowContextPhase Phase;
        public readonly long CreatedAtMs;
        public readonly long UpdatedAtMs;

        public FlowContextInfo(
            long flowId,
            long parentFlowId,
            long ownerEntityId,
            string name,
            FlowContextPhase phase,
            long createdAtMs,
            long updatedAtMs)
        {
            FlowId = flowId;
            ParentFlowId = parentFlowId;
            OwnerEntityId = ownerEntityId;
            Name = name;
            Phase = phase;
            CreatedAtMs = createdAtMs;
            UpdatedAtMs = updatedAtMs;
        }

        public bool IsActive => Phase == FlowContextPhase.Created || Phase == FlowContextPhase.Running;
        public bool IsTerminal => Phase == FlowContextPhase.Completed || Phase == FlowContextPhase.Cancelled || Phase == FlowContextPhase.Failed;
    }

    public sealed class FlowContext
    {
        private readonly List<long> _entityIds = new List<long>();
        private readonly List<long> _childFlowIds = new List<long>();

        internal FlowContext(long flowId, long parentFlowId, long ownerEntityId, string name)
        {
            FlowId = flowId;
            ParentFlowId = parentFlowId;
            OwnerEntityId = ownerEntityId;
            Name = name;
            Phase = FlowContextPhase.Created;
            CreatedAtMs = TimeUtil.CurrentTimeMs;
            UpdatedAtMs = CreatedAtMs;
        }

        public long FlowId { get; }
        public long ParentFlowId { get; }
        public long OwnerEntityId { get; }
        public string Name { get; }
        public FlowContextPhase Phase { get; private set; }
        public long CreatedAtMs { get; }
        public long UpdatedAtMs { get; private set; }
        public IReadOnlyList<long> EntityIds => _entityIds;
        public IReadOnlyList<long> ChildFlowIds => _childFlowIds;

        public FlowContextInfo ToInfo()
        {
            return new FlowContextInfo(FlowId, ParentFlowId, OwnerEntityId, Name, Phase, CreatedAtMs, UpdatedAtMs);
        }

        internal void SetPhase(FlowContextPhase phase)
        {
            Phase = phase;
            UpdatedAtMs = TimeUtil.CurrentTimeMs;
        }

        internal void AddEntity(long entityId)
        {
            if (!_entityIds.Contains(entityId))
                _entityIds.Add(entityId);
            UpdatedAtMs = TimeUtil.CurrentTimeMs;
        }

        internal bool RemoveEntity(long entityId)
        {
            var removed = _entityIds.Remove(entityId);
            if (removed)
                UpdatedAtMs = TimeUtil.CurrentTimeMs;
            return removed;
        }

        internal void AddChildFlow(long flowId)
        {
            if (!_childFlowIds.Contains(flowId))
                _childFlowIds.Add(flowId);
            UpdatedAtMs = TimeUtil.CurrentTimeMs;
        }

    }

    public sealed class FlowContextScope : IDisposable
    {
        private readonly ContextRegistry _registry;
        private readonly FlowContextPhase _disposePhase;
        private bool _disposed;

        internal FlowContextScope(ContextRegistry registry, long flowId, FlowContextPhase disposePhase)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            FlowId = flowId;
            _disposePhase = disposePhase;
        }

        public long FlowId { get; }

        public long CreateEntity()
        {
            return _registry.CreateInFlow(FlowId).Build();
        }

        public EntityBuilder Create()
        {
            return _registry.CreateInFlow(FlowId);
        }

        public void Complete()
        {
            if (_disposed)
                return;

            _registry.SetFlowPhase(FlowId, FlowContextPhase.Completed);
            _disposed = true;
        }

        public void Cancel()
        {
            if (_disposed)
                return;

            _registry.SetFlowPhase(FlowId, FlowContextPhase.Cancelled);
            _disposed = true;
        }

        public void Fail()
        {
            if (_disposed)
                return;

            _registry.SetFlowPhase(FlowId, FlowContextPhase.Failed);
            _disposed = true;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _registry.SetFlowPhase(FlowId, _disposePhase);
            _disposed = true;
        }
    }
}

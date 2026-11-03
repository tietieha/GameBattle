using System;
using AbilityKit.Context;

namespace AbilityKit.Buffs
{
    /// <summary>Shared runtime identity lifecycle. The caller owns live values and snapshot payloads.</summary>
    public sealed class BuffContextBinding
    {
        private readonly ContextRegistry _registry;
        private readonly Func<long, BuffState, bool> _isBoundTo;

        public BuffContextBinding(ContextRegistry registry, Func<long, BuffState, bool> isBoundTo)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _isBoundTo = isBoundTo ?? throw new ArgumentNullException(nameof(isBoundTo));
        }

        public long EnsureIdentity(BuffState runtime)
        {
            if (runtime == null) return 0;
            ValidateOwnership(runtime);
            var id = runtime.RuntimeContextId;
            if (id == 0 || !_registry.Exists(id))
            {
                id = _registry.Create().Build();
                runtime.RuntimeContextId = id;
                runtime.RuntimeContextVersion = 1;
            }
            else if (runtime.RuntimeContextVersion <= 0) runtime.RuntimeContextVersion = 1;
            return id;
        }

        public long RestoreIdentity(BuffState runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            ValidateRestoredReference(runtime.RuntimeContextId, runtime.RuntimeContextVersion);
            var id = runtime.RuntimeContextId;
            if (id == 0) return 0;
            _registry.RestoreEntity(id).Build();
            return id;
        }

        public void BindLocalIdentity(BuffState runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            _registry.TryGetReference(runtime.RuntimeContextId, out runtime.RuntimeContextIdentity);
        }

        public void ValidateOwnership(BuffState runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            var id = runtime.RuntimeContextId;
            if (id != 0 && _registry.Exists(id) && !_isBoundTo(id, runtime))
                throw new InvalidOperationException($"Buff runtime does not own context {id}.");
            if (id != 0 && _registry.Exists(id) && runtime.RuntimeContextIdentity.IsValid &&
                _registry.Query(runtime.RuntimeContextIdentity) != ContextEntityQueryStatus.Found)
                throw new InvalidOperationException($"Buff runtime context {id} has a stale local identity.");
        }

        public void DestroyIdentity(BuffState runtime, Action<long> prepareSnapshotAndUnbind, bool preserveReference = false)
        {
            if (runtime == null || runtime.RuntimeContextId == 0) return;
            ValidateOwnership(runtime);
            var id = runtime.RuntimeContextId;
            prepareSnapshotAndUnbind?.Invoke(id);
            if (_registry.Exists(id)) _registry.Destroy(id);
            if (!preserveReference) runtime.ClearRuntimeContext();
        }

        public static void ValidateRestoredReference(long contextId, long version)
        {
            if (contextId < 0 || contextId == long.MaxValue || (contextId == 0 ? version != 0 : version <= 0))
                throw new InvalidOperationException($"Invalid restored Buff runtime context. id={contextId}, version={version}.");
        }
    }
}

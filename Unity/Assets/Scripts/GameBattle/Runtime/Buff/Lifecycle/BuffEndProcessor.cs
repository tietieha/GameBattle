using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace AbilityKit.Buffs
{
    public interface IBuffEndPort<TTarget, TRuntime, TReason> where TRuntime : BuffState
    {
        void StopContinuous(TTarget target, TRuntime runtime, TReason reason);
        void DetachContinuous(TTarget target, TRuntime runtime);
        void EndContext(TRuntime runtime, TReason reason);
        void ClearOwnerBindings(TTarget target, long sourceContextId);
        void NotifyRemoved(TTarget target, TRuntime runtime, int sourceActorId, TReason reason);
        void ReleaseSource(TRuntime runtime);
        void NotifyEnded(TRuntime runtime);
        void Clear(TRuntime runtime);
        void Return(TRuntime runtime);
    }

    public sealed class BuffEndProcessor<TTarget, TRuntime, TReason> where TRuntime : BuffState
    {
        private readonly IBuffEndPort<TTarget, TRuntime, TReason> _port;

        public BuffEndProcessor(IBuffEndPort<TTarget, TRuntime, TReason> port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        public bool End(TTarget target, List<TRuntime> list, int index, TRuntime runtime,
            int sourceActorId, TReason reason)
        {
            if (runtime == null || !BuffStorage<TRuntime>.RemoveAt(list, index, runtime)) return false;
            EndCommitted(target, runtime, sourceActorId, reason);
            return true;
        }

        public void EndCommitted(TTarget target, TRuntime runtime, int sourceActorId, TReason reason)
        {
            if (runtime == null) return;
            var ownerKey = runtime.SourceContextId;
            Exception failure = null;
            BuffCleanupPipeline.TryStep(() => _port.StopContinuous(target, runtime, reason), ref failure);
            BuffCleanupPipeline.TryStep(() => _port.DetachContinuous(target, runtime), ref failure);
            BuffCleanupPipeline.TryStep(() => _port.EndContext(runtime, reason), ref failure);
            BuffCleanupPipeline.TryStep(() => _port.ClearOwnerBindings(target, ownerKey), ref failure);
            BuffCleanupPipeline.TryStep(() => _port.NotifyRemoved(target, runtime, sourceActorId, reason), ref failure);
            BuffCleanupPipeline.TryStep(() => _port.ReleaseSource(runtime), ref failure);
            BuffCleanupPipeline.TryStep(() => _port.NotifyEnded(runtime), ref failure);
            BuffCleanupPipeline.TryStep(() => _port.Clear(runtime), ref failure);
            BuffCleanupPipeline.TryStep(() => _port.Return(runtime), ref failure);
            BuffCleanupPipeline.Throw(failure);
        }
    }

    public static class BuffCleanupPipeline
    {
        public static void TryStep(Action action, ref Exception firstFailure)
        {
            try { action(); }
            catch (Exception failure) { if (firstFailure == null) firstFailure = failure; }
        }

        public static void Run(params Action[] steps)
        {
            Exception failure = null;
            foreach (var step in steps) TryStep(step, ref failure);
            Throw(failure);
        }

        public static void Throw(Exception failure)
        {
            if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}

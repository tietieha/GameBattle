using System;
using System.Collections.Generic;

namespace AbilityKit.Buffs
{
    public readonly struct BuffApplication<TPayload>
    {
        public BuffApplication(int buffId, int sourceActorId, float durationSeconds, long sourceContextId,
            bool forceNewInstance, in BuffStackingSettings settings, TPayload payload)
        {
            BuffId = buffId;
            SourceActorId = sourceActorId;
            DurationSeconds = durationSeconds;
            SourceContextId = sourceContextId;
            ForceNewInstance = forceNewInstance;
            Settings = settings;
            Payload = payload;
        }

        public int BuffId { get; }
        public int SourceActorId { get; }
        public float DurationSeconds { get; }
        public long SourceContextId { get; }
        public bool ForceNewInstance { get; }
        public BuffStackingSettings Settings { get; }
        public TPayload Payload { get; }
    }

    /// <summary>External configuration, context, effects and presentation binding phases.</summary>
    public interface IBuffApplyPort<TTarget, TRuntime, TPayload> where TRuntime : BuffState
    {
        TRuntime Rent();
        bool PrepareExisting(TTarget target, TRuntime runtime, in BuffApplication<TPayload> application);
        bool CompleteExisting(TTarget target, TRuntime runtime, in BuffApplication<TPayload> application,
            int oldStackCount, in BuffStackingApplyResult result);
        bool PrepareCandidate(TTarget target, TRuntime runtime, in BuffApplication<TPayload> application);
        void CleanupCandidate(TTarget target, TRuntime runtime);
        void EndReplaced(TTarget target, TRuntime runtime);
        void NotifyAdded(TTarget target, TRuntime runtime, in BuffApplication<TPayload> application);
    }

    /// <summary>Owns instance selection and commit order, not genre-specific activation rules.</summary>
    public sealed class BuffApplyProcessor<TTarget, TRuntime, TPayload> where TRuntime : BuffState
    {
        private readonly IBuffApplyPort<TTarget, TRuntime, TPayload> _port;
        private readonly BuffStackingPolicyApplier _stacking = new BuffStackingPolicyApplier();

        public BuffApplyProcessor(IBuffApplyPort<TTarget, TRuntime, TPayload> port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        public bool Apply(TTarget target, List<TRuntime> list, in BuffApplication<TPayload> application)
        {
            if (list == null || application.BuffId <= 0 || application.DurationSeconds < 0 ||
                float.IsNaN(application.DurationSeconds) || float.IsInfinity(application.DurationSeconds)) return false;
            var key = application.SourceContextId != 0
                ? BuffRuntimeKey.MatchInstance(application.BuffId, application.SourceActorId, application.SourceContextId)
                : BuffRuntimeKey.MatchBuffAndSource(application.BuffId, application.SourceActorId);
            var existing = default(TRuntime);
            var index = -1;
            var found = !application.ForceNewInstance && BuffStorage<TRuntime>.TryGetRuntime(list, in key, out existing, out index);
            if (found && application.Settings.StackingPolicy != BuffStackingPolicy.Replace)
            {
                var oldStackCount = existing.StackCount;
                try
                {
                    if (!_port.PrepareExisting(target, existing, in application)) return false;
                    var settings = application.Settings;
                    var result = _stacking.ApplyToExisting(existing, in settings, application.SourceActorId, application.DurationSeconds);
                    return _port.CompleteExisting(target, existing, in application, oldStackCount, in result);
                }
                finally
                {
                    // Preparation may change identity/source even if a later binding fails.
                    BuffStorage<TRuntime>.MarkDirty(list);
                }
            }

            var candidate = _port.Rent();
            if (candidate == null) throw new InvalidOperationException("Buff factory returned null.");
            if (list.Contains(candidate)) throw new InvalidOperationException("Buff factory returned an active instance.");
            var committed = false;
            try
            {
                var settings = application.Settings;
                BuffStackingPolicyApplier.Initialize(candidate, application.BuffId, in settings,
                    application.SourceActorId, application.DurationSeconds);
                candidate.SourceContextId = application.SourceContextId;
                if (!_port.PrepareCandidate(target, candidate, in application)) return false;
                if (found)
                {
                    if (!BuffStorage<TRuntime>.ReplaceAt(list, index, existing, candidate)) return false;
                }
                else
                {
                    list.Add(candidate);
                    BuffStorage<TRuntime>.RegisterRuntime(list, candidate);
                }
                committed = true;
            }
            catch (Exception activationFailure)
            {
                if (!committed)
                {
                    Exception firstFailure = activationFailure;
                    BuffCleanupPipeline.TryStep(() => _port.CleanupCandidate(target, candidate), ref firstFailure);
                    candidate = null;
                    BuffCleanupPipeline.Throw(firstFailure);
                }
                throw;
            }
            finally
            {
                if (!committed && candidate != null) _port.CleanupCandidate(target, candidate);
            }

            // A failed old-instance cleanup must not prevent notification of the committed replacement.
            Exception failure = null;
            if (found) BuffCleanupPipeline.TryStep(() => _port.EndReplaced(target, existing), ref failure);
            var snapshot = application;
            BuffCleanupPipeline.TryStep(() => _port.NotifyAdded(target, candidate, in snapshot), ref failure);
            BuffCleanupPipeline.Throw(failure);
            return true;
        }
    }
}

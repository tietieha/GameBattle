using AbilityKit.Buffs;
using AbilityKit.Demo.Moba.Config.BattleDemo.MO;
using AbilityKit.Demo.Moba.Components;
using RuntimePolicyApplier = AbilityKit.Buffs.BuffStackingPolicyApplier;

namespace AbilityKit.Demo.Moba.Services.Buffs.Core
{
    /// <summary>Adapts MOBA configuration, pooling and continuous bindings to the shared policy.</summary>
    internal sealed class BuffStackingPolicyApplier
    {
        private readonly RuntimePolicyApplier _policy = new RuntimePolicyApplier();

        public BuffStackingApplyResult ApplyToExisting(BuffRuntime existing, BuffMO buff,
            int sourceActorId, float durationSeconds)
        {
            if (existing == null || buff == null) return BuffStackingApplyResult.Ignored;
            var settings = ToSettings(buff);
            return _policy.ApplyToExisting(existing, in settings, sourceActorId, durationSeconds);
        }

        public BuffRuntime CreateNewRuntime(BuffMO buff, int sourceActorId, float durationSeconds)
        {
            var settings = ToSettings(buff);
            var runtime = BuffRepository.RentRuntime();
            RuntimePolicyApplier.Initialize(runtime, buff.Id, in settings, sourceActorId, durationSeconds);
            ResetInterval(runtime, buff);
            return runtime;
        }

        public static void ResetInterval(BuffRuntime runtime, BuffMO buff)
        {
            if (runtime == null || buff == null) return;
            var settings = ToSettings(buff);
            RuntimePolicyApplier.ResetInterval(runtime, in settings);
            if (runtime.Continuous != null)
                runtime.Continuous.IntervalRemainingSeconds = runtime.IntervalRemainingSeconds;
        }

        private static BuffStackingSettings ToSettings(BuffMO buff)
        {
            // Legacy schema enums retain their published numeric IDs.
            return new BuffStackingSettings(
                (AbilityKit.Buffs.BuffStackingPolicy)(int)buff.StackingPolicy,
                (AbilityKit.Buffs.BuffRefreshPolicy)(int)buff.RefreshPolicy,
                buff.MaxStacks, buff.IntervalMs);
        }
    }
}

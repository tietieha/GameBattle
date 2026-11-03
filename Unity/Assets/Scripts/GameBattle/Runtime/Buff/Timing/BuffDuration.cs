using AbilityKit.Core.Mathematics;
using AbilityKit.Deterministic;

namespace AbilityKit.Buffs
{
    /// <summary>Duration projection over the battle-driven elapsed clock; never owns another clock.</summary>
    public struct BuffDuration
    {
        public long? DurationRaw { get; private set; }

        public BuffDuration(float seconds)
        {
            DurationRaw = seconds > 0 ? DeterministicMathBridge.ToFixed(seconds).RawValue : (long?)null;
        }

        public void RefreshRaw(long? durationRaw) => DurationRaw = durationRaw;

        public long? GetRemainingRaw(long elapsedRaw) =>
            DurationRaw.HasValue ? System.Math.Max(0, DurationRaw.Value - elapsedRaw) : (long?)null;

        public float GetRemainingSeconds(long elapsedRaw)
        {
            var remaining = GetRemainingRaw(elapsedRaw);
            return remaining.HasValue ? Fixed64.FromRaw(remaining.Value).ToSingle() : float.PositiveInfinity;
        }

        public bool HasExpired(long elapsedRaw) => DurationRaw.HasValue && elapsedRaw >= DurationRaw.Value;
    }
}

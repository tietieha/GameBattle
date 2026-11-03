using System;

namespace AbilityKit.Core.Pooling
{
    public readonly struct PoolTrimPolicy
    {
        private readonly bool _isSpecified;

        public readonly int MinInactiveCount;
        public readonly int MaxInactiveCount;

        public PoolTrimPolicy(int minInactiveCount, int maxInactiveCount)
        {
            if (minInactiveCount < 0) throw new ArgumentException("MinInactiveCount must be >= 0", nameof(minInactiveCount));
            if (maxInactiveCount < 0) throw new ArgumentException("MaxInactiveCount must be >= 0", nameof(maxInactiveCount));
            if (maxInactiveCount < minInactiveCount) throw new ArgumentException("MaxInactiveCount must be >= MinInactiveCount", nameof(maxInactiveCount));

            _isSpecified = true;
            MinInactiveCount = minInactiveCount;
            MaxInactiveCount = maxInactiveCount;
        }

        public static PoolTrimPolicy KeepNone => new PoolTrimPolicy(0, 0);

        public static PoolTrimPolicy KeepDefaultCapacity => new PoolTrimPolicy(0, int.MaxValue);

        public static PoolTrimPolicy KeepAll => new PoolTrimPolicy(0, int.MaxValue);

        internal int ResolveTargetInactiveCount(int defaultCapacity)
        {
            if (!_isSpecified)
            {
                return System.Math.Max(0, defaultCapacity);
            }

            if (MaxInactiveCount == int.MaxValue)
            {
                return System.Math.Max(MinInactiveCount, defaultCapacity);
            }

            return MaxInactiveCount;
        }
    }
}

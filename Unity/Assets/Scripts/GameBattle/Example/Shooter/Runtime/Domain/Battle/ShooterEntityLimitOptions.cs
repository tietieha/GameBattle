#nullable enable

using System;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public readonly struct ShooterEntityLimitOptions
    {
        public const int DefaultMaxEntityCount = 10000;

        public ShooterEntityLimitOptions(int maxEntityCount)
        {
            MaxEntityCount = maxEntityCount < 1 ? DefaultMaxEntityCount : maxEntityCount;
        }

        public int MaxEntityCount { get; }

        public static ShooterEntityLimitOptions Default => new ShooterEntityLimitOptions(DefaultMaxEntityCount);

        public int ClampRequestedCount(int requestedCount)
        {
            return Math.Min(Math.Max(0, requestedCount), MaxEntityCount);
        }
    }
}

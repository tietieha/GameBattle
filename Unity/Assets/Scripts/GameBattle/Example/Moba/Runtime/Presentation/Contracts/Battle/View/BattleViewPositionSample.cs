using System;
using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public readonly struct BattleViewPositionSample
    {
        public BattleViewPositionSample(double time, in MobaFloat3 position)
        {
            Time = time;
            Position = position;
        }

        public double Time { get; }
        public MobaFloat3 Position { get; }
    }

    public readonly struct BattleViewPositionSampleBufferConfig
    {
        public BattleViewPositionSampleBufferConfig(int capacity)
        {
            Capacity = Math.Max(1, capacity);
        }

        public int Capacity { get; }
    }
}

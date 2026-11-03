using System;

namespace AbilityKit.Ability.Host.Extensions.Server.BattleHost
{
    public sealed class BattleHostStartContext
    {
        public BattleHostStartContext(ulong worldId, string battleId, int tickRate)
        {
            WorldId = worldId;
            BattleId = string.IsNullOrEmpty(battleId) ? worldId.ToString() : battleId;
            TickRate = tickRate > 0 ? tickRate : 30;
        }

        public ulong WorldId { get; }
        public string BattleId { get; }
        public int TickRate { get; }
        public TimeSpan TickInterval => TimeSpan.FromMilliseconds(1000.0 / TickRate);
    }

    public sealed class BattleHostLifecycleContext
    {
        public BattleHostLifecycleContext(BattleHostState state)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public BattleHostState State { get; }
        public bool Started => State.Initialized;
    }
}

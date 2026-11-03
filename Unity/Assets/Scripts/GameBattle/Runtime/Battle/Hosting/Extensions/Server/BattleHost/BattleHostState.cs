namespace AbilityKit.Ability.Host.Extensions.Server.BattleHost
{
    public sealed class BattleHostState
    {
        public ulong WorldId { get; private set; }
        public string BattleId { get; private set; }
        public int Frame { get; private set; }
        public int TickRate { get; private set; }
        public bool Initialized { get; private set; }

        public BattleHostState()
        {
            BattleId = string.Empty;
            TickRate = 30;
        }

        public void Initialize(ulong worldId, string battleId, int tickRate)
        {
            WorldId = worldId;
            BattleId = string.IsNullOrEmpty(battleId) ? worldId.ToString() : battleId;
            TickRate = tickRate > 0 ? tickRate : 30;
            Frame = 0;
            Initialized = true;
        }

        public void AdvanceFrame()
        {
            Frame++;
        }

        public void Reset()
        {
            WorldId = 0;
            BattleId = string.Empty;
            Frame = 0;
            TickRate = 30;
            Initialized = false;
        }
    }
}

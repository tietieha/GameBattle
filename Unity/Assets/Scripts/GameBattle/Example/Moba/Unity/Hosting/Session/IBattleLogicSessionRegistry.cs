using System;

namespace AbilityKit.Game.Battle
{
    public interface IBattleLogicSessionRegistry
    {
        event Action<BattleLogicSession> SessionChanged;

        BattleLogicSession Current { get; }
        bool HasSession { get; }

        BattleLogicSession Start(BattleLogicSessionOptions options, IBattleLogicTransport remoteTransport = null);
        void Stop();
    }
}

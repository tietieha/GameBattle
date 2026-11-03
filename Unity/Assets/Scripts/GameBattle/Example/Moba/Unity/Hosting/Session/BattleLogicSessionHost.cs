using System;

namespace AbilityKit.Game.Battle
{
    public static class BattleLogicSessionHost
    {
        private static IBattleLogicSessionRegistry _registry = new DefaultBattleLogicSessionRegistry();

        public static event Action<BattleLogicSession> SessionChanged
        {
            add => _registry.SessionChanged += value;
            remove => _registry.SessionChanged -= value;
        }

        public static IBattleLogicSessionRegistry Registry
        {
            get => _registry;
            set => _registry = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static BattleLogicSession Current => _registry.Current;

        public static bool HasSession => _registry.HasSession;

        public static BattleLogicSession Start(BattleLogicSessionOptions options, IBattleLogicTransport remoteTransport = null)
        {
            return _registry.Start(options, remoteTransport);
        }

        public static void Stop()
        {
            _registry.Stop();
        }
    }
}

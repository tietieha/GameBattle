#nullable enable

using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal sealed class ShooterCombatEventBuffer
    {
        private readonly ShooterBattleState _state;

        public ShooterCombatEventBuffer(ShooterBattleState state)
        {
            _state = state;
        }

        public void AddFire(int sourcePlayerId, int bulletId, float x, float y)
        {
            _state.Events.Add(new ShooterEventSnapshot(ShooterEventType.Fire, sourcePlayerId, 0, bulletId, x, y, 0));
        }

        public void AddPlayerHit(int sourcePlayerId, int targetPlayerId, int bulletId, float x, float y, int damage)
        {
            _state.Events.Add(new ShooterEventSnapshot(ShooterEventType.Hit, sourcePlayerId, targetPlayerId, bulletId, x, y, damage));
        }

        public void AddEnemyHit(int sourcePlayerId, uint targetEnemyId, int bulletId, float x, float y, int damage)
        {
            _state.Events.Add(new ShooterEventSnapshot(ShooterEventType.Hit, sourcePlayerId, -(int)targetEnemyId, bulletId, x, y, damage));
        }

        public void AddEnemyAttack(uint sourceEnemyId, int targetPlayerId, float x, float y, int damage)
        {
            _state.Events.Add(new ShooterEventSnapshot(ShooterEventType.Hit, -(int)sourceEnemyId, targetPlayerId, 0, x, y, damage));
        }
    }
}

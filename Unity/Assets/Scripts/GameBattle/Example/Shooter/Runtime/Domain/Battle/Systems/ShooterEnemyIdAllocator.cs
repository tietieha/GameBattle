#nullable enable

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal sealed class ShooterEnemyIdAllocator
    {
        public const int FirstEnemyEntityId = 10000;

        private int _nextEnemyId = FirstEnemyEntityId;

        public int NextEnemyId => _nextEnemyId;

        public void Reset()
        {
            _nextEnemyId = FirstEnemyEntityId;
        }

        public int Allocate()
        {
            return _nextEnemyId++;
        }

        public void AdvancePast(int enemyId)
        {
            if (enemyId >= _nextEnemyId)
            {
                _nextEnemyId = enemyId + 1;
            }
        }
    }
}

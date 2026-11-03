#nullable enable

using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal sealed class ShooterPureStateInterestPolicy
    {
        public int CreatePlayerPriority(in ShooterPlayerSnapshot player, ShooterPureStateInterestScope? interestScope)
        {
            var priority = player.Alive ? 100 : 10;
            if (!interestScope.HasValue)
            {
                return priority;
            }

            var scope = interestScope.Value;
            if (scope.ObserverPlayerId > 0 && player.PlayerId == scope.ObserverPlayerId)
            {
                return 1000;
            }

            return IsInsideScope(player.X, player.Y, scope) ? priority + 200 : priority;
        }

        public int CreatePlayerPriority(in ShooterSveltoPlayerComponent player, ShooterPureStateInterestScope? interestScope)
        {
            var priority = player.Alive ? 100 : 10;
            if (!interestScope.HasValue)
            {
                return priority;
            }

            var scope = interestScope.Value;
            if (scope.ObserverPlayerId > 0 && player.PlayerId == scope.ObserverPlayerId)
            {
                return 1000;
            }

            return IsInsideScope(player.X, player.Y, scope) ? priority + 200 : priority;
        }

        public int CreateBulletPriority(in ShooterBulletSnapshot bullet, ShooterPureStateInterestScope? interestScope)
        {
            if (!interestScope.HasValue)
            {
                return 80;
            }

            var scope = interestScope.Value;
            if (scope.ObserverPlayerId > 0 && bullet.OwnerPlayerId == scope.ObserverPlayerId)
            {
                return 250;
            }

            return IsInsideScope(bullet.X, bullet.Y, scope) ? 180 : 1;
        }

        public int CreateBulletPriority(in ShooterSveltoProjectileComponent bullet, ShooterPureStateInterestScope? interestScope)
        {
            if (!interestScope.HasValue)
            {
                return 80;
            }

            var scope = interestScope.Value;
            if (scope.ObserverPlayerId > 0 && bullet.OwnerPlayerId == scope.ObserverPlayerId)
            {
                return 250;
            }

            return IsInsideScope(bullet.X, bullet.Y, scope) ? 180 : 1;
        }

        public int CreateEnemyPriority(in ShooterSveltoTransformComponent transform, in ShooterSveltoHealthComponent health, ShooterPureStateInterestScope? interestScope)
        {
            var priority = health.Alive != 0 ? 70 : 5;
            if (!interestScope.HasValue)
            {
                return priority;
            }

            return IsInsideScope(transform.X, transform.Y, interestScope.Value) ? priority + 160 : 1;
        }

        public float ComputeDistanceSquared(float x, float y, ShooterPureStateInterestScope? interestScope)
        {
            return interestScope.HasValue ? ComputeDistanceSquared(x, y, interestScope.Value) : 0f;
        }

        public bool IsInsideScope(float x, float y, ShooterPureStateInterestScope scope)
        {
            if (!scope.HasRadius)
            {
                return true;
            }

            return ComputeDistanceSquared(x, y, scope) <= scope.Radius * scope.Radius;
        }

        private static float ComputeDistanceSquared(float x, float y, ShooterPureStateInterestScope interestScope)
        {
            var dx = x - interestScope.CenterX;
            var dy = y - interestScope.CenterY;
            return (dx * dx) + (dy * dy);
        }
    }
}

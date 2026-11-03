using System;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public struct ShooterBattleEntityState
    {
        public int EntityId;
        public ShooterBattleEntityKind Kind;
        public int OwnerEntityId;
        public int TargetEntityId;
        public float X;
        public float Y;
        public float DirectionX;
        public float DirectionY;
        public float VelocityX;
        public float VelocityY;
        public int HealthCurrent;
        public int HealthMax;
        public int Score;
        public int RemainingFrames;
        public int PenetrationRemaining;
        public float ExplosionRadius;
        public int ExplosionDamage;
        public bool Alive;

        public bool IsActor => Kind == ShooterBattleEntityKind.Player || Kind == ShooterBattleEntityKind.Enemy;

        public bool IsProjectile => Kind == ShooterBattleEntityKind.Projectile;

        public static ShooterBattleEntityState CreatePlayer(in ShooterSveltoPlayerComponent player)
        {
            return new ShooterBattleEntityState
            {
                EntityId = player.PlayerId,
                Kind = ShooterBattleEntityKind.Player,
                X = player.X,
                Y = player.Y,
                DirectionX = player.AimX,
                DirectionY = player.AimY,
                HealthCurrent = player.Hp,
                HealthMax = Math.Max(player.Hp, 0),
                Score = player.Score,
                Alive = player.Alive
            };
        }

        public static ShooterBattleEntityState CreateProjectile(in ShooterSveltoProjectileComponent projectile)
        {
            return new ShooterBattleEntityState
            {
                EntityId = projectile.BulletId,
                Kind = ShooterBattleEntityKind.Projectile,
                OwnerEntityId = projectile.OwnerPlayerId,
                X = projectile.X,
                Y = projectile.Y,
                VelocityX = projectile.VelocityX,
                VelocityY = projectile.VelocityY,
                RemainingFrames = projectile.RemainingFrames,
                PenetrationRemaining = projectile.PenetrationRemaining,
                ExplosionRadius = projectile.ExplosionRadius,
                ExplosionDamage = projectile.ExplosionDamage,
                Alive = true
            };
        }

        public static ShooterBattleEntityState CreateEnemy(int entityId, in ShooterSveltoTransformComponent transform, in ShooterSveltoHealthComponent health)
        {
            return new ShooterBattleEntityState
            {
                EntityId = entityId,
                Kind = ShooterBattleEntityKind.Enemy,
                X = transform.X,
                Y = transform.Y,
                DirectionX = transform.DirectionX,
                DirectionY = transform.DirectionY,
                HealthCurrent = health.Current,
                HealthMax = health.Max,
                Alive = health.Alive != 0
            };
        }

        public ShooterSveltoPlayerComponent ToPlayerComponent()
        {
            return new ShooterSveltoPlayerComponent
            {
                PlayerId = EntityId,
                X = X,
                Y = Y,
                AimX = DirectionX,
                AimY = DirectionY,
                Hp = HealthCurrent,
                Score = Score,
                Alive = Alive
            };
        }

        public ShooterSveltoProjectileComponent ToProjectileComponent()
        {
            return new ShooterSveltoProjectileComponent
            {
                BulletId = EntityId,
                OwnerPlayerId = OwnerEntityId,
                X = X,
                Y = Y,
                VelocityX = VelocityX,
                VelocityY = VelocityY,
                RemainingFrames = RemainingFrames,
                PenetrationRemaining = PenetrationRemaining,
                ExplosionRadius = ExplosionRadius,
                ExplosionDamage = ExplosionDamage
            };
        }

        public ShooterSveltoTransformComponent ToTransformComponent()
        {
            return new ShooterSveltoTransformComponent
            {
                X = X,
                Y = Y,
                DirectionX = DirectionX,
                DirectionY = DirectionY
            };
        }

        public ShooterSveltoHealthComponent ToHealthComponent()
        {
            return new ShooterSveltoHealthComponent
            {
                Current = HealthCurrent,
                Max = HealthMax,
                Alive = Alive ? 1 : 0
            };
        }
    }
}

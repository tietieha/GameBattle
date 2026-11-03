using System;

namespace AbilityKit.Demo.Shooter.View.EntityViewModel
{
    public sealed class ShooterEntityFactory
    {
        private readonly ShooterEntityLookup _lookup;

        public ShooterEntityFactory(ShooterEntityLookup lookup)
        {
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
        }

        public object CreatePlayer(uint entityId, int playerId)
        {
            var player = new ShooterPlayerEntity
            {
                EntityId = entityId,
                PlayerId = playerId,
                Kind = ShooterViewEntityKind.Player
            };
            _lookup.Bind(entityId, player);
            return player;
        }

        public object CreateBullet(uint entityId, int ownerPlayerId)
        {
            var bullet = new ShooterBulletEntity
            {
                EntityId = entityId,
                OwnerPlayerId = ownerPlayerId,
                Kind = ShooterViewEntityKind.Bullet
            };
            _lookup.Bind(entityId, bullet);
            return bullet;
        }
    }
}
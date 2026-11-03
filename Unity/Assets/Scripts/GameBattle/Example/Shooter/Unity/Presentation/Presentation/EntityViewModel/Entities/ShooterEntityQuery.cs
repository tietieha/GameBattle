using System;

namespace AbilityKit.Demo.Shooter.View.EntityViewModel
{
    public sealed class ShooterEntityQuery : IShooterEntityQuery
    {
        private readonly ShooterEntityLookup _lookup;

        public ShooterEntityQuery(ShooterEntityLookup lookup)
        {
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
        }

        public bool TryResolve(uint entityId, out object entity)
        {
            return _lookup.TryResolve(entityId, out entity);
        }

        public bool TryGetPlayer(uint entityId, out ShooterPlayerEntity player)
        {
            player = null;
            if (!TryResolve(entityId, out var entity)) return false;
            player = entity as ShooterPlayerEntity;
            return player != null;
        }

        public bool TryGetBullet(uint entityId, out ShooterBulletEntity bullet)
        {
            bullet = null;
            if (!TryResolve(entityId, out var entity)) return false;
            bullet = entity as ShooterBulletEntity;
            return bullet != null;
        }

        public bool TryGetTransform(uint entityId, out ShooterTransformComponent transform)
        {
            transform = null;
            if (!TryResolve(entityId, out var entity)) return false;
            
            if (entity is ShooterEntityBase baseEntity)
            {
                transform = baseEntity.Transform;
                return transform != null;
            }
            return false;
        }
    }
}
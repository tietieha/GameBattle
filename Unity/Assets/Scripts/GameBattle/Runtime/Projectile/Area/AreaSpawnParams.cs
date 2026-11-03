using AbilityKit.Core.Mathematics;

namespace AbilityKit.Combat.Projectile
{
    public readonly struct AreaSpawnParams
    {
        public readonly int OwnerId;
        public readonly Vec3 Center;
        public readonly float Radius;

        public readonly int LifetimeFrames;
        public readonly int CollisionLayerMask;

        // 每 N 帧发送一次 Stay 事件（0 表示禁用 Stay 事件）。
        public readonly int StayIntervalFrames;

        public AreaSpawnParams(int ownerId, in Vec3 center, float radius, int lifetimeFrames, int collisionLayerMask, int stayIntervalFrames)
        {
            OwnerId = ownerId;
            Center = center;
            Radius = radius;
            LifetimeFrames = lifetimeFrames;
            CollisionLayerMask = collisionLayerMask;
            StayIntervalFrames = stayIntervalFrames;
        }

        public AreaSpawnParams WithCenter(in Vec3 center)
        {
            return new AreaSpawnParams(OwnerId, center, Radius, LifetimeFrames, CollisionLayerMask, StayIntervalFrames);
        }
    }
}

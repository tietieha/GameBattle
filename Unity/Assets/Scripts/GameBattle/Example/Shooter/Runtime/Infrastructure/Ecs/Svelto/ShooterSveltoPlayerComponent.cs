using Svelto.ECS;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public struct ShooterSveltoPlayerComponent : IEntityComponent
    {
        public int PlayerId;
        public float X;
        public float Y;
        public float AimX;
        public float AimY;
        public int Hp;
        public int Score;
        public bool Alive;
    }

    public sealed class ShooterSveltoPlayerDescriptor : GenericEntityDescriptor<ShooterSveltoPlayerComponent>
    {
    }
}

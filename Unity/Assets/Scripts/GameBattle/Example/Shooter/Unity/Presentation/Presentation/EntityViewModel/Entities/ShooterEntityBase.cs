namespace AbilityKit.Demo.Shooter.View.EntityViewModel
{
    public abstract class ShooterEntityBase
    {
        public uint EntityId { get; set; }
        public ShooterViewEntityKind Kind { get; set; }
        public ShooterTransformComponent Transform { get; set; } = new ShooterTransformComponent();
    }
}
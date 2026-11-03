namespace AbilityKit.Demo.Shooter.View.EntityViewModel
{
    public sealed class ShooterBulletEntity : ShooterEntityBase
    {
        public int OwnerPlayerId { get; set; }
        public int RemainingFrames { get; set; }
    }
}
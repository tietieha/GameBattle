namespace AbilityKit.Demo.Shooter.View.EntityViewModel
{
    public sealed class ShooterPlayerEntity : ShooterEntityBase
    {
        public int PlayerId { get; set; }
        public float Health { get; set; }
        public int Score { get; set; }
    }
}
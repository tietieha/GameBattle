#nullable enable

namespace AbilityKit.Demo.Shooter.View.Session
{
    public sealed class ShooterPresentationSessionOptions
    {
        public bool AutoStart { get; set; } = true;
        
        public bool EnableReconciliationDiagnostics { get; set; } = true;
        
        public int SnapshotBufferCapacity { get; set; } = 256;
    }
}
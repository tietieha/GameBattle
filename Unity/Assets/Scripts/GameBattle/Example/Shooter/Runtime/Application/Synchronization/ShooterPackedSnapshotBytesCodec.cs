using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public sealed class ShooterPackedSnapshotBytesCodec
    {
        public byte[] Export(IShooterPackedSnapshotPort snapshotPort, ulong worldId, bool isFullSnapshot = true, bool authorityOverride = false)
        {
            var snapshot = snapshotPort.ExportPackedSnapshot(worldId, isFullSnapshot, authorityOverride);
            return ShooterPackedSnapshotCodec.Serialize(in snapshot);
        }

        public bool Import(IShooterPackedSnapshotPort snapshotPort, byte[] payload)
        {
            var snapshot = ShooterPackedSnapshotCodec.Deserialize(payload);
            return snapshotPort.ImportPackedSnapshot(in snapshot);
        }
    }
}

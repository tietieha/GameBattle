namespace AbilityKit.Combat.MotionSystem.Core
{
    public interface IMotionSnapshotSource
    {
        bool ExportSnapshot(out MotionSourceSnapshot snapshot);

        bool ImportSnapshot(in MotionSourceSnapshot snapshot);
    }
}

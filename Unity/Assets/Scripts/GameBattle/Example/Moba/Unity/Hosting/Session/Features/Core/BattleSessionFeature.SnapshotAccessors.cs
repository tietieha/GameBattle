using AbilityKit.Core.Snapshots.Routing;

namespace AbilityKit.Game.Flow
{
    public sealed partial class BattleSessionFeature
    {
        private FrameSnapshotDispatcher _snapshots
        {
            get => _handles.Snapshot.Snapshots;
            set => _handles.Snapshot.Snapshots = value;
        }
    }
}

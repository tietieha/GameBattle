using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.World.Services;

namespace AbilityKit.Ability.Host
{
    public interface IWorldStateSnapshotProvider : IService
    {
        bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot);
    }

    public interface IWorldStateSnapshotBatchProvider : IWorldStateSnapshotProvider
    {
        int CollectSnapshots(FrameIndex frame, IList<WorldStateSnapshot> snapshots, int maxSnapshots = 32);
    }
}

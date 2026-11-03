using System;
using AbilityKit.Ability.Host;

namespace AbilityKit.Core.Snapshots.Routing
{
    public interface ISnapshotPipelineStageRegistry
    {
        IDisposable AddPipelineStage<T>(int opCode, int order, Action<object, ISnapshotEnvelope, T> handler);
    }
}

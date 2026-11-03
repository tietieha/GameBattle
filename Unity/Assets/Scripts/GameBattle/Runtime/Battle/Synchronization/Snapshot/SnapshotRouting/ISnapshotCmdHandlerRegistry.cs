using System;
using AbilityKit.Ability.Host;

namespace AbilityKit.Core.Snapshots.Routing
{
    public interface ISnapshotCmdHandlerRegistry
    {
        void RegisterCmdHandler<T>(int opCode, Action<object, ISnapshotEnvelope, T> handler);
    }
}

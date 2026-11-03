using System;

namespace AbilityKit.Core.Snapshots.Routing
{
    public sealed class SnapshotRoutingInstance : IDisposable
    {
        public SnapshotRoutingInstance(FrameSnapshotDispatcher snapshots, SnapshotPipeline pipeline, SnapshotCmdHandler cmdHandler)
        {
            Snapshots = snapshots;
            ExternalDispatcher = null;
            Pipeline = pipeline;
            CmdHandler = cmdHandler;
        }

        public SnapshotRoutingInstance(ISnapshotDispatcher externalDispatcher, SnapshotPipeline pipeline, SnapshotCmdHandler cmdHandler)
        {
            Snapshots = null;
            ExternalDispatcher = externalDispatcher ?? throw new ArgumentNullException(nameof(externalDispatcher));
            Pipeline = pipeline;
            CmdHandler = cmdHandler;
        }

        public FrameSnapshotDispatcher? Snapshots { get; }
        public ISnapshotDispatcher? ExternalDispatcher { get; }
        public SnapshotPipeline Pipeline { get; }
        public SnapshotCmdHandler CmdHandler { get; }

        public void Dispose()
        {
            Pipeline?.Dispose();
            CmdHandler?.Dispose();
            // Only dispose internal snapshots, not external dispatcher
            Snapshots?.Dispose();
        }
    }
}

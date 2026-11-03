using System;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;
using AbilityKit.Ability.Host.Extensions.Moba.Snapshot;
using AbilityKit.Ability.World.Services;
using AbilityKit.Ability.World.Services.Attributes;

namespace AbilityKit.Demo.Moba.Services
{
    [MobaSnapshotEmitter(10)]
    [WorldService(typeof(IMobaEnterGameSnapshotSink))]
    [WorldService(typeof(IMobaEnterGameSnapshotSource))]
    [WorldService(typeof(MobaEnterGameSnapshotService))]
    public sealed class MobaEnterGameSnapshotService : IWorldStateSnapshotProvider, IMobaSnapshotEmitter, IMobaEnterGameSnapshotSink, IMobaEnterGameSnapshotSource
    {
        private bool _hasSnapshot;
        private bool _sent;
        private byte[] _snapshotPayload;

        public void PublishEnterGameResPayload(byte[] payload)
        {
            _snapshotPayload = payload;
            _hasSnapshot = payload != null && payload.Length > 0;
            _sent = false;
        }

        public bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot)
        {
            return TryGetEnterGameSnapshot(out snapshot);
        }

        public bool TryGetEnterGameSnapshot(out WorldStateSnapshot snapshot)
        {
            if (!_hasSnapshot || _sent)
            {
                snapshot = default;
                return false;
            }

            snapshot = new WorldStateSnapshot(AbilityKit.Protocol.Moba.MobaOpCodes.Snapshot.EnterGame, _snapshotPayload);
            _sent = true;
            return true;
        }

        public void Dispose()
        {
            _hasSnapshot = false;
            _sent = false;
            _snapshotPayload = null;
        }
    }
}

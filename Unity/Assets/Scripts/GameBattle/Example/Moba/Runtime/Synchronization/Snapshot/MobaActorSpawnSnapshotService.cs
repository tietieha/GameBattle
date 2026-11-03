using System;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Core.Logging;
using AbilityKit.Ability.Host;
using AbilityKit.Ability.World.Services;
using AbilityKit.Ability.World.Services.Attributes;
using AbilityKit.Protocol.Moba.StateSync;

namespace AbilityKit.Demo.Moba.Services
{
    [MobaSnapshotEmitter(20)]
    [WorldService(typeof(MobaActorSpawnSnapshotService))]
    public sealed class MobaActorSpawnSnapshotService : IWorldStateSnapshotProvider, IMobaSnapshotEmitter
    {
        private bool _hasSnapshot;
        private bool _sent;
        private byte[] _snapshotPayload;

        private FrameIndex _lastFrame;
        private readonly MobaSnapshotBuffer<MobaActorSpawnSnapshotEntry> _pending = new MobaSnapshotBuffer<MobaActorSpawnSnapshotEntry>(64, 512);

        public MobaActorSpawnSnapshotService()
        {
            _lastFrame = new FrameIndex(-999999);
        }

        public void PublishSpawnPayload(byte[] payload)
        {
            _snapshotPayload = payload;
            _hasSnapshot = payload != null && payload.Length > 0;
            _sent = false;
        }

        public void Enqueue(in MobaActorSpawnSnapshotEntry entry)
        {
            if (entry.NetId <= 0) return;
            _pending.Add(entry);
        }

        public bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot)
        {
            if (_hasSnapshot && !_sent)
            {
                if (frame.Value == _lastFrame.Value)
                {
                    snapshot = default;
                    return false;
                }

                snapshot = new WorldStateSnapshot(AbilityKit.Protocol.Moba.MobaOpCodes.Snapshot.ActorSpawn, _snapshotPayload);
                _sent = true;
                _lastFrame = frame;
                return true;
            }

            if (_pending.Count > 0)
            {
                if (frame.Value == _lastFrame.Value)
                {
                    snapshot = default;
                    return false;
                }

                try
                {
                    var entries = _pending.ToArrayAndTrim();
                    var payload = MobaActorSpawnSnapshotCodec.Serialize(entries);
                    _pending.Clear();
                    snapshot = new WorldStateSnapshot(AbilityKit.Protocol.Moba.MobaOpCodes.Snapshot.ActorSpawn, payload);
                    _lastFrame = frame;
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"[MobaActorSpawnSnapshotService] serialize pending spawn snapshot failed (frame={frame.Value}, pending={_pending.Count})");
                }
            }

            snapshot = default;
            return false;
        }

        public void Dispose()
        {
            _hasSnapshot = false;
            _sent = false;
            _snapshotPayload = null;
            _pending.Clear();
            _lastFrame = new FrameIndex(-999999);
        }
    }
}

using System;
using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;
using AbilityKit.Ability.World.Services;
using AbilityKit.Ability.World.Services.Attributes;
using AbilityKit.Demo.Moba.Services.Area;
using AbilityKit.Protocol.Moba.StateSync;

namespace AbilityKit.Demo.Moba.Services
{
    [MobaSnapshotEmitter(50)]
    [WorldService(typeof(MobaAreaEventSnapshotService))]
    public sealed class MobaAreaEventSnapshotService : IService, IMobaSnapshotEmitter
    {
        private readonly MobaLogicWorldRunGateService _phase;
        private readonly MobaAreaRuntimeService _areaRuntime;
        private readonly List<MobaAreaEventSnapshotEntry> _entries = new List<MobaAreaEventSnapshotEntry>(32);

        private FrameIndex _lastFrame;

        public MobaAreaEventSnapshotService(MobaLogicWorldRunGateService phase, MobaAreaRuntimeService areaRuntime)
        {
            _phase = phase ?? throw new ArgumentNullException(nameof(phase));
            _areaRuntime = areaRuntime ?? throw new ArgumentNullException(nameof(areaRuntime));
            _lastFrame = new FrameIndex(-999999);
        }

        public bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot)
        {
            if (!_phase.InGame)
            {
                snapshot = default;
                return false;
            }

            if (frame.Value == _lastFrame.Value)
            {
                snapshot = default;
                return false;
            }
            _lastFrame = frame;

            _entries.Clear();
            _areaRuntime.DrainPresentationEvents(_entries);
            if (_entries.Count == 0)
            {
                snapshot = default;
                return false;
            }

            var payload = MobaAreaEventSnapshotCodec.Serialize(_entries.ToArray());
            snapshot = new WorldStateSnapshot(AbilityKit.Protocol.Moba.MobaOpCodes.Snapshot.AreaEvent, payload);
            return true;
        }

        public void Dispose()
        {
            _entries.Clear();
            _lastFrame = new FrameIndex(-999999);
        }
    }
}

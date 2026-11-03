using System;
using AbilityKit.Ability.FrameSync.Rollback;
using AbilityKit.Ability.Host;
using AbilityKit.Core.Recording.Adapters.EventCodecs;
using AbilityKit.Core.Recording.Core;

namespace AbilityKit.Core.Recording.Adapters.Replay
{
    public sealed class TypedReplayEventHandler : IReplayEventHandler
    {
        public Action<PlayerInputCommand> OnInputCommand;
        public Action<int, WorldStateHash> OnStateHash;
        public Action<WorldStateSnapshot> OnSnapshot;
        public Action<WorldStateSnapshot> OnDelta;

        public void Handle(in RecordEvent e)
        {
            if (InputCommandEventCodec.TryRead(in e, out var cmd))
            {
                OnInputCommand?.Invoke(cmd);
                return;
            }

            if (StateHashEventCodec.TryRead(in e, out var version, out var hash))
            {
                OnStateHash?.Invoke(version, hash);
                return;
            }

            if (WorldSnapshotEventCodec.TryRead(in e, out var snap))
            {
                OnSnapshot?.Invoke(snap);
                return;
            }

            if (WorldDeltaEventCodec.TryRead(in e, out var delta))
            {
                OnDelta?.Invoke(delta);
            }
        }
    }
}

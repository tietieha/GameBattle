using System;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;
using AbilityKit.Ability.World.Services.Attributes;
using AbilityKit.Demo.Moba.Services.Snapshot;
using AbilityKit.Protocol.Moba.StateSync;

namespace AbilityKit.Demo.Moba.Services
{
    [MobaSnapshotEmitter(30)]
    [WorldService(typeof(MobaActorDespawnSnapshotService))]
    public sealed class MobaActorDespawnSnapshotService : LogicWorldSnapshotBufferEmitterBase<MobaActorDespawnSnapshotService, MobaActorDespawnSnapshotEntry>
    {
        private readonly MobaLogicWorldRunGateService _phase;

        public MobaActorDespawnSnapshotService(MobaLogicWorldRunGateService phase) : base(64, 512)
        {
            _phase = phase ?? throw new ArgumentNullException(nameof(phase));
        }

        public void Enqueue(int actorId, byte reason = 0)
        {
            if (actorId <= 0) return;
            Add(new MobaActorDespawnSnapshotEntry(actorId, reason));
        }

        protected override bool CanEmit(FrameIndex frame)
        {
            return _phase.InGame;
        }

        protected override WorldStateSnapshot CreateSnapshot(MobaActorDespawnSnapshotEntry[] entries)
        {
            var payload = MobaActorDespawnSnapshotCodec.Serialize(entries);
            return new WorldStateSnapshot(AbilityKit.Protocol.Moba.MobaOpCodes.Snapshot.ActorDespawn, payload);
        }
    }
}

using AbilityKit.Demo.Moba.Events.Summon;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public readonly struct RemoveSummonArgs
    {
        public readonly int SummonId;
        public readonly int SummonActorId;
        public readonly int RootOwnerActorId;
        public readonly bool RemoveAll;
        public readonly SummonDespawnReason Reason;
        public readonly MobaActionTargetRequest TargetRequest;

        public RemoveSummonArgs(
            int summonId,
            int summonActorId,
            int rootOwnerActorId,
            bool removeAll,
            SummonDespawnReason reason,
            in MobaActionTargetRequest targetRequest)
        {
            SummonId = summonId;
            SummonActorId = summonActorId;
            RootOwnerActorId = rootOwnerActorId;
            RemoveAll = removeAll;
            Reason = reason;
            TargetRequest = targetRequest;
        }
    }
}

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public readonly struct RemoveShieldArgs
    {
        public readonly int ShieldId;
        public readonly int InstanceId;
        public readonly int SourceActorId;
        public readonly bool RemoveAll;
        public readonly MobaActionTargetRequest TargetRequest;

        public RemoveShieldArgs(
            int shieldId,
            int instanceId,
            int sourceActorId,
            bool removeAll,
            in MobaActionTargetRequest targetRequest)
        {
            ShieldId = shieldId;
            InstanceId = instanceId;
            SourceActorId = sourceActorId;
            RemoveAll = removeAll;
            TargetRequest = targetRequest;
        }
    }
}

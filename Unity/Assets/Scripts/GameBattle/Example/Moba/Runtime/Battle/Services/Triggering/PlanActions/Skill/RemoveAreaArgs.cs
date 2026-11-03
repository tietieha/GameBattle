namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public readonly struct RemoveAreaArgs
    {
        public readonly int AreaId;
        public readonly int TemplateId;
        public readonly int OwnerActorId;
        public readonly bool RemoveAll;
        public readonly MobaActionTargetRequest TargetRequest;

        public RemoveAreaArgs(
            int areaId,
            int templateId,
            int ownerActorId,
            bool removeAll,
            in MobaActionTargetRequest targetRequest)
        {
            AreaId = areaId;
            TemplateId = templateId;
            OwnerActorId = ownerActorId;
            RemoveAll = removeAll;
            TargetRequest = targetRequest;
        }
    }
}

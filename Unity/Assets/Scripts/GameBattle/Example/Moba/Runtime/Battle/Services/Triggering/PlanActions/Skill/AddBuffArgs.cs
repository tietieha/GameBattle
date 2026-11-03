namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public struct AddBuffArgs
    {
        public int[] BuffIds;
        public MobaActionTargetRequest TargetRequest;

        public AddBuffArgs(int[] buffIds, in MobaActionTargetRequest targetRequest)
        {
            BuffIds = buffIds;
            TargetRequest = targetRequest;
        }
    }
}

using AbilityKit.Demo.Moba.Components;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public readonly struct ModifyResourceArgs
    {
        public readonly ResourceType ResourceType;
        public readonly float Amount;
        public readonly float MinValue;
        public readonly float MaxValue;
        public readonly bool HasMinValue;
        public readonly bool HasMaxValue;
        public readonly MobaActionTargetRequest TargetRequest;

        public ModifyResourceArgs(
            ResourceType resourceType,
            float amount,
            float minValue,
            float maxValue,
            bool hasMinValue,
            bool hasMaxValue,
            in MobaActionTargetRequest targetRequest)
        {
            ResourceType = resourceType;
            Amount = amount;
            MinValue = minValue;
            MaxValue = maxValue;
            HasMinValue = hasMinValue;
            HasMaxValue = hasMaxValue;
            TargetRequest = targetRequest;
        }
    }
}

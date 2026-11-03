using AbilityKit.Demo.Moba;
using AbilityKit.Demo.Moba.Components;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public readonly struct ConvertResourceToHealArgs
    {
        public readonly ResourceType ResourceType;
        public readonly float Amount;
        public readonly float HealRatio;
        public readonly float OutOfCombatSeconds;
        public readonly DamageType HealType;
        public readonly int ReasonKind;
        public readonly int ReasonParam;
        public readonly MobaActionTargetRequest TargetRequest;

        public ConvertResourceToHealArgs(
            ResourceType resourceType,
            float amount,
            float healRatio,
            float outOfCombatSeconds,
            DamageType healType,
            int reasonKind,
            int reasonParam,
            in MobaActionTargetRequest targetRequest)
        {
            ResourceType = resourceType;
            Amount = amount;
            HealRatio = healRatio;
            OutOfCombatSeconds = outOfCombatSeconds;
            HealType = healType;
            ReasonKind = reasonKind;
            ReasonParam = reasonParam;
            TargetRequest = targetRequest;
        }
    }
}

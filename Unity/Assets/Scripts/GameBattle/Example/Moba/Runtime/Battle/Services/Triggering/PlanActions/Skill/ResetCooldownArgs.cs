namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public readonly struct ResetCooldownArgs
    {
        public readonly int SkillId;
        public readonly int SkillSlot;
        public readonly MobaActionTargetRequest TargetRequest;

        public ResetCooldownArgs(int skillId, int skillSlot, in MobaActionTargetRequest targetRequest)
        {
            SkillId = skillId;
            SkillSlot = skillSlot;
            TargetRequest = targetRequest;
        }
    }
}

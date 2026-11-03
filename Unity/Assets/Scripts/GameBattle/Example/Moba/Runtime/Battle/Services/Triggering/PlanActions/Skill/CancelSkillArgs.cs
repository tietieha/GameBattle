namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public enum CancelSkillMode
    {
        Auto = 0,
        All = 1,
        Slot = 2,
        SkillId = 3,
    }

    public readonly struct CancelSkillArgs
    {
        public readonly CancelSkillMode Mode;
        public readonly int SkillId;
        public readonly int SkillSlot;
        public readonly bool RemoveAll;
        public readonly MobaActionTargetRequest TargetRequest;

        public CancelSkillArgs(
            CancelSkillMode mode,
            int skillId,
            int skillSlot,
            bool removeAll,
            in MobaActionTargetRequest targetRequest)
        {
            Mode = mode;
            SkillId = skillId;
            SkillSlot = skillSlot;
            RemoveAll = removeAll;
            TargetRequest = targetRequest;
        }
    }
}

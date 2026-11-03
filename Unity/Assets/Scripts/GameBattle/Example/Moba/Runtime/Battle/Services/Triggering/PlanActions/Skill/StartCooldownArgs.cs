namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public readonly struct StartCooldownArgs
    {
        public readonly int SkillId;
        public readonly int SkillSlot;
        public readonly int CooldownMs;

        public StartCooldownArgs(int skillId, int skillSlot, int cooldownMs)
        {
            SkillId = skillId;
            SkillSlot = skillSlot;
            CooldownMs = cooldownMs;
        }

        public static StartCooldownArgs Default => new StartCooldownArgs(0, 0, 0);
    }
}

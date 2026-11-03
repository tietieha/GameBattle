namespace AbilityKit.Triggering.Runtime.Plan
{
    public enum ETriggerExecutionMode : byte
    {
        Always = 0,
        Once = 1,
        Cooldown = 2,
        Repeat = 3,
    }

    public readonly struct TriggerExecutionControlPlan
    {
        public static TriggerExecutionControlPlan Always => default;

        public readonly ETriggerExecutionMode Mode;
        public readonly int MaxExecutions;
        public readonly float CooldownMs;

        public TriggerExecutionControlPlan(ETriggerExecutionMode mode, int maxExecutions = 0, float cooldownMs = 0f)
        {
            Mode = mode;
            MaxExecutions = maxExecutions;
            CooldownMs = cooldownMs;
        }

        public bool IsDefault => Mode == ETriggerExecutionMode.Always && MaxExecutions == 0 && CooldownMs <= 0f;
    }
}

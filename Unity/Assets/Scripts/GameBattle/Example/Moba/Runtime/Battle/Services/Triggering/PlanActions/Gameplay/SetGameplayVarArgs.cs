namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public readonly struct SetGameplayVarArgs
    {
        public readonly int KeyId;
        public readonly double Value;

        public SetGameplayVarArgs(int keyId, double value)
        {
            KeyId = keyId;
            Value = value;
        }
    }
}

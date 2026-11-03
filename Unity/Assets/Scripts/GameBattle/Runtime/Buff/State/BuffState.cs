namespace AbilityKit.Buffs
{
    /// <summary>Shared instance state; owner storage and genre-specific bindings belong to the caller.</summary>
    public class BuffState
    {
        public int BuffId;
        public int SourceId;
        public long SourceContextId;
        public long RuntimeContextId;
        public long RuntimeContextVersion;
        public AbilityKit.Context.ContextEntityReference RuntimeContextIdentity;
        public int StackCount;
        public float Remaining;
        public float IntervalRemainingSeconds;

        public void ClearRuntimeContext()
        {
            RuntimeContextId = 0;
            RuntimeContextVersion = 0;
            RuntimeContextIdentity = default;
        }
    }
}

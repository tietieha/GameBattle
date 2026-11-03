namespace AbilityKit.Buffs
{
    /// <summary>Shared instance state; owner storage and genre-specific bindings belong to the caller.</summary>
    public class BuffState
    {
        public int BuffId;
        public int SourceId;
        public long SourceContextId;
        public int StackCount;
        public float Remaining;
        public float IntervalRemainingSeconds;
    }
}

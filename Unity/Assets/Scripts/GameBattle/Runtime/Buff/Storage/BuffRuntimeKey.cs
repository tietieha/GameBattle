namespace AbilityKit.Buffs
{
    public readonly struct BuffRuntimeKey
    {
        public readonly int BuffId;
        public readonly int SourceActorId;
        public readonly long SourceContextId;

        private BuffRuntimeKey(int buffId, int sourceActorId, long sourceContextId)
        {
            BuffId = buffId;
            SourceActorId = sourceActorId;
            SourceContextId = sourceContextId;
        }

        public static BuffRuntimeKey MatchBuff(int buffId) => new BuffRuntimeKey(buffId, 0, 0);
        public static BuffRuntimeKey MatchBuffAndSource(int buffId, int sourceActorId) =>
            new BuffRuntimeKey(buffId, sourceActorId, 0);
        public static BuffRuntimeKey MatchInstance(int buffId, int sourceActorId, long sourceContextId) =>
            new BuffRuntimeKey(buffId, sourceActorId, sourceContextId);

        public bool Matches(BuffState runtime)
        {
            if (runtime == null) return false;
            if (BuffId > 0 && runtime.BuffId != BuffId) return false;
            if (SourceActorId > 0 && runtime.SourceId != SourceActorId) return false;
            if (SourceContextId != 0 && runtime.SourceContextId != SourceContextId) return false;
            return true;
        }
    }
}

namespace AbilityKit.Trace
{
    /// <summary>
    /// Common lifecycle reasons for trace nodes. Business layers may still pass custom integer reasons.
    /// </summary>
    public enum TraceLifecycleReason : byte
    {
        None = 0,
        Completed = 1,
        Cancelled = 2,
        Expired = 3,
        Dispelled = 4,
        Dead = 5,
        Replaced = 6,
        Interrupted = 7,
        Overridden = 8,
        Failed = 9,
    }

    /// <summary>
    /// Strongly typed trace endpoint descriptor used instead of arbitrary object payloads.
    /// </summary>
    public readonly struct TraceEndpoint
    {
        public readonly long Id;
        public readonly string DisplayName;

        public TraceEndpoint(long id, string displayName = null)
        {
            Id = id;
            DisplayName = displayName;
        }

        public bool IsValid => Id != 0 || !string.IsNullOrEmpty(DisplayName);

        public static TraceEndpoint Actor(long actorId)
        {
            return actorId > 0 ? new TraceEndpoint(actorId, string.Concat("Actor:", actorId)) : default;
        }

        public static TraceEndpoint Config(string prefix, int configId)
        {
            return configId > 0 ? new TraceEndpoint(configId, string.Concat(prefix, ":", configId)) : default;
        }
    }

    /// <summary>
    /// Strongly typed input used when creating a trace node.
    /// </summary>
    public readonly struct TraceOrigin
    {
        public readonly long ParentContextId;
        public readonly int Kind;
        public readonly long SourceActorId;
        public readonly long TargetActorId;
        public readonly TraceEndpoint OriginSource;
        public readonly TraceEndpoint OriginTarget;
        public readonly int ConfigId;
        public readonly long OriginContextId;

        public TraceOrigin(
            int kind,
            long sourceActorId = 0,
            long targetActorId = 0,
            TraceEndpoint originSource = default,
            TraceEndpoint originTarget = default,
            int configId = 0,
            long parentContextId = 0,
            long originContextId = 0)
        {
            ParentContextId = parentContextId;
            Kind = kind;
            SourceActorId = sourceActorId;
            TargetActorId = targetActorId;
            OriginSource = originSource;
            OriginTarget = originTarget;
            ConfigId = configId;
            OriginContextId = originContextId;
        }

        public TraceOrigin WithParent(long parentContextId)
        {
            return new TraceOrigin(Kind, SourceActorId, TargetActorId, OriginSource, OriginTarget, ConfigId, parentContextId, OriginContextId);
        }
    }
}

using System;

namespace AbilityKit.Demo.Moba.Services
{
    public enum MobaBattleRouteKind
    {
        Unknown = 0,
        RuntimeInput = 1,
        RuntimeSnapshot = 2,
        RuntimeOutputIntent = 3,
        RuntimeLifecycle = 4,

        [Obsolete("Use RuntimeInput. Route descriptors describe battle-runtime contracts, not client transport ownership.")]
        ClientInput = RuntimeInput,

        [Obsolete("Use RuntimeSnapshot. Route descriptors describe battle-runtime contracts, not server transport ownership.")]
        ServerSnapshot = RuntimeSnapshot,

        [Obsolete("Use RuntimeOutputIntent. Route descriptors describe data-only runtime output intent, not view commands.")]
        ViewCommand = RuntimeOutputIntent,

        [Obsolete("Use RuntimeLifecycle. Route descriptors describe runtime lifecycle contracts, not host/session lifecycle ownership.")]
        Lifecycle = RuntimeLifecycle,
    }

    public readonly struct MobaBattleRouteDescriptor
    {
        public MobaBattleRouteDescriptor(
            int opCode,
            MobaBattleRouteKind kind,
            Type ownerType,
            Type payloadType = null,
            Type handlerType = null,
            string name = null)
        {
            OpCode = opCode;
            Kind = kind;
            OwnerType = ownerType;
            PayloadType = payloadType;
            HandlerType = handlerType;
            Name = string.IsNullOrEmpty(name) ? ownerType?.Name : name;
        }

        public int OpCode { get; }

        public MobaBattleRouteKind Kind { get; }

        public Type OwnerType { get; }

        public Type PayloadType { get; }

        public Type HandlerType { get; }

        public string Name { get; }
    }
}

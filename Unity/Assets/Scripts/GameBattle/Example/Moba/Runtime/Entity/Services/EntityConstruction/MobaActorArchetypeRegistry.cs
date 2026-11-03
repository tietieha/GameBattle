using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Services.EntityConstruction
{
    public sealed class MobaActorArchetypeRegistry
    {
        private readonly Dictionary<MobaEntityKind, ActorArchetypeFactory.CreateHandler> _handlers = new Dictionary<MobaEntityKind, ActorArchetypeFactory.CreateHandler>();

        public void Register(MobaEntityKind kind, ActorArchetypeFactory.CreateHandler handler)
        {
            if (kind == MobaEntityKind.Unknown) throw new ArgumentException("kind cannot be Unknown", nameof(kind));
            _handlers[kind] = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public bool TryGet(MobaEntityKind kind, out ActorArchetypeFactory.CreateHandler handler)
        {
            return _handlers.TryGetValue(kind, out handler) && handler != null;
        }

        public static MobaActorArchetypeRegistry CreateDefault()
        {
            var registry = new MobaActorArchetypeRegistry();
            MobaActorArchetypeAssembler.RegisterDefaults(registry);
            return registry;
        }
    }
}

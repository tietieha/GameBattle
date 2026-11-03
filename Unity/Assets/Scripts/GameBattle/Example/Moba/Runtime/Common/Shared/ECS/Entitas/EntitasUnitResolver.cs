using System;
using System.Collections.Generic;
using AbilityKit.Ability.Share.ECS;
using AbilityKit.Ability.Share.ECS.Entitas;
using AbilityKit.ECS;

namespace AbilityKit.Ability.Share.ECS.Entitas
{
    public sealed class EntitasUnitResolver : IUnitResolver
    {
        private readonly EntitasActorIdLookup _lookup;
        private readonly Dictionary<int, EntitasUnitFacade> _cache = new Dictionary<int, EntitasUnitFacade>();

        public EntitasUnitResolver(EntitasActorIdLookup lookup)
        {
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
        }

        public bool TryResolve(EcsEntityId id, out IUnitFacade unit)
        {
            if (!id.IsValid)
            {
                unit = null;
                return false;
            }

            if (!_lookup.TryGet(id.ActorId, out _))
            {
                unit = null;
                return false;
            }

            if (_cache.TryGetValue(id.ActorId, out var cached) && cached != null)
            {
                unit = cached;
                return true;
            }

            // NOTE: 这里仅做 facade 缓存，避免重复包装同一 ActorId；Tags/Attributes/Effects 继续由组件层实时读取。
            var created = new EntitasUnitFacade(id.ActorId);
            _cache[id.ActorId] = created;
            unit = created;
            return true;
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}

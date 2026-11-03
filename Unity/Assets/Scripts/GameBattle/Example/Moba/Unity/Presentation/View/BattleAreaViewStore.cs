using System.Collections.Generic;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleAreaViewStore
    {
        private readonly Dictionary<int, BattleAreaViewHandle> _areaViews = new Dictionary<int, BattleAreaViewHandle>(128);

        public bool Contains(int areaId)
        {
            return _areaViews.ContainsKey(areaId);
        }

        public void Add(BattleAreaViewHandle handle)
        {
            if (handle == null) return;
            if (handle.AreaId <= 0) return;

            _areaViews[handle.AreaId] = handle;
        }

        public void Expire(int areaId)
        {
            if (!_areaViews.TryGetValue(areaId, out var handle)) return;

            handle?.Destroy();
            _areaViews.Remove(areaId);
        }

        public void Clear()
        {
            foreach (var kv in _areaViews)
            {
                kv.Value?.Destroy();
            }

            _areaViews.Clear();
        }
    }
}

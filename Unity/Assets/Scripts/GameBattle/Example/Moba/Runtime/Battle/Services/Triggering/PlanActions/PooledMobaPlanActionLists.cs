using System.Collections.Generic;
using AbilityKit.Core.Pooling;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    internal static class PooledMobaPlanActionLists
    {
        private static readonly ObjectPool<List<int>> s_intListPool = Pools.GetPool(
            createFunc: () => new List<int>(8),
            onRelease: list => list.Clear(),
            defaultCapacity: 16,
            maxSize: 256);

        public static List<int> GetIntList()
        {
            return s_intListPool.Get();
        }

        public static void Release(List<int> list)
        {
            if (list == null) return;
            s_intListPool.Release(list);
        }
    }
}

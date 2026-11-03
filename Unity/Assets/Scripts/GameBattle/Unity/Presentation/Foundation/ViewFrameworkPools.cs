using System;
using System.Collections.Generic;
using AbilityKit.Core.Pooling;

namespace AbilityKit.Game.View.Foundation
{
    public static class ViewFrameworkPools
    {
        public static PooledList<T> GetList<T>(int minimumCapacity = 0)
        {
            var pool = ListPool<T>.Pool;
            var list = pool.Get();
            list.Clear();

            if (minimumCapacity > 0 && list.Capacity < minimumCapacity)
            {
                list.Capacity = minimumCapacity;
            }

            return new PooledList<T>(pool, list);
        }

        private static class ListPool<T>
        {
            public static readonly ObjectPool<List<T>> Pool = new ObjectPool<List<T>>(
                new ObjectPoolOptions<List<T>>(() => new List<T>(4))
                {
                    OnGet = list => list.Clear(),
                    OnRelease = list => list.Clear(),
                    DefaultCapacity = 4,
                    MaxSize = 256
                });
        }
    }
}

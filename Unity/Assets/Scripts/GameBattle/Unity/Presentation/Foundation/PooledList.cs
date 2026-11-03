using System;
using System.Collections.Generic;
using AbilityKit.Core.Pooling;

namespace AbilityKit.Game.View.Foundation
{
    public readonly struct PooledList<T> : IDisposable
    {
        private readonly ObjectPool<List<T>> _pool;

        internal PooledList(ObjectPool<List<T>> pool, List<T> list)
        {
            _pool = pool;
            List = list ?? throw new ArgumentNullException(nameof(list));
        }

        public List<T> List { get; }

        public void Dispose()
        {
            if (List == null) return;
            _pool?.Release(List);
        }
    }
}

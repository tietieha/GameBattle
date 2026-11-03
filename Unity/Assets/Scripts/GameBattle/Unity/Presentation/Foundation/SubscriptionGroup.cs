using System;
using System.Collections.Generic;

namespace AbilityKit.Game.View.Foundation
{
    public sealed class SubscriptionGroup<T> : IDisposable where T : class
    {
        private readonly List<T> _items;
        private readonly Action<T> _release;
        private readonly Action<Exception>? _fail;
        private bool _isClearing;

        public SubscriptionGroup(Action<T> release, Action<Exception>? fail = null, int capacity = 4)
        {
            _release = release ?? throw new ArgumentNullException(nameof(release));
            _fail = fail;
            _items = new List<T>(Math.Max(0, capacity));
        }

        public int Count => _items.Count;

        public T? Add(T? subscription)
        {
            if (subscription != null)
            {
                _items.Add(subscription);
            }

            return subscription;
        }

        public bool Remove(T? subscription, bool release = true)
        {
            if (subscription == null) return false;
            var index = _items.IndexOf(subscription);
            if (index < 0) return false;

            _items.RemoveAt(index);
            if (release)
            {
                ReleaseOne(subscription);
            }

            return true;
        }

        public void Clear()
        {
            if (_isClearing) return;
            _isClearing = true;

            try
            {
                for (var i = _items.Count - 1; i >= 0; i--)
                {
                    ReleaseOne(_items[i]);
                }

                _items.Clear();
            }
            finally
            {
                _isClearing = false;
            }
        }

        public void Dispose()
        {
            Clear();
        }

        private void ReleaseOne(T item)
        {
            try
            {
                _release(item);
            }
            catch (Exception ex)
            {
                _fail?.Invoke(ex);
            }
        }
    }
}

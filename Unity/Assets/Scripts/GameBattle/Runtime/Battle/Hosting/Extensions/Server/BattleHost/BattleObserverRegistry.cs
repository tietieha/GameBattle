using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.Host.Extensions.Server.BattleHost
{
    public sealed class BattleObserverRegistry<TObserver>
    {
        private readonly List<TObserver> _observers = new List<TObserver>();

        public int Count => _observers.Count;

        public bool Subscribe(TObserver observer)
        {
            if (observer == null || _observers.Contains(observer))
            {
                return false;
            }

            _observers.Add(observer);
            return true;
        }

        public bool Unsubscribe(TObserver observer)
        {
            if (observer == null)
            {
                return false;
            }

            return _observers.Remove(observer);
        }

        public IReadOnlyList<TObserver> Snapshot()
        {
            return _observers.ToArray();
        }

        public void Clear()
        {
            _observers.Clear();
        }
    }
}

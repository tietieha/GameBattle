using System;
using AbilityKit.Core.Logging;
using AbilityKit.Game.View.Foundation;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleSubscriptionGroup : IDisposable
    {
        private readonly SubscriptionGroup<IDisposable> _inner;

        public BattleSubscriptionGroup(int capacity = 4)
        {
            _inner = new SubscriptionGroup<IDisposable>(
                subscription => subscription.Dispose(),
                ex => Log.Exception(ex),
                capacity);
        }

        public T Add<T>(T subscription) where T : class, IDisposable
        {
            _inner.Add(subscription);
            return subscription;
        }

        public void Clear()
        {
            _inner.Clear();
        }

        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}

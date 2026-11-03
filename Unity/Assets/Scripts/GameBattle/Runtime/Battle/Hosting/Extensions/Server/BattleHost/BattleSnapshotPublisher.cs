using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.Host.Extensions.Server.BattleHost
{
    public delegate TSnapshot BattleSnapshotFactory<out TSnapshot>(int frame, bool isFullSnapshot);

    public delegate TSnapshot BattleObserverSnapshotFactory<in TObserver, out TSnapshot>(TObserver observer, int frame, bool isFullSnapshot);

    public delegate void BattleSnapshotSender<in TObserver, in TSnapshot>(TObserver observer, TSnapshot snapshot);

    public delegate void BattleSnapshotPublishErrorHandler<in TObserver>(TObserver observer, Exception exception);

    public sealed class BattleSnapshotPublisher<TObserver, TSnapshot>
    {
        private readonly BattleSnapshotFactory<TSnapshot> _factory;
        private readonly BattleSnapshotSender<TObserver, TSnapshot> _sender;
        private readonly BattleSnapshotPublishErrorHandler<TObserver> _errorHandler;

        public BattleSnapshotPublisher(
            BattleSnapshotFactory<TSnapshot> factory,
            BattleSnapshotSender<TObserver, TSnapshot> sender,
            BattleSnapshotPublishErrorHandler<TObserver> errorHandler)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        }

        public int Publish(IReadOnlyList<TObserver> observers, int frame, bool isFullSnapshot)
        {
            if (observers == null || observers.Count == 0)
            {
                return 0;
            }

            var snapshot = _factory(frame, isFullSnapshot);
            var sentCount = 0;
            for (int i = 0; i < observers.Count; i++)
            {
                if (Send(observers[i], snapshot))
                {
                    sentCount++;
                }
            }

            return sentCount;
        }

        public int PublishPerObserver(IReadOnlyList<TObserver> observers, int frame, bool isFullSnapshot, BattleObserverSnapshotFactory<TObserver, TSnapshot> factory)
        {
            if (observers == null || observers.Count == 0)
            {
                return 0;
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var sentCount = 0;
            for (int i = 0; i < observers.Count; i++)
            {
                var observer = observers[i];
                var snapshot = factory(observer, frame, isFullSnapshot);
                if (Send(observer, snapshot))
                {
                    sentCount++;
                }
            }

            return sentCount;
        }

        public int PublishTo(TObserver observer, int frame, bool isFullSnapshot)
        {
            if (observer == null)
            {
                return 0;
            }

            var snapshot = _factory(frame, isFullSnapshot);
            return Send(observer, snapshot) ? 1 : 0;
        }

        public int PublishTo(TObserver observer, int frame, bool isFullSnapshot, BattleObserverSnapshotFactory<TObserver, TSnapshot> factory)
        {
            if (observer == null)
            {
                return 0;
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var snapshot = factory(observer, frame, isFullSnapshot);
            return Send(observer, snapshot) ? 1 : 0;
        }

        private bool Send(TObserver observer, TSnapshot snapshot)
        {
            try
            {
                _sender(observer, snapshot);
                return true;
            }
            catch (Exception ex)
            {
                _errorHandler(observer, ex);
                return false;
            }
        }
    }
}

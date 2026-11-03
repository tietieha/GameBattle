using System;

namespace AbilityKit.Game.View.Presentation
{
    public abstract class PresentationFacade<TSnapshot, TViewBatch>
        where TViewBatch : struct, IViewBatch
    {
        protected readonly IViewModelProjector<TSnapshot, TViewBatch> Projector;
        protected readonly IViewModelStore<TViewBatch> Store;
        protected readonly IViewStream<TViewBatch> Stream;

        protected PresentationFacade(
            IViewModelProjector<TSnapshot, TViewBatch> projector,
            IViewModelStore<TViewBatch> store,
            IViewStream<TViewBatch> stream)
        {
            Projector = projector ?? throw new ArgumentNullException(nameof(projector));
            Store = store ?? throw new ArgumentNullException(nameof(store));
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public TViewBatch Current => Store.Current;

        protected TViewBatch ApplyAndPublish(in TSnapshot snapshot)
        {
            var batch = Projector.Project(in snapshot);
            Store.Apply(in batch);
            Stream.Publish(in batch);
            return batch;
        }

        public virtual void Clear()
        {
            Store.Clear();
        }
    }
}

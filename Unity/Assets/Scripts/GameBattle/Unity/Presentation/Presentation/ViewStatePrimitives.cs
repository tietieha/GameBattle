using System;

namespace AbilityKit.Game.View.Presentation
{
    public sealed class ViewModelStore<TViewBatch> : IViewModelStore<TViewBatch>
        where TViewBatch : struct, IViewBatch
    {
        public TViewBatch Current { get; private set; }

        public void Apply(in TViewBatch batch)
        {
            Current = batch;
        }

        public void Clear()
        {
            Current = default;
        }
    }

    public sealed class ViewStream<TViewBatch> : IViewStream<TViewBatch>
        where TViewBatch : struct, IViewBatch
    {
        public event Action<TViewBatch>? BatchApplied;

        public void Publish(in TViewBatch batch)
        {
            BatchApplied?.Invoke(batch);
        }
    }
}

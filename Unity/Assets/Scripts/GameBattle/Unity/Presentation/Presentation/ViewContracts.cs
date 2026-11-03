namespace AbilityKit.Game.View.Presentation
{
    public interface IViewBatch
    {
        ulong WorldId { get; }
        int Frame { get; }
        ulong Sequence { get; }
    }

    public interface IViewModelProjector<TSnapshot, TViewBatch>
        where TViewBatch : struct, IViewBatch
    {
        TViewBatch Project(in TSnapshot snapshot);
    }

    public interface IViewModelStore<TViewBatch>
        where TViewBatch : struct, IViewBatch
    {
        TViewBatch Current { get; }
        void Apply(in TViewBatch batch);
        void Clear();
    }

    public interface IViewStream<TViewBatch>
        where TViewBatch : struct, IViewBatch
    {
        event System.Action<TViewBatch> BatchApplied;
        void Publish(in TViewBatch batch);
    }

    public interface IViewSink<TViewBatch>
        where TViewBatch : struct, IViewBatch
    {
        void ApplyBatch(in TViewBatch batch);
        void Clear();
    }
}

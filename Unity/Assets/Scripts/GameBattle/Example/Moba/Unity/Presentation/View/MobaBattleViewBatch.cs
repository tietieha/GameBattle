#nullable enable

using AbilityKit.Game.View.Presentation;

namespace AbilityKit.Game.Flow
{
    public readonly struct MobaBattleViewBatch : IViewBatch
    {
        public MobaBattleViewBatch(
            ulong worldId,
            int frame,
            ulong sequence,
            MobaBattleViewBatchSource source,
            int actorCount,
            int visibleActorCount,
            int presentationCueCount)
        {
            WorldId = worldId;
            Frame = frame;
            Sequence = sequence;
            Source = source;
            ActorCount = actorCount < 0 ? 0 : actorCount;
            VisibleActorCount = visibleActorCount < 0 ? 0 : visibleActorCount;
            PresentationCueCount = presentationCueCount < 0 ? 0 : presentationCueCount;
        }

        public ulong WorldId { get; }

        public int Frame { get; }

        public ulong Sequence { get; }

        public MobaBattleViewBatchSource Source { get; }

        public int ActorCount { get; }

        public int VisibleActorCount { get; }

        public int PresentationCueCount { get; }

        public bool HasActors => ActorCount > 0;

        public bool HasPresentationCues => PresentationCueCount > 0;

        public static MobaBattleViewBatch Empty { get; } = new MobaBattleViewBatch(
            0UL,
            0,
            0UL,
            MobaBattleViewBatchSource.Debug,
            0,
            0,
            0);
    }

    public enum MobaBattleViewBatchSource
    {
        Snapshot = 1,
        InterpolatedSnapshot = 2,
        TriggerEvent = 3,
        Debug = 4
    }

    public interface IMobaBattleViewSink : IViewSink<MobaBattleViewBatch>
    {
        void ApplyBattleView(in MobaBattleViewBatch batch);

        new void Clear();

        void IViewSink<MobaBattleViewBatch>.ApplyBatch(in MobaBattleViewBatch batch)
        {
            ApplyBattleView(in batch);
        }
    }
}

using AbilityKit.Demo.Moba.Services;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan.Json;

namespace AbilityKit.Demo.Moba.Triggering
{
    public sealed class MobaPresentationCueFactory : TriggerPlanJsonDatabase.ICueFactory
    {
        private readonly MobaPresentationCueSnapshotService _snapshots;
        private readonly IMobaPresentationCueResolver _resolver;

        public MobaPresentationCueFactory(MobaPresentationCueSnapshotService snapshots)
            : this(snapshots, null)
        {
        }

        public MobaPresentationCueFactory(MobaPresentationCueSnapshotService snapshots, IMobaPresentationCueResolver resolver)
        {
            _snapshots = snapshots;
            _resolver = resolver ?? new MobaPresentationCueResolver();
        }

        public ITriggerCue Create(in TriggerCueDescriptor descriptor)
        {
            // 即使触发器级 descriptor 为空，也保留一个可接收 context.CueDescriptor 的 cue 实例，
            // 这样行为级 cue 可以独立通过 ActionCallPlan.Cue 派发到表现层。
            return new MobaPresentationTriggerCue(_snapshots, _resolver, in descriptor);
        }
    }
}

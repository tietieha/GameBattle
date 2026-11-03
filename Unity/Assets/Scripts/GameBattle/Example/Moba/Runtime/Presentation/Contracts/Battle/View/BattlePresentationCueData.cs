using System;
using System.Collections.Generic;
using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public enum BattlePresentationCueStage
    {
        None = 0,
        ConditionPassed = 1,
        ConditionFailed = 2,
        BeforeAction = 3,
        Executed = 4,
        Interrupted = 5,
        Skipped = 6,
        Started = 20,
        Ticked = 21,
        Refreshed = 22,
        StackChanged = 23,
        Expired = 24,
        Removed = 25,
        Completed = 26,
    }

    public readonly struct BattlePresentationCueData
    {
        public BattlePresentationCueData(
            BattlePresentationCueStage stage,
            string cueKind,
            string cueVfxId,
            string cueSfxId,
            int templateId,
            int vfxId,
            int sfxId,
            string requestKey,
            int sourceActorId,
            int targetActorId,
            int triggerEventId,
            string triggerEventName,
            int triggerId,
            int phase,
            int priority,
            int order,
            int actionIndex,
            int interruptReason,
            string interruptSourceName,
            int interruptTriggerId,
            bool interruptConditionPassed,
            IReadOnlyList<int> targets,
            IReadOnlyList<MobaFloat3> positions,
            MobaFloat3 offset,
            int durationMsOverride,
            float scale,
            MobaColor32 color,
            string ownerKind = null,
            long instanceId = 0,
            string instanceKey = null,
            int stackCount = 0,
            int maxStackCount = 0,
            float elapsedSeconds = 0f,
            float remainingSeconds = 0f,
            int lifecycleReason = 0,
            int contextKind = 0,
            int originKind = 0,
            long sourceContextId = 0,
            long rootContextId = 0,
            long ownerContextId = 0,
            int sourceConfigId = 0,
            string contextEventId = null,
            IReadOnlyList<int> numericParamKeys = null,
            IReadOnlyList<float> numericParamValues = null,
            IReadOnlyList<string> stringParamKeys = null,
            IReadOnlyList<string> stringParamValues = null)
        {
            Stage = stage;
            CueKind = cueKind;
            CueVfxId = cueVfxId;
            CueSfxId = cueSfxId;
            TemplateId = templateId;
            VfxId = vfxId;
            SfxId = sfxId;
            RequestKey = requestKey;
            SourceActorId = sourceActorId;
            TargetActorId = targetActorId;
            TriggerEventId = triggerEventId;
            TriggerEventName = triggerEventName;
            TriggerId = triggerId;
            Phase = phase;
            Priority = priority;
            Order = order;
            ActionIndex = actionIndex;
            InterruptReason = interruptReason;
            InterruptSourceName = interruptSourceName;
            InterruptTriggerId = interruptTriggerId;
            InterruptConditionPassed = interruptConditionPassed;
            Targets = targets ?? Array.Empty<int>();
            Positions = positions ?? Array.Empty<MobaFloat3>();
            Offset = offset;
            DurationMsOverride = durationMsOverride;
            Scale = scale;
            Color = color;
            OwnerKind = ownerKind;
            InstanceId = instanceId;
            InstanceKey = instanceKey;
            StackCount = stackCount;
            MaxStackCount = maxStackCount;
            ElapsedSeconds = elapsedSeconds;
            RemainingSeconds = remainingSeconds;
            LifecycleReason = lifecycleReason;
            ContextKind = contextKind;
            OriginKind = originKind;
            SourceContextId = sourceContextId;
            RootContextId = rootContextId;
            OwnerContextId = ownerContextId;
            SourceConfigId = sourceConfigId;
            ContextEventId = contextEventId;
            NumericParamKeys = numericParamKeys ?? Array.Empty<int>();
            NumericParamValues = numericParamValues ?? Array.Empty<float>();
            StringParamKeys = stringParamKeys ?? Array.Empty<string>();
            StringParamValues = stringParamValues ?? Array.Empty<string>();
        }

        public BattlePresentationCueStage Stage { get; }
        public string CueKind { get; }
        public string CueVfxId { get; }
        public string CueSfxId { get; }
        public int TemplateId { get; }
        public int VfxId { get; }
        public int SfxId { get; }
        public string RequestKey { get; }
        public int SourceActorId { get; }
        public int TargetActorId { get; }
        public int TriggerEventId { get; }
        public string TriggerEventName { get; }
        public int TriggerId { get; }
        public int Phase { get; }
        public int Priority { get; }
        public int Order { get; }
        public int ActionIndex { get; }
        public int InterruptReason { get; }
        public string InterruptSourceName { get; }
        public int InterruptTriggerId { get; }
        public bool InterruptConditionPassed { get; }
        public IReadOnlyList<int> Targets { get; }
        public IReadOnlyList<MobaFloat3> Positions { get; }
        public MobaFloat3 Offset { get; }
        public int DurationMsOverride { get; }
        public float Scale { get; }
        public MobaColor32 Color { get; }
        public string OwnerKind { get; }
        public long InstanceId { get; }
        public string InstanceKey { get; }
        public int StackCount { get; }
        public int MaxStackCount { get; }
        public float ElapsedSeconds { get; }
        public float RemainingSeconds { get; }
        public int LifecycleReason { get; }
        public int ContextKind { get; }
        public int OriginKind { get; }
        public long SourceContextId { get; }
        public long RootContextId { get; }
        public long OwnerContextId { get; }
        public int SourceConfigId { get; }
        public string ContextEventId { get; }
        public IReadOnlyList<int> NumericParamKeys { get; }
        public IReadOnlyList<float> NumericParamValues { get; }
        public IReadOnlyList<string> StringParamKeys { get; }
        public IReadOnlyList<string> StringParamValues { get; }
    }
}

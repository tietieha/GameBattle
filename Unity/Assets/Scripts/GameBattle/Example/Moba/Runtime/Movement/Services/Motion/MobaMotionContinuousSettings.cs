using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Services.Motion
{
    public readonly struct MobaMotionContinuousSettings
    {
        public MobaMotionContinuousSettings(
            int continuousProcessId,
            int continuousTagTemplateId,
            IReadOnlyList<int> triggerIds,
            int intervalMs,
            IReadOnlyList<int> intervalTriggerIds)
        {
            ContinuousProcessId = continuousProcessId;
            ContinuousTagTemplateId = continuousTagTemplateId;
            TriggerIds = triggerIds ?? Array.Empty<int>();
            IntervalMs = intervalMs;
            IntervalTriggerIds = intervalTriggerIds ?? Array.Empty<int>();
        }

        public int ContinuousProcessId { get; }
        public int ContinuousTagTemplateId { get; }
        public IReadOnlyList<int> TriggerIds { get; }
        public int IntervalMs { get; }
        public IReadOnlyList<int> IntervalTriggerIds { get; }

        public static MobaMotionContinuousSettings Empty => new MobaMotionContinuousSettings(0, 0, Array.Empty<int>(), 0, Array.Empty<int>());
    }
}

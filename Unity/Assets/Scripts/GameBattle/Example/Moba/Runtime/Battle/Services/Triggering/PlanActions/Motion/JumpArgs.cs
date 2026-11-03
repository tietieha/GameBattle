using System;
using System.Collections.Generic;
using AbilityKit.Demo.Moba.Services.Motion;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public readonly struct JumpArgs
    {
        public readonly float Height;
        public readonly float DurationMs;
        public readonly int Priority;
        public readonly bool ApplyToCaster;
        public readonly int MotionGroupId;
        public readonly MobaMotionContinuousSettings Continuous;
        public readonly IReadOnlyList<int> LandingTriggerIds;

        public JumpArgs(
            float height,
            float durationMs,
            int priority = 10,
            bool applyToCaster = true,
            int motionGroupId = 0,
            MobaMotionContinuousSettings continuous = default,
            IReadOnlyList<int> landingTriggerIds = null)
        {
            Height = height;
            DurationMs = durationMs;
            Priority = priority;
            ApplyToCaster = applyToCaster;
            MotionGroupId = motionGroupId;
            Continuous = continuous;
            LandingTriggerIds = landingTriggerIds ?? Array.Empty<int>();
        }

        public static JumpArgs Default => new JumpArgs(0f, 0f, 10, true, 0, MobaMotionContinuousSettings.Empty, Array.Empty<int>());
    }
}

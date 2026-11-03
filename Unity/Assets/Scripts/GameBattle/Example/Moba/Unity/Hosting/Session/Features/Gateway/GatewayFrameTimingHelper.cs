using System;
using AbilityKit.Game.Battle.Agent;

namespace AbilityKit.Game.Flow
{
    internal readonly struct GatewayFrameTimingInput
    {
        public readonly GatewayWorldStartAnchor Anchor;
        public readonly bool HasClockSync;
        public readonly double ClockOffsetSecondsEwma;
        public readonly double RttSecondsEwma;
        public readonly BattleStartPlanTimeSyncOptions TimeSync;

        public GatewayFrameTimingInput(
            in GatewayWorldStartAnchor anchor,
            bool hasClockSync,
            double clockOffsetSecondsEwma,
            double rttSecondsEwma,
            in BattleStartPlanTimeSyncOptions timeSync)
        {
            Anchor = anchor;
            HasClockSync = hasClockSync;
            ClockOffsetSecondsEwma = clockOffsetSecondsEwma;
            RttSecondsEwma = rttSecondsEwma;
            TimeSync = timeSync;
        }
    }

    internal static class GatewayFrameTimingHelper
    {
        public static int ResolveIdealFrameRaw(in GatewayFrameTimingInput input, double localNowSeconds)
        {
            if (!IsReady(in input)) return 0;

            var elapsed = ResolveElapsedSeconds(in input, localNowSeconds);
            var dt = input.Anchor.FixedDeltaSeconds;
            if (dt <= 0) return 0;

            var frames = (int)Math.Floor(elapsed / dt);
            return input.Anchor.StartFrame + frames;
        }

        public static int ResolveIdealFrameSafetyMarginFrames(in GatewayFrameTimingInput input)
        {
            if (!IsReady(in input)) return 0;

            var dt = input.Anchor.FixedDeltaSeconds;
            if (dt <= 0) return 0;

            var timeSync = input.TimeSync;
            var constMargin = timeSync.IdealFrameSafetyConstMarginFrames;
            if (constMargin < 0) constMargin = 0;

            var rttFactor = timeSync.IdealFrameSafetyRttFactor;
            if (rttFactor < 0) rttFactor = 0;

            var rttFrames = (int)Math.Ceiling((input.RttSecondsEwma / dt) * rttFactor);
            if (rttFrames < 0) rttFrames = 0;

            var margin = constMargin;
            if (rttFrames > margin) margin = rttFrames;

            var minMargin = timeSync.IdealFrameSafetyMinMarginFrames;
            var maxMargin = timeSync.IdealFrameSafetyMaxMarginFrames;
            if (minMargin < 0) minMargin = 0;
            if (maxMargin < minMargin) maxMargin = minMargin;

            if (margin < minMargin) margin = minMargin;
            if (margin > maxMargin) margin = maxMargin;

            return margin;
        }

        public static int ResolveIdealFrameLimit(in GatewayFrameTimingInput input, double localNowSeconds)
        {
            if (!IsReady(in input)) return 0;

            var idealRaw = ResolveIdealFrameRaw(in input, localNowSeconds);
            var margin = ResolveIdealFrameSafetyMarginFrames(in input);
            var limit = idealRaw - margin;
            if (limit < input.Anchor.StartFrame) limit = input.Anchor.StartFrame;
            return limit;
        }

        private static bool IsReady(in GatewayFrameTimingInput input)
        {
            return input.HasClockSync && input.Anchor.ServerTickFrequency != 0;
        }

        private static double ResolveElapsedSeconds(in GatewayFrameTimingInput input, double localNowSeconds)
        {
            var startServerSeconds = input.Anchor.StartServerTicks / (double)input.Anchor.ServerTickFrequency;
            var localStartSeconds = startServerSeconds + input.ClockOffsetSecondsEwma;
            var elapsed = localNowSeconds - localStartSeconds;
            return elapsed < 0 ? 0 : elapsed;
        }
    }
}

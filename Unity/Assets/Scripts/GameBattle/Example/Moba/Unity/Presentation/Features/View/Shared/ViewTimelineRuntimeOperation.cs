using AbilityKit.Game.Flow.Battle.Modules;

namespace AbilityKit.Game.Flow
{
    internal readonly struct ViewTimelineAlignmentDecision
    {
        public readonly bool ShouldSeek;
        public readonly int Frame;
        public readonly float SecondsPerFrame;

        public ViewTimelineAlignmentDecision(bool shouldSeek, int frame, float secondsPerFrame)
        {
            ShouldSeek = shouldSeek;
            Frame = frame;
            SecondsPerFrame = secondsPerFrame;
        }
    }

    internal sealed class ViewTimelineRuntimeOperation
    {
        public static ViewTimelineAlignmentDecision ResolveAlignment(int currentFrame, int lastAlignedFrame, int tickRate)
        {
            if (currentFrame == lastAlignedFrame) return new ViewTimelineAlignmentDecision(false, currentFrame, 0f);

            var secondsPerFrame = tickRate > 0 ? 1f / tickRate : 0f;
            return new ViewTimelineAlignmentDecision(true, currentFrame, secondsPerFrame);
        }

        public void SeekAllToCurrentFrame(IViewFeatureRuntime runtime)
        {
            if (runtime?.Timeline == null) return;

            var battleCtx = runtime.Context;
            if (battleCtx == null) return;

            var decision = ResolveAlignment(
                battleCtx.LastFrame,
                runtime.LastAlignedFrame,
                battleCtx.Plan.World.TickRate);
            if (!decision.ShouldSeek) return;

            runtime.Timeline.SeekAll(decision.Frame, decision.SecondsPerFrame);

            runtime.LastAlignedFrame = decision.Frame;

            var worldId = battleCtx.RuntimeWorldId;
            battleCtx.Hooks?.ViewFrameAligned.Invoke(new ViewFrameAlignedEvent(isConfirmed: runtime.IsConfirmed, worldId: worldId, frame: decision.Frame));
        }
    }
}

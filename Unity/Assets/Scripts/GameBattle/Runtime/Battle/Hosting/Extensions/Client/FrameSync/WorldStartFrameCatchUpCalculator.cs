#nullable enable

using System;

namespace AbilityKit.Ability.Host.Extensions.Client.FrameSync
{
    public readonly struct WorldStartFrameAnchor
    {
        public readonly long StartServerTicks;
        public readonly long ServerTickFrequency;
        public readonly int StartFrame;
        public readonly double FixedDeltaSeconds;

        public WorldStartFrameAnchor(long startServerTicks, long serverTickFrequency, int startFrame, double fixedDeltaSeconds)
        {
            StartServerTicks = startServerTicks;
            ServerTickFrequency = serverTickFrequency;
            StartFrame = startFrame;
            FixedDeltaSeconds = fixedDeltaSeconds;
        }

        public bool IsValid => StartServerTicks > 0L && ServerTickFrequency > 0L && FixedDeltaSeconds > 0d;
    }

    public readonly struct WorldFrameCatchUpResult
    {
        public readonly int TargetFrame;
        public readonly int CatchUpFrames;
        public readonly double ElapsedSeconds;
        public readonly bool AnchorValid;

        public WorldFrameCatchUpResult(int targetFrame, int catchUpFrames, double elapsedSeconds, bool anchorValid)
        {
            TargetFrame = targetFrame;
            CatchUpFrames = catchUpFrames;
            ElapsedSeconds = elapsedSeconds;
            AnchorValid = anchorValid;
        }
    }

    public static class WorldStartFrameCatchUpCalculator
    {
        public static WorldFrameCatchUpResult Calculate(in WorldStartFrameAnchor anchor, long serverNowTicks)
        {
            if (!anchor.IsValid || serverNowTicks <= anchor.StartServerTicks)
            {
                return new WorldFrameCatchUpResult(anchor.StartFrame, 0, 0d, anchor.IsValid);
            }

            var elapsedSeconds = (serverNowTicks - anchor.StartServerTicks) / (double)anchor.ServerTickFrequency;
            var elapsedFrames = (int)Math.Floor(elapsedSeconds / anchor.FixedDeltaSeconds);
            if (elapsedFrames <= 0)
            {
                return new WorldFrameCatchUpResult(anchor.StartFrame, 0, elapsedSeconds, anchorValid: true);
            }

            var targetFrame = anchor.StartFrame + elapsedFrames;
            return new WorldFrameCatchUpResult(targetFrame, elapsedFrames, elapsedSeconds, anchorValid: true);
        }

        public static WorldFrameCatchUpResult CalculateFromSnapshotFrame(in WorldStartFrameAnchor anchor, long serverNowTicks, int snapshotFrame)
        {
            var result = Calculate(in anchor, serverNowTicks);
            var catchUpFrames = Math.Max(0, result.TargetFrame - snapshotFrame);
            return new WorldFrameCatchUpResult(result.TargetFrame, catchUpFrames, result.ElapsedSeconds, result.AnchorValid);
        }
    }
}

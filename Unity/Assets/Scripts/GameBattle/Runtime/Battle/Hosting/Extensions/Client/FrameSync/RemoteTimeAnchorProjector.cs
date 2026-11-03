#nullable enable

using System;
using AbilityKit.Network.Runtime.Sync;

namespace AbilityKit.Ability.Host.Extensions.Client.FrameSync
{
    public readonly struct RemoteTimeAnchorProjection
    {
        public RemoteTimeAnchorProjection(
            bool anchorValid,
            long serverNowTicks,
            int targetFrame,
            int catchUpFrames,
            double elapsedSeconds,
            SyncTimeAnchor timeAnchor)
        {
            AnchorValid = anchorValid;
            ServerNowTicks = serverNowTicks;
            TargetFrame = targetFrame;
            CatchUpFrames = catchUpFrames;
            ElapsedSeconds = elapsedSeconds;
            TimeAnchor = timeAnchor;
        }

        public bool AnchorValid { get; }

        public long ServerNowTicks { get; }

        public int TargetFrame { get; }

        public int CatchUpFrames { get; }

        public double ElapsedSeconds { get; }

        public SyncTimeAnchor TimeAnchor { get; }
    }

    public static class RemoteTimeAnchorProjector
    {
        public static RemoteTimeAnchorProjection Project(in WorldStartFrameAnchor worldStartAnchor, long serverNowTicks)
        {
            if (!worldStartAnchor.IsValid || serverNowTicks <= 0L)
            {
                return default;
            }

            var catchUp = WorldStartFrameCatchUpCalculator.Calculate(in worldStartAnchor, serverNowTicks);
            if (!catchUp.AnchorValid)
            {
                return default;
            }

            var timelineTicks = Math.Max(0, catchUp.TargetFrame - worldStartAnchor.StartFrame);
            var anchor = SyncTimeAnchor
                .FromLocalFrame(catchUp.TargetFrame, timelineTicks, catchUp.ElapsedSeconds)
                .WithAuthoritativeFrame(catchUp.TargetFrame)
                .WithServerTicks(serverNowTicks);

            return new RemoteTimeAnchorProjection(
                true,
                serverNowTicks,
                catchUp.TargetFrame,
                catchUp.CatchUpFrames,
                catchUp.ElapsedSeconds,
                anchor);
        }
    }
}

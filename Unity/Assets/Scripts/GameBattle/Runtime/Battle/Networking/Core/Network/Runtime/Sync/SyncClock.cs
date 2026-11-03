#nullable enable

using System;

namespace AbilityKit.Network.Runtime.Sync
{
    /// <summary>
    /// 同步栈的单一时间源：推进本地帧时间线，并在 <see cref="ServerClockEstimator"/> 已观测到往返样本后，
    /// 为每个产出的 <see cref="SyncTimeAnchor"/> 标记估算服务器时钟。它是时间锚点的唯一工厂，
    /// 让预测、插值、回溯与演示播放共享同一套时钟。
    /// </summary>
    public sealed class SyncClock
    {
        private readonly double _deltaSeconds;
        private readonly long _timelineTicksPerStep;
        private int _localFrame;

        public SyncClock(double deltaSeconds, long timelineTicksPerStep = 1L, ServerClockEstimator? serverClock = null)
        {
            if (deltaSeconds <= 0d) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (timelineTicksPerStep <= 0L) throw new ArgumentOutOfRangeException(nameof(timelineTicksPerStep));

            _deltaSeconds = deltaSeconds;
            _timelineTicksPerStep = timelineTicksPerStep;
            ServerClock = serverClock;
        }

        /// <summary>
        /// 可选的服务器时钟估算器。当它存在且已有样本时，时间锚点会携带估算的 <see cref="SyncTimeAnchor.ServerTicks"/>。
        /// </summary>
        public ServerClockEstimator? ServerClock { get; }

        /// <summary><see cref="Advance"/> 下一次将发出的本地帧索引。</summary>
        public int LocalFrame => _localFrame;

        /// <summary>每次 <see cref="Advance"/> 调用推进的秒数。</summary>
        public double DeltaSeconds => _deltaSeconds;

        /// <summary>
        /// 产出当前帧的时间锚点，然后推进本地帧计数器。
        /// </summary>
        public SyncTimeAnchor Advance()
        {
            var anchor = AnchorFor(_localFrame);
            _localFrame++;
            return anchor;
        }

        /// <summary>
        /// 为显式帧索引构建时间锚点，不推进内部状态。适用于拥有自身帧计数器的重放或确定性 harness 循环。
        /// </summary>
        public SyncTimeAnchor AnchorFor(int localFrame)
        {
            if (localFrame < 0) throw new ArgumentOutOfRangeException(nameof(localFrame));

            var timelineTicks = localFrame * _timelineTicksPerStep;
            var elapsedSeconds = localFrame * _deltaSeconds;
            var anchor = SyncTimeAnchor.FromLocalFrame(localFrame, timelineTicks, elapsedSeconds);

            if (ServerClock is { HasSample: true })
            {
                anchor = ServerClock.StampServerTicks(anchor, timelineTicks);
            }

            return anchor;
        }

        /// <summary>将本地帧计数器重置为零。</summary>
        public void Reset()
        {
            _localFrame = 0;
        }
    }
}

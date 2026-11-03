#nullable enable

using System;

namespace AbilityKit.Network.Runtime.Sync
{
    /// <summary>
    /// 时间同步交换与同步时钟之间的协议无关接缝。它持有一个 <see cref="ServerClockEstimator"/>，
    /// 将每次请求/响应往返折叠进去，并提供接入该估算器的 <see cref="SyncClock"/>，让产出的 anchor 在样本充分
    /// 收敛后携带估算服务器时钟。
    /// <para>
    /// 该 bridge 只接收原始 <see cref="long"/> tick 值；具体线协议消息（例如房间网关时间同步响应）的映射发生在
    /// 协议边界，因此本包永远不依赖任何协议包。调用方负责确保三个 tick 时间戳都使用 <see cref="TickFrequency"/>
    /// 确立的同一 tick 单位。
    /// </para>
    /// </summary>
    public sealed class TimeSyncBridge
    {
        private readonly ServerClockEstimator _estimator;

        public TimeSyncBridge(long tickFrequency)
        {
            _estimator = new ServerClockEstimator(tickFrequency);
        }

        /// <summary>客户端与服务器时间戳共享的 tick 单位。</summary>
        public long TickFrequency => _estimator.ServerTickFrequency;

        /// <summary>至少一个往返样本已折叠后为 true。</summary>
        public bool HasConverged => _estimator.HasSample;

        /// <summary>目前已观测到的往返样本数量。</summary>
        public int SampleCount => _estimator.SampleCount;

        /// <summary>目前观测到的最佳（最低）往返延迟，单位为秒。</summary>
        public double BestRoundTripSeconds => _estimator.BestRoundTripSeconds;

        /// <summary>当前最佳估算的客户端到服务器时钟偏移，单位为秒。</summary>
        public double OffsetSeconds => _estimator.OffsetSeconds;

        /// <summary>底层估算器，暴露给诊断与时钟接线使用。</summary>
        public ServerClockEstimator Estimator => _estimator;

        /// <summary>
        /// 将一次时间同步交换折叠进估算器。三个时间戳对应典型请求/响应流程：客户端在
        /// <paramref name="clientSendTicks"/> 发送，服务器以 <paramref name="serverNowTicks"/> 上报自身时钟，
        /// 客户端在 <paramref name="clientReceiveTicks"/> 收到响应。
        /// </summary>
        /// <returns>当该样本改善了最佳往返估算时为 true。</returns>
        public bool ObserveResponse(long clientSendTicks, long serverNowTicks, long clientReceiveTicks)
        {
            return _estimator.ObserveRoundTrip(clientSendTicks, serverNowTicks, clientReceiveTicks);
        }

        /// <summary>
        /// 产出接入该 bridge 估算器的 <see cref="SyncClock"/>。当 <see cref="HasConverged"/> 为 true 后，
        /// 它发出的 anchor 会携带估算服务器时钟。
        /// </summary>
        public SyncClock CreateClock(double deltaSeconds, long timelineTicksPerStep = 1L)
        {
            return new SyncClock(deltaSeconds, timelineTicksPerStep, _estimator);
        }
    }
}

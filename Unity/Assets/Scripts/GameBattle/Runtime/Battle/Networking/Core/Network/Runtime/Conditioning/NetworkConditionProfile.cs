#nullable enable

namespace AbilityKit.Network.Runtime.Conditioning
{
    /// <summary>
    /// A reproducible description of a simulated network environment.
    /// Gameplay-agnostic: any sample (Shooter, Moba) can run any sync model under any profile.
    /// Latency is expressed one-way in milliseconds; combine inbound and outbound to model RTT.
    /// </summary>
    public readonly struct NetworkConditionProfile
    {
        public NetworkConditionProfile(
            int baseLatencyMs,
            int jitterMs,
            double packetLossRate,
            double reorderRate,
            int bandwidthKbps)
        {
            BaseLatencyMs = baseLatencyMs < 0 ? 0 : baseLatencyMs;
            JitterMs = jitterMs < 0 ? 0 : jitterMs;
            PacketLossRate = Clamp01(packetLossRate);
            ReorderRate = Clamp01(reorderRate);
            BandwidthKbps = bandwidthKbps < 0 ? 0 : bandwidthKbps;
        }

        /// <summary>One-way base latency in milliseconds applied to every packet.</summary>
        public int BaseLatencyMs { get; }

        /// <summary>Maximum +/- jitter in milliseconds added on top of <see cref="BaseLatencyMs"/>.</summary>
        public int JitterMs { get; }

        /// <summary>Probability in [0,1] that a packet is dropped instead of delivered.</summary>
        public double PacketLossRate { get; }

        /// <summary>
        /// Probability in [0,1] that a packet is pulled earlier than its scheduled time,
        /// causing it to be delivered out of order relative to its neighbours.
        /// </summary>
        public double ReorderRate { get; }

        /// <summary>Bandwidth cap in kilobits per second. 0 means unlimited.</summary>
        public int BandwidthKbps { get; }

        /// <summary>Ideal link: zero latency, no loss. Useful as the baseline in comparisons.</summary>
        public static NetworkConditionProfile Ideal =>
            new NetworkConditionProfile(0, 0, 0d, 0d, 0);

        /// <summary>Local area network: a few milliseconds, no loss.</summary>
        public static NetworkConditionProfile Lan =>
            new NetworkConditionProfile(5, 1, 0d, 0d, 0);

        /// <summary>Typical 4G mobile link: moderate latency with noticeable jitter.</summary>
        public static NetworkConditionProfile Mobile4G =>
            new NetworkConditionProfile(60, 20, 0.005d, 0.01d, 0);

        /// <summary>Cross-region link: high base latency, mild jitter.</summary>
        public static NetworkConditionProfile CrossRegion =>
            new NetworkConditionProfile(150, 25, 0.01d, 0.01d, 0);

        /// <summary>Poor wifi: high jitter and meaningful loss, the stress case for sync models.</summary>
        public static NetworkConditionProfile PoorWifi =>
            new NetworkConditionProfile(80, 60, 0.05d, 0.05d, 0);

        /// <summary>
        /// Low-bandwidth link (128 Kbps) with otherwise ideal conditions.
        /// Exercises bandwidth budget — useful for verifying that sync models
        /// remain correct when the transport throttles throughput.
        /// </summary>
        public static NetworkConditionProfile LimitedBandwidth =>
            new NetworkConditionProfile(0, 0, 0d, 0d, 128);

        private static double Clamp01(double value)
        {
            if (value < 0d) return 0d;
            if (value > 1d) return 1d;
            return value;
        }
    }
}

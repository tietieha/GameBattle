#nullable enable

namespace AbilityKit.Network.Runtime.Conditioning
{
    /// <summary>
    /// Running counters for a <see cref="NetworkConditioningMiddleware"/>, useful for demo overlays
    /// and assertions in tests.
    /// </summary>
    public readonly struct NetworkConditioningStats
    {
        public NetworkConditioningStats(
            long inboundReceived,
            long inboundDelivered,
            long inboundDropped,
            long inboundReordered,
            long outboundReceived,
            long outboundDelivered,
            long outboundDropped,
            long outboundReordered,
            int pendingCount)
        {
            InboundReceived = inboundReceived;
            InboundDelivered = inboundDelivered;
            InboundDropped = inboundDropped;
            InboundReordered = inboundReordered;
            OutboundReceived = outboundReceived;
            OutboundDelivered = outboundDelivered;
            OutboundDropped = outboundDropped;
            OutboundReordered = outboundReordered;
            PendingCount = pendingCount;
        }

        public long InboundReceived { get; }
        public long InboundDelivered { get; }
        public long InboundDropped { get; }
        public long InboundReordered { get; }
        public long OutboundReceived { get; }
        public long OutboundDelivered { get; }
        public long OutboundDropped { get; }
        public long OutboundReordered { get; }

        /// <summary>Packets currently buffered awaiting their scheduled delivery time.</summary>
        public int PendingCount { get; }
    }
}

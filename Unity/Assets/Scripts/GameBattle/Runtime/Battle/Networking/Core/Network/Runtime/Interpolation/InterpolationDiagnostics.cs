#nullable enable

namespace AbilityKit.Network.Runtime
{
    /// <summary>
    /// A point-in-time snapshot of authoritative remote interpolation playback health, surfaced for
    /// diagnostics / smoke output / connection-quality hints. This is intentionally kept off the
    /// mode-agnostic <see cref="INetworkSyncController"/> surface: only controllers that actually
    /// interpolate implement <see cref="IInterpolationDiagnosticsProvider"/>.
    /// </summary>
    public readonly struct InterpolationDiagnostics
    {
        public InterpolationDiagnostics(
            int bufferedRemoteSnapshotCount,
            long remotePlaybackTicks,
            long estimatedServerTicks,
            bool hasPublishedRemoteFrame,
            bool isRemotePlaybackStarved)
        {
            BufferedRemoteSnapshotCount = bufferedRemoteSnapshotCount;
            RemotePlaybackTicks = remotePlaybackTicks;
            EstimatedServerTicks = estimatedServerTicks;
            HasPublishedRemoteFrame = hasPublishedRemoteFrame;
            IsRemotePlaybackStarved = isRemotePlaybackStarved;
        }

        /// <summary>Number of remote authoritative snapshots currently buffered for interpolation.</summary>
        public int BufferedRemoteSnapshotCount { get; }

        /// <summary>The current delayed remote playback time, in timeline ticks.</summary>
        public long RemotePlaybackTicks { get; }

        /// <summary>The current local estimate of authoritative server time, in timeline ticks.</summary>
        public long EstimatedServerTicks { get; }

        /// <summary>Whether at least one remote interpolation frame has been published to presentation.</summary>
        public bool HasPublishedRemoteFrame { get; }

        /// <summary>
        /// Whether the delayed playback time is running past the newest buffered snapshot by more than
        /// the configured extrapolation tolerance (the remote buffer is starved and playback is holding
        /// the last authoritative pose rather than extrapolating further).
        /// </summary>
        public bool IsRemotePlaybackStarved { get; }

        /// <summary>How far the playback clock trails the estimated server time, in timeline ticks.</summary>
        public long PlaybackDelayTicks => EstimatedServerTicks - RemotePlaybackTicks;
    }

    /// <summary>
    /// Implemented by sync controllers that run authoritative remote interpolation and can therefore
    /// report <see cref="InterpolationDiagnostics"/>. Controllers for non-interpolating sync models do
    /// not implement this, so consumers should probe with a type check / <c>TryGet</c> pattern.
    /// </summary>
    public interface IInterpolationDiagnosticsProvider
    {
        InterpolationDiagnostics GetInterpolationDiagnostics();
    }
}

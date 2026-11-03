namespace AbilityKit.Network.Runtime
{
    /// <summary>
    /// Common contract for a client side synchronization controller.
    /// A controller owns the runtime behaviour for one <see cref="NetworkSyncModel"/>,
    /// such as prediction and rollback, authoritative interpolation or batched state sync.
    /// This abstraction is intentionally gameplay-agnostic so every sample can describe
    /// its active synchronization strategy through a single shared seam.
    /// </summary>
    public interface INetworkSyncController
    {
        /// <summary>
        /// The synchronization strategy this controller implements.
        /// </summary>
        NetworkSyncModel SyncModel { get; }

        /// <summary>
        /// Whether the controller has an active synchronized session running.
        /// </summary>
        bool IsStarted { get; }

        /// <summary>
        /// The current frame the controller has advanced its local simulation or playback to.
        /// </summary>
        int CurrentFrame { get; }
    }
}

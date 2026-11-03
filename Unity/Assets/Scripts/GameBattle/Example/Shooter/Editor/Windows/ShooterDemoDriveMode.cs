#nullable enable

namespace AbilityKit.Demo.Shooter.Editor.Windows
{
    /// <summary>
    /// Determines how the <see cref="ShooterDemoWindow"/> drives and observes a session.
    /// </summary>
    public enum ShooterDemoDriveMode
    {
        /// <summary>
        /// The window owns the session and pumps it via <c>EditorApplication.update</c>,
        /// rendering entities through the SceneView gizmo sink. Best for quick acceptance.
        /// </summary>
        EditorDirect = 0,

        /// <summary>
        /// The window attaches to a live host published through
        /// <c>ShooterHostSessionRegistry</c>. It does not pump logic itself; instead it
        /// observes diagnostics and hot-tunes network conditions via the shared registry.
        /// </summary>
        HostAttach = 1,

        /// <summary>
        /// The window starts a Unity Play-mode client that connects to the remote Shooter
        /// state-sync server, restoring an active room before creating a new one.
        /// </summary>
        RemoteStateSync = 2,
    }
}

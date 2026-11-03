#nullable enable

using System;
using AbilityKit.Game.View.Presentation;

namespace AbilityKit.Demo.Shooter.View
{
    internal static class ShooterViewRenderBackendFactory
    {
        public static IShooterViewBinder Create(
            ShooterPresentationFacade presentation,
            IShooterSnapshotViewSink? viewSink,
            ViewRenderBackend backend)
        {
            if (presentation == null) throw new ArgumentNullException(nameof(presentation));

            return backend switch
            {
                ViewRenderBackend.GameObject => new ShooterSnapshotViewBinder(presentation, viewSink),
                ViewRenderBackend.Dots => new ShooterDotsSnapshotViewBinder(presentation, viewSink),
                _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, "Unsupported Shooter view render backend."),
            };
        }
    }
}

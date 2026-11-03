#nullable enable

using System;
using AbilityKit.Game.View.Presentation;

namespace AbilityKit.Demo.Shooter.View
{
    public sealed class ShooterPresentationSessionContext
    {
        private int _retainCount;

        public ShooterPresentationSessionContext(ShooterPresentationFacade presentation)
            : this(presentation, null)
        {
        }

        public ShooterPresentationSessionContext(ShooterPresentationFacade presentation, IShooterSnapshotViewSink? viewSink)
            : this(presentation, viewSink, ViewRenderBackend.GameObject)
        {
        }

        public ShooterPresentationSessionContext(
            ShooterPresentationFacade presentation,
            IShooterSnapshotViewSink? viewSink,
            ViewRenderBackend renderBackend)
        {
            Presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
            RenderBackend = renderBackend;
            Binder = ShooterViewRenderBackendFactory.Create(Presentation, viewSink, renderBackend);
            View = Binder as ShooterSnapshotViewBinder;
        }

        public ShooterPresentationFacade Presentation { get; }

        public ViewRenderBackend RenderBackend { get; }

        public IShooterViewBinder Binder { get; }

        public ShooterSnapshotViewBinder? View { get; }

        public int RetainCount => _retainCount;

        internal void Retain()
        {
            _retainCount++;
        }

        internal bool Release()
        {
            if (_retainCount > 0)
            {
                _retainCount--;
            }

            if (_retainCount == 0)
            {
                DisposeBinder();
                return true;
            }

            return false;
        }

        public static ShooterPresentationSessionContext CreateDefault()
        {
            return CreateDefault(null);
        }

        public static ShooterPresentationSessionContext CreateDefault(IShooterSnapshotViewSink? viewSink)
        {
            return CreateDefault(viewSink, ViewRenderBackend.GameObject);
        }

        public static ShooterPresentationSessionContext CreateDefault(
            IShooterSnapshotViewSink? viewSink,
            ViewRenderBackend renderBackend)
        {
            return new ShooterPresentationSessionContext(new ShooterPresentationFacade(), viewSink, renderBackend);
        }

        public static ShooterPresentationSessionContext CreateFromFacade(ShooterPresentationFacade presentation)
        {
            return CreateFromFacade(presentation, null);
        }

        public static ShooterPresentationSessionContext CreateFromFacade(ShooterPresentationFacade presentation, IShooterSnapshotViewSink? viewSink)
        {
            return CreateFromFacade(presentation, viewSink, ViewRenderBackend.GameObject);
        }

        public static ShooterPresentationSessionContext CreateFromFacade(
            ShooterPresentationFacade presentation,
            IShooterSnapshotViewSink? viewSink,
            ViewRenderBackend renderBackend)
        {
            return new ShooterPresentationSessionContext(presentation, viewSink, renderBackend);
        }

        internal void DisposeBinder()
        {
            if (Binder is IDisposable disposable)
            {
                disposable.Dispose();
                return;
            }

            Binder.Clear();
        }
    }
}

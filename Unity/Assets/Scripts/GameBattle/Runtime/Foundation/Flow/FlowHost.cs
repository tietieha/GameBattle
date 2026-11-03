using System;
using AbilityKit.Ability.Flow.Pooling;

namespace AbilityKit.Ability.Flow
{
    public sealed class FlowHost<TArgs> : IDisposable
    {
        private IFlowRootProvider<TArgs> _provider;
        private FlowSession _session;
        private bool _disposed;

        public event Action Started;
        public event Action<FlowStatus, FlowStatus> StatusChanged;
        public event Action<FlowStatus> Finished;
        public event Action<Exception> UnhandledException;

        public IFlowObserver Observer { get; set; }
        public IFlowTraceRecorder TraceRecorder { get; set; }

        internal FlowHost(bool deferRent)
        {
            _disposed = true;
        }

        public FlowHost(IFlowRootProvider<TArgs> provider)
        {
            ResetForRent(provider, FlowPools.RentSession());
        }

        public FlowStatus Status
        {
            get
            {
                ThrowIfDisposed();
                return _session.Status;
            }
        }

        public FlowContext Context
        {
            get
            {
                ThrowIfDisposed();
                return _session.Context;
            }
        }

        public void Start(TArgs args)
        {
            ThrowIfDisposed();
            var root = _provider.CreateRoot(args);
            _session.Observer = Observer;
            _session.TraceRecorder = TraceRecorder;
            _session.Start(root);
        }

        internal FlowStatus Step(float deltaTime)
        {
            ThrowIfDisposed();
            return _session.Step(deltaTime);
        }

        public void Stop()
        {
            ThrowIfDisposed();
            _session.Stop();
        }

        public void Dispose()
        {
            if (_disposed) return;

            ResetCallbacks();
            FlowPools.ReleaseSession(_session);
            _session = null;
            _provider = null;
            _disposed = true;
        }

        internal void ResetForRent(IFlowRootProvider<TArgs> provider, FlowSession session)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _session.Started += OnSessionStarted;
            _session.StatusChanged += OnSessionStatusChanged;
            _session.Finished += OnSessionFinished;
            _session.UnhandledException += OnSessionUnhandledException;
            _disposed = false;
        }

        internal void ResetForRelease()
        {
            if (!_disposed)
            {
                Dispose();
                return;
            }

            ResetCallbacks();
            _session = null;
            _provider = null;
        }

        private void OnSessionStarted()
        {
            Started?.Invoke();
        }

        private void OnSessionStatusChanged(FlowStatus previous, FlowStatus next)
        {
            StatusChanged?.Invoke(previous, next);
        }

        private void OnSessionFinished(FlowStatus status)
        {
            Finished?.Invoke(status);
        }

        private void OnSessionUnhandledException(Exception ex)
        {
            UnhandledException?.Invoke(ex);
        }

        private void ResetCallbacks()
        {
            if (_session != null)
            {
                _session.Started -= OnSessionStarted;
                _session.StatusChanged -= OnSessionStatusChanged;
                _session.Finished -= OnSessionFinished;
                _session.UnhandledException -= OnSessionUnhandledException;
            }

            Started = null;
            StatusChanged = null;
            Finished = null;
            UnhandledException = null;
            Observer = null;
            TraceRecorder = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FlowHost<TArgs>));
        }
    }
}

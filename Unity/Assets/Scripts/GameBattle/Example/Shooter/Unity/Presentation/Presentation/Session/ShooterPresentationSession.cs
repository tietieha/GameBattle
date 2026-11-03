#nullable enable

using System;

namespace AbilityKit.Demo.Shooter.View.Session
{
    public sealed class ShooterPresentationSession : IDisposable
    {
        private readonly ShooterPresentationSessionOptions _options;
        private readonly IShooterPresentationClient? _client;
        private readonly ShooterPresentationSessionContext _context;
        private bool _disposed;

        public ShooterPresentationSession(ShooterPresentationSessionOptions options, IShooterPresentationClient? client = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _client = client;
            _context = ShooterPresentationSessionContext.CreateDefault();
            
            if (_client != null)
            {
                _client.DataReceived += OnDataReceived;
            }
            
            if (_options.AutoStart && _client != null)
            {
                _client.Connect();
            }
        }

        public ShooterPresentationSessionContext Context => _context;

        public bool IsConnected => _client?.IsConnected ?? false;

        public event Action<uint, ArraySegment<byte>>? DataReceived;

        private void OnDataReceived(uint opCode, ArraySegment<byte> payload)
        {
            DataReceived?.Invoke(opCode, payload);
            _context.Presentation.TryApplyGatewayPush(opCode, payload);
        }

        public void Tick(float deltaTime)
        {
            _client?.Tick(deltaTime);
            _context.Binder.TickInterpolation(deltaTime);
        }

        public void Connect()
        {
            _client?.Connect();
        }

        public void Disconnect()
        {
            _client?.Disconnect();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            if (_client != null)
            {
                _client.DataReceived -= OnDataReceived;
                _client.Dispose();
            }
            
            _context.DisposeBinder();
        }
    }
}
using System;
using AbilityKit.Network.Abstractions;
using AbilityKit.Network.Protocol;
using GameFramework.Network;

namespace AbilityKit.GameFramework.Network
{
    public sealed class GameFrameworkNetworkChannelConnection : IConnection
    {
        private readonly INetworkChannel _channel;
        private bool _disposed;

        public GameFrameworkNetworkChannelConnection(INetworkChannel channel)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _channel.SetDefaultHandler(OnPacketReceived);
            State = _channel.Connected ? ConnectionState.Connected : ConnectionState.Disconnected;
        }

        public ConnectionState State { get; private set; }

        public bool IsConnected => !_disposed && _channel.Connected;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<Exception> Error;
        public event Action<uint, uint, ArraySegment<byte>> PacketReceived;
        public event Action<uint, ArraySegment<byte>> ServerPushReceived;
        public event Action<string, string> Kicked;

        public void Open(string host, int port)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("Host is required.", nameof(host));
            if (port <= 0 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port));

            var addresses = System.Net.Dns.GetHostAddresses(host);
            if (addresses == null || addresses.Length == 0)
            {
                throw new InvalidOperationException($"Host address not found: {host}");
            }

            State = ConnectionState.Connecting;
            _channel.Connect(addresses[0], port);
        }

        public void Close()
        {
            if (_disposed) return;
            _channel.Close();
            MarkDisconnected();
        }

        public void Tick(float deltaTime)
        {
            if (_disposed) return;
            if (State == ConnectionState.Connecting && _channel.Connected)
            {
                MarkConnected();
            }
            else if (State == ConnectionState.Connected && !_channel.Connected)
            {
                MarkDisconnected();
            }
        }

        public void Send(uint opCode, ArraySegment<byte> payload, ushort flags = 0, uint seq = 0)
        {
            ThrowIfDisposed();
            var header = new NetworkPacketHeader((NetworkPacketFlags)flags, opCode, seq, (uint)(payload.Array == null ? 0 : payload.Count));
            _channel.Send(new AbilityKitGatewayPacket(header, payload));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                _channel.SetDefaultHandler(null);
                _channel.Close();
            }
            finally
            {
                State = ConnectionState.Disconnected;
            }
        }

        private void OnPacketReceived(object sender, Packet packet)
        {
            if (_disposed || packet is not AbilityKitGatewayPacket gatewayPacket)
            {
                return;
            }

            var header = gatewayPacket.Header;
            var payload = gatewayPacket.Payload;
            if ((header.Flags & NetworkPacketFlags.ServerPush) != 0)
            {
                ServerPushReceived?.Invoke(header.OpCode, payload);
                return;
            }

            PacketReceived?.Invoke(header.OpCode, header.Seq, payload);
        }

        private void MarkConnected()
        {
            if (State == ConnectionState.Connected) return;
            State = ConnectionState.Connected;
            Connected?.Invoke();
        }

        private void MarkDisconnected()
        {
            if (State == ConnectionState.Disconnected) return;
            State = ConnectionState.Disconnected;
            Disconnected?.Invoke();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(GameFrameworkNetworkChannelConnection));
        }
    }
}

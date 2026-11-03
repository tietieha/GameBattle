using System;

namespace AbilityKit.Network.Abstractions
{
    public readonly struct AbilityKitConnectionDescriptor
    {
        public AbilityKitConnectionDescriptor(AbilityKitConnectionRole role, string host, int port, string transportName = null)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("Connection host is required.", nameof(host));
            if (port <= 0 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port));

            Role = role;
            Host = host;
            Port = port;
            TransportName = string.IsNullOrWhiteSpace(transportName) ? null : transportName;
        }

        public AbilityKitConnectionRole Role { get; }

        public string Host { get; }

        public int Port { get; }

        public string TransportName { get; }
    }
}

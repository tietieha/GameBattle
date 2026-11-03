#nullable enable

using System;

namespace AbilityKit.Demo.Shooter.View
{
    public readonly struct ShooterClientNetworkEndpoint
    {
        public ShooterClientNetworkEndpoint(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("host is required.", nameof(host));
            }

            if (port <= 0 || port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            Host = host;
            Port = port;
        }

        public string Host { get; }

        public int Port { get; }

        public static ShooterClientNetworkEndpoint Localhost(int port)
        {
            return new ShooterClientNetworkEndpoint("127.0.0.1", port);
        }
    }
}

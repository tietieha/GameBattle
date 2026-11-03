using System;

namespace AbilityKit.Coordinator
{
    /// <summary>
    /// 远程连接使用的网络端点。
    /// </summary>
    public readonly struct NetworkEndpoint : IEquatable<NetworkEndpoint>
    {
        public string Host { get; }
        public int Port { get; }

        public NetworkEndpoint(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public bool IsValid => !string.IsNullOrEmpty(Host) && Port > 0;

        public override string ToString() => $"{Host}:{Port}";

        public bool Equals(NetworkEndpoint other) => Host == other.Host && Port == other.Port;
        public override bool Equals(object obj) => obj is NetworkEndpoint other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Host, Port);

        public static bool operator ==(NetworkEndpoint left, NetworkEndpoint right) => left.Equals(right);
        public static bool operator !=(NetworkEndpoint left, NetworkEndpoint right) => !left.Equals(right);

        public static NetworkEndpoint None => default;
    }
}

using AbilityKit.Network.Protocol;
using GameFramework.Network;

namespace AbilityKit.GameFramework.Network
{
    public sealed class AbilityKitGatewayPacketHeader : IPacketHeader
    {
        public const int Size = 4 + NetworkPacketHeader.Size;

        public AbilityKitGatewayPacketHeader(NetworkPacketHeader networkHeader)
        {
            NetworkHeader = networkHeader;
            PacketLength = checked((int)networkHeader.PayloadLength);
        }

        public NetworkPacketHeader NetworkHeader { get; }

        public int PacketLength { get; }
    }
}

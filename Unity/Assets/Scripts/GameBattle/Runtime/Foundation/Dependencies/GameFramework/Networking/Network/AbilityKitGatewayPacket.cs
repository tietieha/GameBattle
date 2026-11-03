using System;
using AbilityKit.Network.Protocol;
using GameFramework.Network;

namespace AbilityKit.GameFramework.Network
{
    public sealed class AbilityKitGatewayPacket : Packet
    {
        public AbilityKitGatewayPacket(NetworkPacketHeader header, ArraySegment<byte> payload)
        {
            Header = header;
            Payload = Copy(payload);
        }

        public override int Id => (int)Header.OpCode;

        public NetworkPacketHeader Header { get; }

        public ArraySegment<byte> Payload { get; }

        public override void Clear()
        {
        }

        private static ArraySegment<byte> Copy(ArraySegment<byte> source)
        {
            if (source.Array == null || source.Count <= 0)
            {
                return default;
            }

            var bytes = new byte[source.Count];
            Buffer.BlockCopy(source.Array, source.Offset, bytes, 0, source.Count);
            return new ArraySegment<byte>(bytes);
        }
    }
}

using System;
using System.Buffers.Binary;
using System.IO;
using AbilityKit.Network.Protocol;
using GameFramework.Network;

namespace AbilityKit.GameFramework.Network
{
    public sealed class AbilityKitGatewayNetworkChannelHelper : INetworkChannelHelper
    {
        public int PacketHeaderLength => AbilityKitGatewayPacketHeader.Size;

        public void Initialize(INetworkChannel networkChannel)
        {
        }

        public void Shutdown()
        {
        }

        public void PrepareForConnecting()
        {
        }

        public bool SendHeartBeat()
        {
            return false;
        }

        public bool Serialize<T>(T packet, Stream destination) where T : Packet
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (packet is not AbilityKitGatewayPacket gatewayPacket)
            {
                return false;
            }

            var payload = gatewayPacket.Payload;
            var frameSize = NetworkFrameCodec.GetFrameSize(payload.Count);
            var buffer = new byte[frameSize];
            var payloadSpan = payload.Array == null || payload.Count == 0
                ? ReadOnlySpan<byte>.Empty
                : new ReadOnlySpan<byte>(payload.Array, payload.Offset, payload.Count);

            NetworkFrameCodec.WriteFrame(buffer, gatewayPacket.Header, payloadSpan);
            destination.Write(buffer, 0, buffer.Length);
            return true;
        }

        public IPacketHeader DeserializePacketHeader(Stream source, out object customErrorData)
        {
            customErrorData = null;
            if (source == null || source.Length < AbilityKitGatewayPacketHeader.Size)
            {
                return null;
            }

            var buffer = ReadAll(source, AbilityKitGatewayPacketHeader.Size);
            var span = new ReadOnlySpan<byte>(buffer);
            var frameLength = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(0, 4));
            if (frameLength < NetworkPacketHeader.Size)
            {
                customErrorData = "AbilityKit gateway frame length is invalid.";
                return null;
            }

            var networkHeader = NetworkPacketHeader.Read(span.Slice(4, NetworkPacketHeader.Size));
            if (networkHeader.PayloadLength != frameLength - NetworkPacketHeader.Size)
            {
                customErrorData = "AbilityKit gateway payload length mismatch.";
                return null;
            }

            return new AbilityKitGatewayPacketHeader(networkHeader);
        }

        public Packet DeserializePacket(IPacketHeader packetHeader, Stream source, out object customErrorData)
        {
            customErrorData = null;
            if (packetHeader is not AbilityKitGatewayPacketHeader gatewayHeader || source == null)
            {
                return null;
            }

            var payloadBytes = ReadAll(source, gatewayHeader.PacketLength);
            var payload = payloadBytes.Length == 0 ? default : new ArraySegment<byte>(payloadBytes);
            return new AbilityKitGatewayPacket(gatewayHeader.NetworkHeader, payload);
        }

        private static byte[] ReadAll(Stream source, int length)
        {
            var buffer = new byte[length];
            source.Position = 0;
            var read = source.Read(buffer, 0, length);
            if (read != length)
            {
                throw new EndOfStreamException($"Expected {length} bytes, got {read} bytes.");
            }

            return buffer;
        }
    }
}

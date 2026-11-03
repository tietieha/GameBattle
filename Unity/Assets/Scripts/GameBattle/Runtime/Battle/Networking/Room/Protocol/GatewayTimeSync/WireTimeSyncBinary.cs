using System;
using MemoryPack;

namespace AbilityKit.Protocol.GatewayTimeSync
{
    /// <summary>
    /// Wire time sync protocol codec backed by MemoryPack.
    /// </summary>
    public static class WireTimeSyncBinary
    {
        public static ArraySegment<byte> Serialize(in WireTimeSyncReq req)
        {
            var bytes = MemoryPackSerializer.Serialize(req);
            return new ArraySegment<byte>(bytes);
        }

        public static ArraySegment<byte> Serialize(in WireTimeSyncRes res)
        {
            var bytes = MemoryPackSerializer.Serialize(res);
            return new ArraySegment<byte>(bytes);
        }

        public static WireTimeSyncReq DeserializeTimeSyncReq(ArraySegment<byte> payload)
        {
            if (payload.Array == null || payload.Count == 0)
                return default;

            var span = new ReadOnlySpan<byte>(payload.Array, payload.Offset, payload.Count);
            return MemoryPackSerializer.Deserialize<WireTimeSyncReq>(span);
        }

        public static WireTimeSyncReq DeserializeTimeSyncReq(ReadOnlySpan<byte> payload)
        {
            if (payload.Length == 0)
                return default;

            return MemoryPackSerializer.Deserialize<WireTimeSyncReq>(payload);
        }

        public static WireTimeSyncRes DeserializeTimeSyncRes(ArraySegment<byte> payload)
        {
            if (payload.Array == null || payload.Count == 0)
                return default;

            var span = new ReadOnlySpan<byte>(payload.Array, payload.Offset, payload.Count);
            return MemoryPackSerializer.Deserialize<WireTimeSyncRes>(span);
        }

        public static WireTimeSyncRes DeserializeTimeSyncRes(ReadOnlySpan<byte> payload)
        {
            if (payload.Length == 0)
                return default;

            return MemoryPackSerializer.Deserialize<WireTimeSyncRes>(payload);
        }
    }
}

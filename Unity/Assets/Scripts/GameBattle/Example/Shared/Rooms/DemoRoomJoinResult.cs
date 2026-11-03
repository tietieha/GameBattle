using AbilityKit.Protocol.Room;

namespace AbilityKit.Demo.Common.Rooms
{
    public readonly struct DemoRoomJoinResult
    {
        public DemoRoomJoinResult(bool success, DemoRoomEndpoint endpoint, ulong numericRoomId, WireRoomSnapshot snapshot, WireWorldStartAnchor worldStartAnchor, string message)
        {
            Success = success;
            Endpoint = endpoint;
            NumericRoomId = numericRoomId;
            Snapshot = snapshot;
            WorldStartAnchor = worldStartAnchor;
            Message = message ?? string.Empty;
        }

        public bool Success { get; }

        public DemoRoomEndpoint Endpoint { get; }

        public ulong NumericRoomId { get; }

        public WireRoomSnapshot Snapshot { get; }

        public WireWorldStartAnchor WorldStartAnchor { get; }

        public string Message { get; }
    }
}

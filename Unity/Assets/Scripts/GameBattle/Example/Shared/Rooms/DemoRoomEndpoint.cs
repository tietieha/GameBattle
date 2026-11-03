#nullable enable

namespace AbilityKit.Demo.Common.Rooms
{
    public readonly struct DemoRoomEndpoint
    {
        public DemoRoomEndpoint(string region, string serverId, string roomId, string roomType)
        {
            Region = region ?? string.Empty;
            ServerId = serverId ?? string.Empty;
            RoomId = roomId ?? string.Empty;
            RoomType = roomType ?? string.Empty;
        }

        public string Region { get; }

        public string ServerId { get; }

        public string RoomId { get; }

        public string RoomType { get; }
    }
}

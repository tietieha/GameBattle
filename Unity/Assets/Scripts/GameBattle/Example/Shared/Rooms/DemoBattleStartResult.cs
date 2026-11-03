using AbilityKit.Protocol.Room;

namespace AbilityKit.Demo.Common.Rooms
{
    public readonly struct DemoBattleStartResult
    {
        public DemoBattleStartResult(bool success, DemoRoomEndpoint endpoint, string battleId, ulong worldId, bool started, string message)
        {
            Success = success;
            Endpoint = endpoint;
            BattleId = battleId ?? string.Empty;
            WorldId = worldId;
            Started = started;
            Message = message ?? string.Empty;
        }

        public bool Success { get; }

        public DemoRoomEndpoint Endpoint { get; }

        public string BattleId { get; }

        public ulong WorldId { get; }

        public bool Started { get; }

        public string Message { get; }

        public static DemoBattleStartResult FromWire(DemoRoomEndpoint endpoint, WireStartRoomBattleRes response)
        {
            return new DemoBattleStartResult(response.Success, endpoint, response.BattleId, response.WorldId, response.Started, response.Message);
        }
    }
}

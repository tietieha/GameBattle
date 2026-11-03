using MemoryPack;

namespace AbilityKit.Protocol.Moba.StateSync
{
    [MemoryPackable]
    public readonly partial struct WireSubscribeStateSyncReq
    {
        [MemoryPackOrder(0)] public string BattleGrainKey { get; init; }
        [MemoryPackOrder(1)] public string RoomId { get; init; }
    }

    [MemoryPackable]
    public readonly partial struct WireSubscribeStateSyncRes
    {
        [MemoryPackOrder(0)] public bool Success { get; init; }
        [MemoryPackOrder(1)] public string Message { get; init; }
    }
}

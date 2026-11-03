using System.Collections.Generic;
using MemoryPack;

namespace AbilityKit.Protocol.Moba.Generated.Gateway
{
    // ========== 登录协议 ==========

    [MemoryPackable]
    public readonly partial struct WireGuestLoginReq
    {
        [MemoryPackOrder(0)] public string GuestId { get; init; }
    }

    [MemoryPackable]
    public readonly partial struct WireGuestLoginRes
    {
        [MemoryPackOrder(0)] public bool Success { get; init; }
        [MemoryPackOrder(1)] public string SessionToken { get; init; }
        [MemoryPackOrder(2)] public string AccountId { get; init; }
        [MemoryPackOrder(3)] public string Message { get; init; }
    }

    // ========== 房间协议 ==========

    [MemoryPackable]
    public readonly partial struct WireCreateRoomReq
    {
        [MemoryPackOrder(0)] public string SessionToken { get; init; }
        [MemoryPackOrder(1)] public string Region { get; init; }
        [MemoryPackOrder(2)] public string ServerId { get; init; }
        [MemoryPackOrder(3)] public string RoomType { get; init; }
        [MemoryPackOrder(4)] public string Title { get; init; }
        [MemoryPackOrder(5)] public bool IsPublic { get; init; }
        [MemoryPackOrder(6)] public int MaxPlayers { get; init; }
        [MemoryPackOrder(7)] public Dictionary<string, string>? Tags { get; init; }
    }

    [MemoryPackable]
    public readonly partial struct WireCreateRoomRes
    {
        [MemoryPackOrder(0)] public bool Success { get; init; }
        [MemoryPackOrder(1)] public string RoomId { get; init; }
        [MemoryPackOrder(2)] public string Message { get; init; }
    }

    [MemoryPackable]
    public readonly partial struct WireJoinRoomReq
    {
        [MemoryPackOrder(0)] public string SessionToken { get; init; }
        [MemoryPackOrder(1)] public string Region { get; init; }
        [MemoryPackOrder(2)] public string ServerId { get; init; }
        [MemoryPackOrder(3)] public string RoomId { get; init; }
    }

    [MemoryPackable]
    public readonly partial struct WireJoinRoomRes
    {
        [MemoryPackOrder(0)] public bool Success { get; init; }
        [MemoryPackOrder(1)] public string RoomId { get; init; }
        [MemoryPackOrder(2)] public ulong NumericRoomId { get; init; }
        [MemoryPackOrder(3)] public string Message { get; init; }
    }
}

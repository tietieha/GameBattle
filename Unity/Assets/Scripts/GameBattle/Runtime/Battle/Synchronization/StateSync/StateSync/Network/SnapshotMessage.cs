using System;
using System.Collections.Generic;
using MemoryPack;

namespace AbilityKit.Ability.StateSync.Network
{
    /// <summary>
    /// 网络传输用的快照消息
    /// 包含快照元数据和实际数据
    /// </summary>
    [MemoryPackable]
    public partial class SnapshotMessage
    {
        /// <summary>
        /// 世界 ID
        /// </summary>
        public ulong WorldId { get; set; }

        /// <summary>
        /// 当前帧号
        /// </summary>
        public int Frame { get; set; }

        /// <summary>
        /// 时间戳（毫秒）
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 是否为完整快照
        /// </summary>
        public bool IsFullSnapshot { get; set; } = true;

        /// <summary>
        /// 压缩标记
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// 快照数据
        /// </summary>
        public byte[] SnapshotData { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 状态哈希
        /// </summary>
        public uint StateHash { get; set; }

        /// <summary>
        /// 创建快照消息
        /// </summary>
        public static SnapshotMessage Create<T>(T snapshot) where T : class, new()
        {
            var data = MemoryPackSerializer.Serialize(snapshot);
            return new SnapshotMessage
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SnapshotData = data,
                IsFullSnapshot = true
            };
        }

        /// <summary>
        /// 解析快照消息
        /// </summary>
        public T? ParseSnapshot<T>() where T : class
        {
            if (SnapshotData == null || SnapshotData.Length == 0)
                return null;

            try
            {
                return MemoryPackSerializer.Deserialize<T>(SnapshotData);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 打包为网络字节
        /// </summary>
        public byte[] Pack()
        {
            return MemoryPackSerializer.Serialize(this);
        }

        /// <summary>
        /// 从网络字节解包
        /// </summary>
        public static SnapshotMessage? Unpack(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            try
            {
                return MemoryPackSerializer.Deserialize<SnapshotMessage>(data);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 快照请求消息
    /// </summary>
    [MemoryPackable]
    public partial class SnapshotRequestMessage
    {
        public ulong WorldId { get; set; }
        public int FromFrame { get; set; }
        public int ToFrame { get; set; }
        public bool RequestFullSnapshot { get; set; }
    }

    /// <summary>
    /// 状态哈希验证消息
    /// </summary>
    [MemoryPackable]
    public partial class StateHashMessage
    {
        public ulong WorldId { get; set; }
        public int Frame { get; set; }
        public uint Hash { get; set; }
        public byte[] StateData { get; set; } = Array.Empty<byte>();
    }
}

using System;
using AbilityKit.Ability.StateSync.Snapshot;

namespace AbilityKit.Ability.StateSync.Network
{
    /// <summary>
    /// 快照打包器接口
    /// 用于将 WorldStateSnapshot 序列化为网络传输格式
    /// </summary>
    public interface ISnapshotPacker
    {
        /// <summary>
        /// 打包快照用于网络传输
        /// </summary>
        byte[] Pack(WorldStateSnapshot snapshot);

        /// <summary>
        /// 解包网络数据为快照
        /// </summary>
        WorldStateSnapshot? Unpack(byte[] data);

        /// <summary>
        /// 压缩快照数据
        /// </summary>
        byte[] Compress(byte[] data);

        /// <summary>
        /// 解压缩数据
        /// </summary>
        byte[] Decompress(byte[] data);
    }

    /// <summary>
    /// 默认的快照打包器，使用 MemoryPack 进行序列化
    /// </summary>
    public sealed class MemoryPackSnapshotPacker : ISnapshotPacker
    {
        private readonly bool _enableCompression;

        public MemoryPackSnapshotPacker(bool enableCompression = false)
        {
            _enableCompression = enableCompression;
        }

        public byte[] Pack(WorldStateSnapshot snapshot)
        {
            var data = WorldStateSnapshot.Serialize(snapshot);
            
            if (_enableCompression && data.Length > 128)
            {
                return Compress(data);
            }
            
            return data;
        }

        public WorldStateSnapshot? Unpack(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            try
            {
                if (_enableCompression && data.Length > 0)
                {
                    // 检查是否压缩 (简单检查第一个字节是否是 gzip 头)
                    var decompressed = TryDecompress(data);
                    if (decompressed != null)
                    {
                        data = decompressed;
                    }
                }

                return WorldStateSnapshot.Deserialize(data);
            }
            catch
            {
                return null;
            }
        }

        public byte[] Compress(byte[] data)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using var output = new System.IO.MemoryStream();
            using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public byte[] Decompress(byte[] data)
        {
            return TryDecompress(data) ?? data;
        }

        private byte[]? TryDecompress(byte[] data)
        {
            if (data == null || data.Length < 2)
                return null;

            // 检查 gzip 魔数 (0x1F, 0x8B)
            if (data[0] == 0x1F && data[1] == 0x8B)
            {
                try
                {
                    using var input = new System.IO.MemoryStream(data);
                    using var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress, leaveOpen: true);
                    using var output = new System.IO.MemoryStream();
                    gzip.CopyTo(output);
                    return output.ToArray();
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }
}

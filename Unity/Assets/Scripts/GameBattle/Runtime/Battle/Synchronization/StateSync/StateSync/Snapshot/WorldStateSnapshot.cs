using System;
using MemoryPack;

namespace AbilityKit.Ability.StateSync.Snapshot
{
    /// <summary>
    /// 通用向量类型（独立于引擎，用于序列化）
    /// 用于网络传输和快照序列化
    ///
    /// 注意：此类型与 AbilityKit.Core.Mathematics.Vec3 是不同的类型
    /// - Vec3: 完整数学库，适合内部计算
    /// - Vec3 (snapshot): MemoryPackable，适合序列化
    /// 业务层在转换时应注意类型匹配
    /// </summary>
    [MemoryPackable]
    public partial struct Vec3
    {
        public float X, Y, Z;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static readonly Vec3 Zero = new Vec3(0f, 0f, 0f);
        public static readonly Vec3 One = new Vec3(1f, 1f, 1f);
        public static readonly Vec3 Up = new Vec3(0f, 1f, 0f);

        public float MagnitudeSquared() => X * X + Y * Y + Z * Z;
        public float Magnitude() => (float)Math.Sqrt(MagnitudeSquared());

        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 operator *(Vec3 a, float s) => new Vec3(a.X * s, a.Y * s, a.Z * s);
        public static Vec3 operator /(Vec3 a, float s) => new Vec3(a.X / s, a.Y / s, a.Z / s);

        public bool ApproximatelyEquals(Vec3 other, float epsilon = 0.0001f)
        {
            return Math.Abs(X - other.X) < epsilon &&
                   Math.Abs(Y - other.Y) < epsilon &&
                   Math.Abs(Z - other.Z) < epsilon;
        }

        /// <summary>
        /// 从 Core.Math.Vec3 转换
        /// </summary>
        public static Vec3 FromCoreVec3(AbilityKit.Core.Mathematics.Vec3 v) => new Vec3(v.X, v.Y, v.Z);

        /// <summary>
        /// 转换为 Core.Math.Vec3
        /// </summary>
        public AbilityKit.Core.Mathematics.Vec3 ToCoreVec3() => new AbilityKit.Core.Mathematics.Vec3(X, Y, Z);
    }

    /// <summary>
    /// 通用四元数类型（独立于引擎，用于序列化）
    /// 用于网络传输和快照序列化
    ///
    /// 注意：此类型与 AbilityKit.Core.Mathematics.Quat 是不同的类型
    /// - Quat: 完整数学库，适合内部计算
    /// - Quat (snapshot): MemoryPackable，适合序列化
    /// 业务层在转换时应注意类型匹配
    /// </summary>
    [MemoryPackable]
    public partial struct Quat
    {
        public float X, Y, Z, W;

        public Quat(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static readonly Quat Identity = new Quat(0f, 0f, 0f, 1f);

        public bool ApproximatelyEquals(Quat other, float epsilon = 0.0001f)
        {
            return Math.Abs(X - other.X) < epsilon &&
                   Math.Abs(Y - other.Y) < epsilon &&
                   Math.Abs(Z - other.Z) < epsilon &&
                   Math.Abs(W - other.W) < epsilon;
        }

        /// <summary>
        /// 从 Core.Math.Quat 转换
        /// </summary>
        public static Quat FromCoreQuat(AbilityKit.Core.Mathematics.Quat q) => new Quat(q.X, q.Y, q.Z, q.W);

        /// <summary>
        /// 转换为 Core.Math.Quat
        /// </summary>
        public AbilityKit.Core.Mathematics.Quat ToCoreQuat() => new AbilityKit.Core.Mathematics.Quat(X, Y, Z, W);
    }

    /// <summary>
    /// 世界状态快照
    /// 通用的快照结构，只包含框架级数据
    /// 业务层可通过扩展 partial class 来添加业务数据
    /// </summary>
    [MemoryPackable]
    public partial class WorldStateSnapshot
    {
        /// <summary>
        /// 世界 ID
        /// </summary>
        public ulong WorldId { get; set; }

        /// <summary>
        /// 快照版本号
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 帧号
        /// </summary>
        public int Frame { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 世界状态标志位
        /// </summary>
        public uint WorldFlags { get; set; }

        /// <summary>
        /// 是否是完整快照（true）或增量快照（false）
        /// </summary>
        public bool IsFullSnapshot { get; set; } = true;

        /// <summary>
        /// 当前版本号常量
        /// </summary>
        public const int CurrentVersion = 1;

        public WorldStateSnapshot()
        {
            Version = CurrentVersion;
        }

        /// <summary>
        /// 序列化快照
        /// </summary>
        public static byte[] Serialize(WorldStateSnapshot snapshot)
        {
            return MemoryPackSerializer.Serialize(snapshot);
        }

        /// <summary>
        /// 反序列化快照
        /// </summary>
        public static WorldStateSnapshot Deserialize(byte[] data)
        {
            return MemoryPackSerializer.Deserialize<WorldStateSnapshot>(data);
        }

        /// <summary>
        /// 从字节数组创建快照
        /// </summary>
        public static WorldStateSnapshot FromBytes(byte[] data)
        {
            return Deserialize(data);
        }

        /// <summary>
        /// 转换为字节数组
        /// </summary>
        public byte[] ToBytes()
        {
            return Serialize(this);
        }

        /// <summary>
        /// 计算状态哈希
        /// </summary>
        public StateHash ComputeHash()
        {
            return StateHashComputer.Compute(this);
        }

        /// <summary>
        /// 克隆快照
        /// </summary>
        public WorldStateSnapshot Clone()
        {
            var bytes = ToBytes();
            var clone = FromBytes(bytes);
            return clone;
        }
    }
}

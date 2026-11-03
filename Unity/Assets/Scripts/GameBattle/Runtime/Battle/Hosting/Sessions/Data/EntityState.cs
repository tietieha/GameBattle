using System;
using MemoryPack;

namespace AbilityKit.Coordinator
{
    /// <summary>
    /// 便于传输的快照实体信封。
    /// </summary>
    public readonly struct SnapshotEntityState
    {
        /// <summary>
        /// 逻辑世界中的实体标识。
        /// </summary>
        public readonly int EntityId;

        /// <summary>
        /// 应用层定义的状态类型。
        /// </summary>
        public readonly int StateType;

        /// <summary>
        /// 序列化后的应用层状态载荷。
        /// </summary>
        public readonly byte[] Payload;

        public SnapshotEntityState(int entityId, int stateType, byte[] payload)
        {
            EntityId = entityId;
            StateType = stateType;
            Payload = payload ?? Array.Empty<byte>();
        }

        public static SnapshotEntityState Create<TPayload>(int entityId, in TPayload payload)
        {
            var stateType = CoordinatorPayloadCodec.TryGetOpCode<TPayload>(out var opCode) ? opCode : 0;
            return Create(entityId, stateType, in payload);
        }

        public static SnapshotEntityState Create<TPayload>(int entityId, int stateType, in TPayload payload)
        {
            return new SnapshotEntityState(entityId, stateType, CoordinatorPayloadCodec.Encode(in payload));
        }

        public bool TryGetPayload<TPayload>(out TPayload payload)
        {
            if (CoordinatorPayloadCodec.TryGetOpCode<TPayload>(out var opCode) && StateType != opCode)
            {
                payload = default;
                return false;
            }

            return CoordinatorPayloadCodec.TryDecode(Payload, out payload);
        }
    }

    /// <summary>
    /// 默认实体状态载荷，作为便捷模型保留。
    /// </summary>
    [MemoryPackable]
    public partial struct EntityState
    {
        /// <summary>
        /// 实体标识。
        /// </summary>
        [MemoryPackOrder(0)] public int EntityId;

        /// <summary>
        /// X 轴位置。
        /// </summary>
        [MemoryPackOrder(1)] public float X;

        /// <summary>
        /// Y 轴位置。
        /// </summary>
        [MemoryPackOrder(2)] public float Y;

        /// <summary>
        /// Z 轴位置。
        /// </summary>
        [MemoryPackOrder(3)] public float Z;

        /// <summary>
        /// 绕 Y 轴旋转。
        /// </summary>
        [MemoryPackOrder(4)] public float Rotation;

        /// <summary>
        /// X 轴速度。
        /// </summary>
        [MemoryPackOrder(5)] public float VelocityX;

        /// <summary>
        /// Z 轴速度。
        /// </summary>
        [MemoryPackOrder(6)] public float VelocityZ;

        /// <summary>
        /// 当前生命值。
        /// </summary>
        [MemoryPackOrder(7)] public float Hp;

        /// <summary>
        /// 最大生命值。
        /// </summary>
        [MemoryPackOrder(8)] public float HpMax;

        /// <summary>
        /// 队伍标识。
        /// </summary>
        [MemoryPackOrder(9)] public int TeamId;

        /// <summary>
        /// 实体是否死亡。
        /// </summary>
        [MemoryPackOrder(10)] public bool IsDead;

        public EntityState(int entityId)
        {
            EntityId = entityId;
            X = Y = Z = 0;
            Rotation = 0;
            VelocityX = VelocityZ = 0;
            Hp = HpMax = 0;
            TeamId = 0;
            IsDead = true;
        }

        public SnapshotEntityState ToSnapshotEntityState()
        {
            return SnapshotEntityState.Create(EntityId, in this);
        }

        public static EntityState Empty(int entityId) => new EntityState(entityId);
    }
}

using System;

namespace AbilityKit.Coordinator
{
    /// <summary>
    /// 帧快照数据。
    /// </summary>
    public readonly struct FrameSnapshotData
    {
        /// <summary>
        /// 帧索引。
        /// </summary>
        public int FrameIndex { get; }

        /// <summary>
        /// 以秒为单位的时间戳。
        /// </summary>
        public double Timestamp { get; }

        /// <summary>
        /// 快照类型。
        /// </summary>
        public SnapshotType Type { get; }

        /// <summary>
        /// 当前帧中由应用定义的实体状态。
        /// </summary>
        public SnapshotEntityState[] Entities { get; }

        /// <summary>
        /// 应用特定格式的自定义数据载荷。
        /// </summary>
        public byte[] CustomPayload { get; }

        public FrameSnapshotData(
            int frameIndex,
            double timestamp,
            SnapshotType type,
            SnapshotEntityState[] entities = null,
            byte[] customPayload = null)
        {
            FrameIndex = frameIndex;
            Timestamp = timestamp;
            Type = type;
            Entities = entities ?? Array.Empty<SnapshotEntityState>();
            CustomPayload = customPayload ?? Array.Empty<byte>();
        }

        public FrameSnapshotData(
            int frameIndex,
            double timestamp,
            SnapshotType type,
            EntityState[] entities,
            byte[] customPayload = null)
            : this(frameIndex, timestamp, type, ToSnapshotEntityStates(entities), customPayload)
        {
        }

        public FrameSnapshotData WithCustomPayload<TPayload>(in TPayload payload)
        {
            return new FrameSnapshotData(
                FrameIndex,
                Timestamp,
                Type,
                Entities,
                CoordinatorPayloadCodec.Encode(in payload));
        }

        public bool TryGetCustomPayload<TPayload>(out TPayload payload)
        {
            return CoordinatorPayloadCodec.TryDecode(CustomPayload, out payload);
        }

        public static FrameSnapshotData Create<TPayload>(
            int frameIndex,
            double timestamp,
            SnapshotType type,
            in TPayload customPayload,
            SnapshotEntityState[] entities = null)
        {
            return new FrameSnapshotData(
                frameIndex,
                timestamp,
                type,
                entities,
                CoordinatorPayloadCodec.Encode(in customPayload));
        }

        public bool TryGetEntities<TPayload>(out TPayload[] payloads)
        {
            payloads = Array.Empty<TPayload>();
            if (Entities == null || Entities.Length == 0)
            {
                return false;
            }

            var result = new TPayload[Entities.Length];
            for (int i = 0; i < Entities.Length; i++)
            {
                if (!Entities[i].TryGetPayload(out result[i]))
                {
                    payloads = Array.Empty<TPayload>();
                    return false;
                }
            }

            payloads = result;
            return true;
        }

        public EntityState[] ToDefaultEntityStates()
        {
            return TryGetEntities<EntityState>(out var states) ? states : Array.Empty<EntityState>();
        }

        private static SnapshotEntityState[] ToSnapshotEntityStates(EntityState[] entities)
        {
            if (entities == null || entities.Length == 0)
            {
                return Array.Empty<SnapshotEntityState>();
            }

            var result = new SnapshotEntityState[entities.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                result[i] = entities[i].ToSnapshotEntityState();
            }

            return result;
        }

        /// <summary>
        /// 创建空快照。
        /// </summary>
        public static FrameSnapshotData Empty => default;
    }

    /// <summary>
    /// 快照类型。
    /// </summary>
    public enum SnapshotType
    {
        /// <summary>
        /// 包含所有实体的完整快照。
        /// </summary>
        Full = 0,

        /// <summary>
        /// 仅包含变化实体的增量快照。
        /// </summary>
        Delta = 1,

        /// <summary>
        /// 关键帧快照。
        /// </summary>
        KeyFrame = 2,
    }
}

using System;

namespace AbilityKit.Ability.StateSync
{
    /// <summary>
    /// 状态变化事件
    /// </summary>
    public struct StateChangeEvent
    {
        /// <summary>实体 ID</summary>
        public int EntityId;

        /// <summary>帧号</summary>
        public int Frame;

        /// <summary>槽位名称</summary>
        public string SlotName;

        /// <summary>旧值</summary>
        public object OldValue;

        /// <summary>新值</summary>
        public object NewValue;

        /// <summary>是否是预测值（而非服务器确认值）</summary>
        public bool IsPredicted;

        /// <summary>值是否发生变化</summary>
        public bool HasChanged => !Equals(OldValue, NewValue);

        public StateChangeEvent(int entityId, int frame, string slotName, object oldValue, object newValue, bool isPredicted)
        {
            EntityId = entityId;
            Frame = frame;
            SlotName = slotName;
            OldValue = oldValue;
            NewValue = newValue;
            IsPredicted = isPredicted;
        }
    }

    /// <summary>
    /// 回滚事件
    /// </summary>
    public struct RollbackEvent
    {
        /// <summary>实体 ID</summary>
        public int EntityId;

        /// <summary>回滚起始帧</summary>
        public int FromFrame;

        /// <summary>回滚目标帧</summary>
        public int ToFrame;

        /// <summary>回滚原因</summary>
        public RollbackReason Reason;

        /// <summary>冲突级别</summary>
        public ConflictLevel Level;

        public RollbackEvent(int entityId, int fromFrame, int toFrame, RollbackReason reason, ConflictLevel level)
        {
            EntityId = entityId;
            FromFrame = fromFrame;
            ToFrame = toFrame;
            Reason = reason;
            Level = level;
        }
    }

    /// <summary>
    /// 回滚原因
    /// </summary>
    public enum RollbackReason
    {
        /// <summary>服务器校正</summary>
        ServerCorrection,

        /// <summary>检测到不同步</summary>
        DesyncDetected,

        /// <summary>手动回滚</summary>
        Manual
    }

    /// <summary>
    /// 快照应用事件
    /// </summary>
    public struct SnapshotAppliedEvent
    {
        /// <summary>实体 ID</summary>
        public int EntityId;

        /// <summary>快照帧号</summary>
        public int Frame;

        /// <summary>是否被回滚覆盖</summary>
        public bool WasRollback;

        public SnapshotAppliedEvent(int entityId, int frame, bool wasRollback)
        {
            EntityId = entityId;
            Frame = frame;
            WasRollback = wasRollback;
        }
    }
}

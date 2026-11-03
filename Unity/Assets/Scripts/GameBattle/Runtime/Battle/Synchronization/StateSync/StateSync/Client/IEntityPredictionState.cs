using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.StateSync.Client
{
    using SlotValue = AbilityKit.Ability.StateSync.SlotValue;

    /// <summary>
    /// 实体预测状态接口
    /// 封装单个实体的预测逻辑
    /// </summary>
    public interface IEntityPredictionState
    {
        /// <summary>
        /// 实体 ID
        /// </summary>
        int EntityId { get; }

        /// <summary>
        /// 是否是本地玩家控制的实体
        /// </summary>
        bool IsLocalPlayer { get; }

        /// <summary>
        /// 当前状态槽位
        /// </summary>
        AbilityKit.Ability.StateSync.Prediction.StateSlots CurrentSlots { get; }

        /// <summary>
        /// 是否处于预测状态
        /// </summary>
        bool IsPredicted { get; }

        /// <summary>
        /// 当前帧号
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 槽位变化事件
        /// </summary>
        event Action<string, object, object> OnSlotChanged;

        /// <summary>
        /// 回滚事件
        /// </summary>
        event Action<int, int> OnRollback;

        /// <summary>
        /// 注册预测处理器
        /// </summary>
        void RegisterHandler(IClientPredictionHandler handler);

        /// <summary>
        /// 执行输入预测
        /// </summary>
        void Predict(IInputCommand input, int frame);

        /// <summary>
        /// 应用服务器状态
        /// </summary>
        bool ApplyServerState(int serverFrame, ServerEntitySnapshot snapshot);

        /// <summary>
        /// 回滚到指定帧
        /// </summary>
        void RollbackTo(int frame);

        /// <summary>
        /// 快照当前状态
        /// </summary>
        void CaptureSnapshot(int frame);

        /// <summary>
        /// 获取指定帧的快照
        /// </summary>
        AbilityKit.Ability.StateSync.Prediction.StateSlots GetSnapshot(int frame);

        /// <summary>
        /// 推进到下一帧
        /// </summary>
        void AdvanceFrame();

        /// <summary>
        /// 获取待处理的状态变化
        /// </summary>
        IReadOnlyList<StateChangeEvent> GetPendingStateChanges();

        /// <summary>
        /// 清除待处理的状态变化
        /// </summary>
        void ClearPendingStateChanges();
    }
}

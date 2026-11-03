using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.StateSync.Client
{
    /// <summary>
    /// 客户端预测模块接口
    /// 管理所有实体的预测状态，协调输入、预测、回滚
    ///
    /// 【适用场景】
    /// 此模块适合以下类型的游戏：
    /// - MOBA / ARPG：实体数量适中（几十到几百个），每个实体有独立的预测逻辑
    /// - 每个实体需要独立的 Handler 来处理预测（如移动预测、冷却预测）
    /// - 需要精确追踪每个实体的预测状态和快照
    ///
    /// 【不适用场景】
    /// - FPS / 动作游戏：通常使用 ECS 风格，按系统（移动、技能）划分 Handler
    ///   这种情况下建议使用 PredictionCoordinator
    ///
    /// 【使用方式】
    /// 1. 实现 IPredictableEntity 接口定义实体
    /// 2. 为每个需要预测的方面实现 IClientPredictionHandler
    /// 3. 注册实体后，调用 SubmitInput 提交输入
    /// 4. 每帧调用 Tick 推进预测
    /// 5. 调用 ApplyServerSnapshot 应用服务器校正
    /// </summary>
    public interface IClientPredictionModule : IDisposable
    {
        /// <summary>
        /// 本地玩家 ID
        /// </summary>
        int LocalPlayerId { get; }

        /// <summary>
        /// 当前预测帧号
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 服务器确认帧号
        /// </summary>
        int ConfirmedFrame { get; }

        /// <summary>
        /// 是否有待确认的预测
        /// </summary>
        bool HasUnconfirmedPrediction { get; }

        /// <summary>
        /// 初始化模块
        /// </summary>
        void Initialize(IClientPredictionConfig config);

        /// <summary>
        /// 注册可预测实体
        /// </summary>
        void RegisterEntity(IPredictableEntity entity);

        /// <summary>
        /// 注销实体
        /// </summary>
        void UnregisterEntity(int entityId);

        /// <summary>
        /// 提交本地输入
        /// </summary>
        void SubmitInput(IInputCommand input);

        /// <summary>
        /// 每帧调用，推进预测
        /// </summary>
        void Tick(int frame);

        /// <summary>
        /// 应用服务器快照
        /// </summary>
        void ApplyServerSnapshot(int serverFrame, ServerEntitySnapshot[] snapshots);

        /// <summary>
        /// 获取实体的预测状态
        /// </summary>
        AbilityKit.Ability.StateSync.Prediction.StateSlots GetPredictedSlots(int entityId);

        /// <summary>
        /// 获取实体的 EntityPredictionState
        /// </summary>
        IEntityPredictionState GetEntityState(int entityId);

        /// <summary>
        /// 状态变化事件
        /// </summary>
        event Action<StateChangeEvent> OnStateChanged;

        /// <summary>
        /// 回滚事件
        /// </summary>
        event Action<RollbackEvent> OnRollback;

        /// <summary>
        /// 快照应用事件
        /// </summary>
        event Action<int, int> OnSnapshotApplied;
    }

    /// <summary>
    /// 预测配置
    /// </summary>
    public interface IClientPredictionConfig
    {
        /// <summary>
        /// 本地玩家 ID
        /// </summary>
        int LocalPlayerId { get; }

        /// <summary>
        /// 最大预测帧数
        /// </summary>
        int MaxPredictionFrames { get; }

        /// <summary>
        /// 快照阈值
        /// </summary>
        float SnapThreshold { get; }

        /// <summary>
        /// 是否启用回滚
        /// </summary>
        bool EnableRollback { get; }

        /// <summary>
        /// 最大输入缓冲区大小
        /// </summary>
        int MaxInputBufferSize { get; }
    }

    /// <summary>
    /// 服务器实体快照
    /// </summary>
    public sealed class ServerEntitySnapshot
    {
        /// <summary>
        /// 实体 ID
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// 帧号
        /// </summary>
        public int Frame { get; set; }

        /// <summary>
        /// 原始字节数据
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 状态哈希
        /// </summary>
        public long StateHash { get; set; }
    }

    /// <summary>
    /// 客户端预测配置默认实现
    /// </summary>
    public sealed class ClientPredictionConfig : IClientPredictionConfig
    {
        public int LocalPlayerId { get; set; }
        public int MaxPredictionFrames { get; set; } = 30;
        public float SnapThreshold { get; set; } = 0.1f;
        public bool EnableRollback { get; set; } = true;
        public int MaxInputBufferSize { get; set; } = 128;

        public static ClientPredictionConfig Default => new ClientPredictionConfig
        {
            LocalPlayerId = 0,
            MaxPredictionFrames = 30,
            SnapThreshold = 0.1f,
            EnableRollback = true,
            MaxInputBufferSize = 128
        };
    }
}

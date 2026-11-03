using System.Collections.Generic;

namespace AbilityKit.Ability.StateSync.Client
{
    using StateSlots = AbilityKit.Ability.StateSync.Prediction.StateSlots;

    /// <summary>
    /// 可预测实体接口
    /// 由业务层实现，定义实体需要预测的状态
    /// </summary>
    public interface IPredictableEntity
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
        /// 实体类型名称（用于调试）
        /// </summary>
        string EntityType { get; }

        /// <summary>
        /// 获取预测处理器
        /// </summary>
        IReadOnlyList<IClientPredictionHandler> GetPredictionHandlers();

        /// <summary>
        /// 获取当前状态槽位
        /// </summary>
        StateSlots GetStateSlots();

        /// <summary>
        /// 从预测状态恢复
        /// </summary>
        void RestoreFromPredictedState(StateSlots slots);

        /// <summary>
        /// 应用服务器权威状态
        /// </summary>
        void ApplyServerState(ServerEntitySnapshot snapshot);
    }

    /// <summary>
    /// 客户端预测处理器接口
    /// </summary>
    public interface IClientPredictionHandler
    {
        /// <summary>
        /// 处理器名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 预测策略
        /// </summary>
        PredictionStrategy Strategy { get; }

        /// <summary>
        /// 需要的槽位模式（如 "position", "cooldown.*"）
        /// </summary>
        IReadOnlyList<string> RequiredSlots { get; }

        /// <summary>
        /// 执行本地预测
        /// </summary>
        void PredictLocal(IInputCommand input, StateSlots slots, int frame);

        /// <summary>
        /// 校验预测结果
        /// </summary>
        PredictionResult Validate(StateSlots predicted, ServerEntitySnapshot server);

        /// <summary>
        /// 应用服务器状态到当前预测状态
        /// </summary>
        void ApplyServerState(StateSlots server, StateSlots current);
    }

    /// <summary>
    /// 预测处理器工厂接口
    /// 用于根据实体类型创建对应的处理器
    /// </summary>
    public interface IPredictionHandlerFactory
    {
        /// <summary>
        /// 是否支持此实体类型
        /// </summary>
        bool CanHandle(IPredictableEntity entity);

        /// <summary>
        /// 创建处理器
        /// </summary>
        IClientPredictionHandler CreateHandler(IPredictableEntity entity);
    }
}

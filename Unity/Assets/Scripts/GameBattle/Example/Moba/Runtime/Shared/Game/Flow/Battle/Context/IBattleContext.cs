using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 战斗上下文接口
    /// 定义跨平台的战斗上下文契约
    /// 整合所有战斗相关的核心组件
    /// </summary>
    public interface IBattleContext
    {
        // ============ 基础属性 ============

        /// <summary>
        /// 当前帧
        /// </summary>
        int LastFrame { get; }

        /// <summary>
        /// 逻辑时间（秒）
        /// </summary>
        double LogicTimeSeconds { get; }

        /// <summary>
        /// 帧率
        /// </summary>
        float FrameRate { get; }

        /// <summary>
        /// 本地玩家网络 ID
        /// </summary>
        int LocalActorId { get; }

        /// <summary>
        /// 战斗启动计划
        /// </summary>
        BattleStartPlan Plan { get; }

        // ============ 实体管理 ============

        /// <summary>
        /// 实体世界
        /// </summary>
        IShareEntityWorld EntityWorld { get; }

        /// <summary>
        /// 实体查找器
        /// </summary>
        IShareEntityLookup EntityLookup { get; }

        /// <summary>
        /// 实体工厂
        /// </summary>
        IShareEntityFactory EntityFactory { get; }

        /// <summary>
        /// 实体查询器
        /// </summary>
        IBattleEntityQuery EntityQuery { get; }

        // ============ 快照系统 ============

        /// <summary>
        /// 快照分发器
        /// </summary>
        IFrameSnapshotDispatcher Snapshots { get; }

        // ============ 输入系统 ============

        /// <summary>
        /// 本地输入队列
        /// </summary>
        IInputQueue LocalInputQueue { get; }

        // ============ 会话管理 ============

        /// <summary>
        /// 会话编排器
        /// </summary>
        ISessionOrchestrator Session { get; }
    }

    /// <summary>
    /// 输入队列接口
    /// </summary>
    public interface IInputQueue
    {
        /// <summary>
        /// 入队输入
        /// </summary>
        void Enqueue(int frame, PlayerInputData input);

        /// <summary>
        /// 获取指定帧的输入
        /// </summary>
        bool TryDequeue(int frame, out PlayerInputData input);

        /// <summary>
        /// 清空队列
        /// </summary>
        void Clear();

        /// <summary>
        /// 队列中的输入数量
        /// </summary>
        int Count { get; }
    }
}

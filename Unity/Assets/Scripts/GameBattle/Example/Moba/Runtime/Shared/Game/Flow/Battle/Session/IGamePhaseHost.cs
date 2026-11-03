using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 游戏阶段上下文接口
    /// 定义游戏阶段运行时的共享状态
    /// </summary>
    public interface IGamePhaseContext
    {
        /// <summary>
        /// 获取根上下文对象
        /// </summary>
        object Root { get; }

        /// <summary>
        /// 获取游戏流程域
        /// </summary>
        object Entry { get; }
    }

    /// <summary>
    /// 游戏阶段宿主接口
    /// 定义游戏阶段的生命周期和方法
    /// </summary>
    public interface IGamePhaseHost
    {
        /// <summary>
        /// 附加到游戏阶段
        /// </summary>
        /// <param name="ctx">阶段上下文</param>
        void OnAttach(in GamePhaseContext ctx);

        /// <summary>
        /// 从游戏阶段分离
        /// </summary>
        /// <param name="ctx">阶段上下文</param>
        void OnDetach(in GamePhaseContext ctx);

        /// <summary>
        /// 每帧更新
        /// </summary>
        /// <param name="ctx">阶段上下文</param>
        /// <param name="deltaTime">帧时间</param>
        void Tick(in GamePhaseContext ctx, float deltaTime);
    }

    /// <summary>
    /// 战斗阶段上下文接口
    /// 扩展游戏阶段上下文，添加战斗相关状态
    /// </summary>
    public interface IBattlePhaseContext : IGamePhaseContext
    {
        /// <summary>
        /// 获取战斗逻辑会话
        /// </summary>
        IBattleLogicSession Session { get; set; }

        /// <summary>
        /// 获取最后处理的帧索引
        /// </summary>
        int LastFrame { get; set; }

        /// <summary>
        /// 获取逻辑时间（秒）
        /// </summary>
        double LogicTimeSeconds { get; set; }

        /// <summary>
        /// 获取脏实体列表
        /// </summary>
        System.Collections.Generic.IReadOnlyList<int> DirtyEntities { get; }
    }

    /// <summary>
    /// 游戏阶段上下文结构体
    /// 传递阶段上下文数据
    /// </summary>
    public readonly struct GamePhaseContext
    {
        /// <summary>
        /// 根上下文对象
        /// </summary>
        public object Root { get; }

        /// <summary>
        /// 游戏流程入口
        /// </summary>
        public object Entry { get; }

        /// <summary>
        /// 用户数据
        /// </summary>
        public object UserData { get; }

        public GamePhaseContext(object root, object entry, object userData = null)
        {
            Root = root;
            Entry = entry;
            UserData = userData;
        }
    }
}

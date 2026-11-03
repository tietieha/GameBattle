using System;
using AbilityKit.Ability.Host;
using AbilityKit.Ability.World.Abstractions;

namespace AbilityKit.Coordinator
{
    /// <summary>
    /// Session Coordinator 主机接口。
    ///
    /// 设计：
    /// - 提供平台相关实现。
    /// - 创建 WorldHost、注册服务并加载配置。
    /// - 由应用层或游戏集成层实现。
    /// </summary>
    public interface ISessionCoordinatorConfigPolicy
    {
        void ConfigureSession(ref SessionConfig config);
    }

    public interface ISessionCoordinatorHost
    {
        /// <summary>
        /// 创建世界主机实例。
        /// </summary>
        IWorldHost CreateWorldHost(SessionConfig config);

        /// <summary>
        /// 在 coordinator 创建世界前配置世界创建选项。
        /// </summary>
        void ConfigureWorldCreateOptions(in SessionConfig config, WorldCreateOptions options);

        /// <summary>
        /// 向世界注册服务。
        /// </summary>
        void RegisterServices(IWorld world, SessionConfig config);

        /// <summary>
        /// 加载会话配置。
        /// </summary>
        void LoadConfig(IWorld world, SessionConfig config);

        /// <summary>
        /// 创建玩家出生数据。
        /// </summary>
        PlayerSpawnData[] CreatePlayerSpawnData(SessionConfig config);
    }
}

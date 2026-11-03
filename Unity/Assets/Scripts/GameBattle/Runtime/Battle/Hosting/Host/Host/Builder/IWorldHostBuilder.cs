using System;
using System.Collections.Generic;
using AbilityKit.Ability.Host.Builder.Components;
using AbilityKit.Ability.Host.Framework;
using AbilityKit.Ability.Host.WorldBlueprints;
using AbilityKit.Ability.World.Abstractions;

namespace AbilityKit.Ability.Host.Builder
{
    /// <summary>
    /// WorldHost 构建器接口
    /// 用于灵活组合 HostRuntime 的各个组件
    /// </summary>
    public interface IWorldHostBuilder
    {
        // ============== Core Components ==============

        /// <summary>
        /// 设置世界工厂
        /// </summary>
        IWorldHostBuilder SetWorldFactory(IWorldFactory factory);

        /// <summary>
        /// 设置连接管理器
        /// </summary>
        IWorldHostBuilder SetConnectionManager(IConnectionManager connectionManager);

        /// <summary>
        /// 设置时间驱动
        /// </summary>
        IWorldHostBuilder SetTimeDriver(ITimeDriver timeDriver);

        // ============== Optional Components ==============

        /// <summary>
        /// 设置输入驱动（帧同步模式）
        /// </summary>
        IWorldHostBuilder SetInputDriver(IInputDriver inputDriver);

        /// <summary>
        /// 设置快照提供器
        /// </summary>
        IWorldHostBuilder SetSnapshotProvider(ISnapshotProvider snapshotProvider);

        /// <summary>
        /// 设置世界蓝图注册表
        /// </summary>
        IWorldHostBuilder SetBlueprintRegistry(IWorldBlueprintRegistry blueprintRegistry);

        // ============== Modules ==============

        /// <summary>
        /// 添加运行时模块
        /// </summary>
        IWorldHostBuilder AddModule(IHostRuntimeModule module);

        /// <summary>
        /// 添加多个运行时模块
        /// </summary>
        IWorldHostBuilder AddModules(IEnumerable<IHostRuntimeModule> modules);

        // ============== Build ==============

        /// <summary>
        /// 构建 HostRuntime 实例
        /// </summary>
        IWorldHost Build();

        /// <summary>
        /// 构建并返回 Runtime 和 Options（用于后续模块安装）
        /// </summary>
        (IWorldHost Host, HostRuntimeOptions Options) BuildWithOptions();
    }
}

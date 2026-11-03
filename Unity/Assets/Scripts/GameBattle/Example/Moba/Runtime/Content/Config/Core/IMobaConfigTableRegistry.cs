using System;
using AbilityKit.Ability.Config;

namespace AbilityKit.Demo.Moba.Config.Core
{
    /// <summary>
    /// MOBA 配置表注册器接口，扩展通用 IConfigTableRegistry。
    /// </summary>
    public interface IMobaConfigTableRegistry : IConfigTableRegistry
    {
        /// <summary>
        /// 获取所有配置表条目，供 MOBA 运行时加载管线使用。
        /// </summary>
        BattleDemo.MobaRuntimeConfigTableRegistry.Entry[] MobaTables { get; }
    }
}

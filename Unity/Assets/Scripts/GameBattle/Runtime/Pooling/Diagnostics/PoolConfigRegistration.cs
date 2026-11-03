using System;

namespace AbilityKit.Core.Pooling
{
    /// <summary>
    /// 对象池配置提供者注册句柄，用于测试、热更新模块或编辑器工具在作用域结束时安全注销临时配置。
    /// </summary>
    public readonly struct PoolConfigRegistration : IDisposable
    {
        private readonly IPoolConfigProvider _provider;

        /// <summary>
        /// 创建配置提供者注册句柄。
        /// </summary>
        /// <param name="provider">已注册的配置提供者。</param>
        /// <param name="info">注册时生成的诊断信息。</param>
        public PoolConfigRegistration(IPoolConfigProvider provider, PoolConfigProviderInfo info)
        {
            _provider = provider;
            Info = info;
        }

        /// <summary>
        /// 获取注册时生成的配置提供者诊断信息。
        /// </summary>
        public PoolConfigProviderInfo Info { get; }

        /// <summary>
        /// 获取该句柄是否绑定了有效的配置提供者。
        /// </summary>
        public bool IsValid => _provider != null;

        /// <summary>
        /// 注销该句柄对应的配置提供者。
        /// </summary>
        /// <returns>如果配置提供者仍在配置中心并被成功移除，则返回 <c>true</c>。</returns>
        public bool Unregister()
        {
            return _provider != null && PoolConfigCenter.UnregisterProvider(_provider);
        }

        /// <summary>
        /// 注销该句柄对应的配置提供者。
        /// </summary>
        public void Dispose()
        {
            Unregister();
        }
    }
}

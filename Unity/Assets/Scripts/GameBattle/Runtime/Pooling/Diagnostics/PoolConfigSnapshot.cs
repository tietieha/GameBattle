namespace AbilityKit.Core.Pooling
{
    /// <summary>
    /// 对象池配置命中快照，记录最终生效的配置及其提供者来源。
    /// </summary>
    public readonly struct PoolConfigSnapshot
    {
        /// <summary>
        /// 配置查询请求。
        /// </summary>
        public readonly PoolConfigRequest Request;

        /// <summary>
        /// 最终生效的对象池配置。
        /// </summary>
        public readonly PoolItemConfig Config;

        /// <summary>
        /// 提供最终生效配置的提供者诊断信息。
        /// </summary>
        public readonly PoolConfigProviderInfo Provider;

        /// <summary>
        /// 创建对象池配置命中快照。
        /// </summary>
        /// <param name="request">配置查询请求。</param>
        /// <param name="config">最终生效的对象池配置。</param>
        /// <param name="provider">提供最终生效配置的提供者诊断信息。</param>
        public PoolConfigSnapshot(PoolConfigRequest request, PoolItemConfig config, PoolConfigProviderInfo provider)
        {
            Request = request;
            Config = config;
            Provider = provider;
        }
    }
}

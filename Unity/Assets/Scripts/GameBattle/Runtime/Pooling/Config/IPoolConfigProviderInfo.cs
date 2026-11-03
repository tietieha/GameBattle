namespace AbilityKit.Core.Pooling
{
    /// <summary>
    /// 可提供配置来源信息的对象池配置提供者。
    /// </summary>
    public interface IPoolConfigProviderInfo
    {
        /// <summary>
        /// 获取配置提供者的诊断信息。
        /// </summary>
        PoolConfigProviderInfo Info { get; }
    }
}

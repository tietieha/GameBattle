namespace AbilityKit.Core.Pooling
{
    /// <summary>
    /// 包级对象池配置提供者，每个包可以向中心注册一个或多个提供者。
    /// </summary>
    public interface IPoolConfigProvider
    {
        bool TryGetConfig(PoolConfigRequest request, out PoolItemConfig config);
    }
}

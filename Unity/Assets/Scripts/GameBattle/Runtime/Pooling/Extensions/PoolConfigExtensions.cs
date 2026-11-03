using System;

namespace AbilityKit.Core.Pooling
{
    /// <summary>
    /// 用于强类型对象池配置注册与查询的便捷扩展。
    /// </summary>
    public static class PoolConfigExtensions
    {
        /// <summary>
        /// 为指定作用域创建强类型对象池配置请求。
        /// </summary>
        public static PoolConfigRequest For<T>(this string scopeName, PoolKey key = default)
        {
            return new PoolConfigRequest(scopeName, typeof(T), key);
        }

        /// <summary>
        /// 为运行时类型和作用域创建对象池配置请求。
        /// </summary>
        public static PoolConfigRequest For(this string scopeName, Type elementType, PoolKey key = default)
        {
            return new PoolConfigRequest(scopeName, elementType, key);
        }

        /// <summary>
        /// 通过强类型参数查询配置提供者，避免调用点手动构造 <see cref="PoolConfigRequest" />。
        /// </summary>
        public static bool TryGetConfig<T>(this IPoolConfigProvider provider, string scopeName, PoolKey key, out PoolItemConfig config)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            return provider.TryGetConfig(new PoolConfigRequest(scopeName, typeof(T), key), out config);
        }

        /// <summary>
        /// 使用已构造好的配置在构建器中注册强类型对象池项。
        /// </summary>
        public static PoolConfigBuilder Pool<T>(this PoolConfigBuilder builder, PoolItemConfig config, PoolKey key = default) where T : class
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.Add<T>(config, key);
        }

        /// <summary>
        /// 使用常用容量参数在构建器中注册强类型对象池项。
        /// </summary>
        public static PoolConfigBuilder Pool<T>(
            this PoolConfigBuilder builder,
            int defaultCapacity,
            int maxSize,
            int prewarmCount = -1,
            bool collectionCheck = true,
            PoolTrimPolicy trimPolicy = default,
            bool neverTrim = false,
            PoolKey key = default) where T : class
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.Add<T>(defaultCapacity, maxSize, prewarmCount, collectionCheck, trimPolicy, neverTrim, key);
        }

        /// <summary>
        /// 使用常用容量参数为命名作用域注册强类型对象池项。
        /// </summary>
        public static PoolConfigBuilder Pool<T>(
            this PoolConfigBuilder builder,
            string scopeName,
            int defaultCapacity,
            int maxSize,
            int prewarmCount = -1,
            bool collectionCheck = true,
            PoolTrimPolicy trimPolicy = default,
            bool neverTrim = false,
            PoolKey key = default) where T : class
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.Add<T>(scopeName, key, defaultCapacity, maxSize, prewarmCount, collectionCheck, trimPolicy, neverTrim);
        }

        /// <summary>
        /// 将强类型对象池项注册为禁用状态。
        /// </summary>
        public static PoolConfigBuilder DisablePool<T>(this PoolConfigBuilder builder, PoolKey key = default) where T : class
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.Disable<T>(key);
        }
    }
}

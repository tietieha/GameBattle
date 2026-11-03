namespace AbilityKit.Core.Pooling
{
    /// <summary>
    /// 单个对象池的声明式设置，配置提供者可以从包级配置中返回该结构。
    /// </summary>
    public readonly struct PoolItemConfig
    {
        private readonly bool _specified;

        public readonly bool Enabled;
        public readonly int DefaultCapacity;
        public readonly int MaxSize;
        public readonly int PrewarmCount;
        public readonly bool CollectionCheck;
        public readonly PoolTrimPolicy TrimPolicy;
        public readonly bool NeverTrim;

        public PoolItemConfig(
            bool enabled = true,
            int defaultCapacity = 0,
            int maxSize = 1024,
            int prewarmCount = -1,
            bool collectionCheck = true,
            PoolTrimPolicy trimPolicy = default,
            bool neverTrim = false)
        {
            _specified = true;
            Enabled = enabled;
            DefaultCapacity = defaultCapacity;
            MaxSize = maxSize;
            PrewarmCount = prewarmCount < 0 ? defaultCapacity : prewarmCount;
            CollectionCheck = collectionCheck;
            TrimPolicy = neverTrim ? PoolTrimPolicy.KeepAll : trimPolicy;
            NeverTrim = neverTrim;
        }

        public bool IsSpecified => _specified;

        public static PoolItemConfig Unspecified => default;

        public static PoolItemConfig Disabled => new PoolItemConfig(enabled: false);

        public static PoolItemConfig Default(
            int defaultCapacity = 0,
            int maxSize = 1024,
            int prewarmCount = -1,
            bool collectionCheck = true,
            PoolTrimPolicy trimPolicy = default,
            bool neverTrim = false)
        {
            return new PoolItemConfig(true, defaultCapacity, maxSize, prewarmCount, collectionCheck, trimPolicy, neverTrim);
        }
    }
}

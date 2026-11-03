using System.Collections.Generic;

namespace AbilityKit.Core.Pooling
{
    /// <summary>
    /// 单个对象池配置命中项，记录匹配请求、配置内容、提供者信息以及是否为最终生效项。
    /// </summary>
    public readonly struct PoolConfigMatch
    {
        /// <summary>
        /// 创建对象池配置命中项。
        /// </summary>
        /// <param name="request">配置查询请求。</param>
        /// <param name="config">命中的配置内容。</param>
        /// <param name="provider">命中配置的提供者诊断信息。</param>
        /// <param name="isWinner">该命中项是否为最终生效项。</param>
        public PoolConfigMatch(PoolConfigRequest request, PoolItemConfig config, PoolConfigProviderInfo provider, bool isWinner)
        {
            Request = request;
            Config = config;
            Provider = provider;
            IsWinner = isWinner;
        }

        /// <summary>
        /// 获取配置查询请求。
        /// </summary>
        public PoolConfigRequest Request { get; }

        /// <summary>
        /// 获取命中的配置内容。
        /// </summary>
        public PoolItemConfig Config { get; }

        /// <summary>
        /// 获取命中配置的提供者诊断信息。
        /// </summary>
        public PoolConfigProviderInfo Provider { get; }

        /// <summary>
        /// 获取该命中项是否为最终生效项。
        /// </summary>
        public bool IsWinner { get; }
    }

    /// <summary>
    /// 对象池配置冲突诊断报告，列出所有匹配候选以及最终生效项。
    /// </summary>
    public readonly struct PoolConfigReport
    {
        /// <summary>
        /// 创建对象池配置冲突诊断报告。
        /// </summary>
        /// <param name="request">配置查询请求。</param>
        /// <param name="matches">所有匹配候选。</param>
        /// <param name="winner">最终生效项。</param>
        public PoolConfigReport(PoolConfigRequest request, IReadOnlyList<PoolConfigMatch> matches, PoolConfigMatch winner)
        {
            Request = request;
            Matches = matches ?? new List<PoolConfigMatch>(0);
            Winner = winner;
        }

        /// <summary>
        /// 获取配置查询请求。
        /// </summary>
        public PoolConfigRequest Request { get; }

        /// <summary>
        /// 获取所有匹配候选，列表中包含最终生效项。
        /// </summary>
        public IReadOnlyList<PoolConfigMatch> Matches { get; }

        /// <summary>
        /// 获取最终生效项。
        /// </summary>
        public PoolConfigMatch Winner { get; }

        /// <summary>
        /// 获取是否存在最终生效项。
        /// </summary>
        public bool HasWinner => Matches != null && Matches.Count > 0;
    }
}

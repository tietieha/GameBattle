using System.Collections.Generic;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 能力管线配置接口。
    /// </summary>
    public interface IAbilityPipelineConfig
    {
        /// <summary>
        /// 配置 ID。
        /// </summary>
        int ConfigId { get; }

        /// <summary>
        /// 配置名称。
        /// </summary>
        string ConfigName { get; }

        /// <summary>
        /// 阶段配置列表。
        /// </summary>
        IReadOnlyList<IAbilityPhaseConfig> PhaseConfigs { get; }

        /// <summary>
        /// 是否允许中断。
        /// </summary>
        bool AllowInterrupt { get; }

        /// <summary>
        /// 是否允许暂停。
        /// </summary>
        bool AllowPause { get; }
    }

    /// <summary>
    /// 能力管线阶段配置接口。
    /// </summary>
    public interface IAbilityPhaseConfig
    {
        /// <summary>
        /// 阶段 ID。
        /// </summary>
        AbilityPipelinePhaseId PhaseId { get; }

        /// <summary>
        /// 阶段类型标识。
        /// </summary>
        string PhaseType { get; }

        /// <summary>
        /// 阶段持续时间；-1 表示无限持续。
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// 阶段参数集合。
        /// </summary>
        Dictionary<string, object> Parameters { get; }
    }
}

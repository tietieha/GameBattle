using System.Collections.Generic;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线追踪记录器接口。
    /// 运行时使用空实现，编辑器使用完整实现。
    /// </summary>
    public interface IPipelineTraceRecorder
    {
        /// <summary>
        /// 是否启用追踪
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// 记录追踪数据
        /// </summary>
        void Record(IPipelineLifeOwner owner, PipelineTraceData data);

        /// <summary>
        /// 获取指定拥有者的追踪记录；未记录时返回空。
        /// </summary>
        IPipelineRunTrace? GetTrace(int ownerId);

        /// <summary>
        /// 获取指定拥有者的追踪快照；未记录时返回空集合。
        /// </summary>
        IReadOnlyList<PipelineTraceEvent> GetSnapshot(int ownerId);
    }

    /// <summary>
    /// 管线追踪记录接口（环形缓冲区实现）。
    /// </summary>
    public interface IPipelineRunTrace
    {
        /// <summary>
        /// 容量
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// 当前记录数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 添加追踪事件
        /// </summary>
        void Add(EPipelineTraceEventType type, AbilityPipelinePhaseId phaseId, EAbilityPipelineState state, string message);

        /// <summary>
        /// 获取当前追踪记录快照。
        /// </summary>
        IReadOnlyList<PipelineTraceEvent> GetSnapshot();
    }
}

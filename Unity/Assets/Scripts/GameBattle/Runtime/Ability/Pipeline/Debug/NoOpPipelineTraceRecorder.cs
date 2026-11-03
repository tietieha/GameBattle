using System;
using System.Collections.Generic;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线追踪记录器的空实现。
    /// </summary>
    public sealed class NoOpPipelineTraceRecorder : IPipelineTraceRecorder
    {
        /// <summary>
        /// 单例实例。
        /// </summary>
        public static readonly NoOpPipelineTraceRecorder Instance = new NoOpPipelineTraceRecorder();

        /// <summary>
        /// 空实现始终不启用。
        /// </summary>
        public bool IsEnabled => false;

        /// <summary>
        /// 不记录任何追踪数据。
        /// </summary>
        public void Record(IPipelineLifeOwner owner, PipelineTraceData data)
        {
        }

        /// <summary>
        /// 获取指定拥有者的追踪数据。
        /// </summary>
        public IPipelineRunTrace GetTrace(int ownerId)
        {
            return NoOpPipelineRunTrace.Instance;
        }

        /// <summary>
        /// 获取指定拥有者的追踪快照。
        /// </summary>
        public IReadOnlyList<PipelineTraceEvent> GetSnapshot(int ownerId)
        {
            return Array.Empty<PipelineTraceEvent>();
        }
    }

    /// <summary>
    /// 管线运行追踪的空实现。
    /// </summary>
    public sealed class NoOpPipelineRunTrace : IPipelineRunTrace
    {
        /// <summary>
        /// 单例实例。
        /// </summary>
        public static readonly NoOpPipelineRunTrace Instance = new NoOpPipelineRunTrace();

        /// <summary>
        /// 始终为 0。
        /// </summary>
        public int Capacity => 0;

        /// <summary>
        /// 始终为 0。
        /// </summary>
        public int Count => 0;

        /// <summary>
        /// 不保存任何记录。
        /// </summary>
        public void Add(EPipelineTraceEventType type, AbilityPipelinePhaseId phaseId, EAbilityPipelineState state, string message)
        {
        }

        /// <summary>
        /// 获取当前追踪记录快照。
        /// </summary>
        public IReadOnlyList<PipelineTraceEvent> GetSnapshot()
        {
            return Array.Empty<PipelineTraceEvent>();
        }
    }
}

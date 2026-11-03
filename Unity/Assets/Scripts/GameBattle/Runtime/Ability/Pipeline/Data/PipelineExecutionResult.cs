using System;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线执行结果，统一描述一次同步或托管执行的最终状态。
    /// </summary>
    public readonly struct PipelineExecutionResult
    {
        /// <summary>
        /// 创建管线执行结果。
        /// </summary>
        public PipelineExecutionResult(EAbilityPipelineState state, AbilityPipelinePhaseId lastPhaseId, Exception? exception = null)
        {
            State = state;
            LastPhaseId = lastPhaseId;
            Exception = exception;
        }

        /// <summary>
        /// 最终管线状态。
        /// </summary>
        public EAbilityPipelineState State { get; }

        /// <summary>
        /// 最后执行或失败的阶段 ID。
        /// </summary>
        public AbilityPipelinePhaseId LastPhaseId { get; }

        /// <summary>
        /// 执行期间捕获的异常；没有异常时为 null。
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// 是否成功完成。
        /// </summary>
        public bool Succeeded => State == EAbilityPipelineState.Completed;

        /// <summary>
        /// 是否处于失败或中断导致的终止状态。
        /// </summary>
        public bool IsTerminalFailure => State == EAbilityPipelineState.Failed;
    }
}

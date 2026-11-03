using System;
using AbilityKit.Pipeline;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线事件集合，集中承载运行、阶段和调试追踪回调。
    /// </summary>
    public class AbilityPipelineEvents<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        private int _sequence;

        /// <summary>
        /// 生成递增追踪序列号。
        /// </summary>
        internal int NextSequence => ++_sequence;

        /// <summary>
        /// 管线开始回调。
        /// </summary>
        public Action<TCtx>? OnPipelineStart;
        
        /// <summary>
        /// 管线完成回调。
        /// </summary>
        public Action<TCtx>? OnPipelineComplete;
        
        /// <summary>
        /// 管线失败回调。
        /// </summary>
        public Action<TCtx, Exception>? OnPipelineFailed;
        
        /// <summary>
        /// 管线错误回调。
        /// </summary>
        public Action<TCtx, Exception?>? OnPipelineError;
        
        /// <summary>
        /// 管线中断回调。
        /// </summary>
        public Action<TCtx, bool>? OnPipelineInterrupt;
        
        /// <summary>
        /// 管线暂停回调。
        /// </summary>
        public Action<TCtx>? OnPipelinePause;
        
        /// <summary>
        /// 管线恢复回调。
        /// </summary>
        public Action<TCtx>? OnPipelineResume;

        /// <summary>
        /// 阶段开始回调。
        /// </summary>
        public Action<IAbilityPipelinePhase<TCtx>, TCtx>? OnPhaseStart;
        
        /// <summary>
        /// 阶段完成回调。
        /// </summary>
        public Action<IAbilityPipelinePhase<TCtx>, TCtx>? OnPhaseComplete;
        
        /// <summary>
        /// 阶段错误回调。
        /// </summary>
        public Action<IAbilityPipelinePhase<TCtx>, TCtx, Exception>? OnPhaseError;

        /// <summary>
        /// 每帧更新回调。
        /// </summary>
        public Action<TCtx, float, EAbilityPipelineState>? OnTick;

        /// <summary>
        /// 记录追踪数据
        /// </summary>
        internal void RecordTrace(PipelineRuntime runtime, IPipelineLifeOwner owner, EPipelineTraceEventType type, AbilityPipelinePhaseId phaseId, EAbilityPipelineState state, string message)
        {
            var data = new PipelineTraceData(_sequence++, type, phaseId, state, message);
            runtime.RecordTrace(owner, data);
        }

        /// <summary>
        /// 记录追踪数据（带阶段信息）。
        /// </summary>
        internal void RecordTracePhase(PipelineRuntime runtime, IPipelineLifeOwner owner, EPipelineTraceEventType type, AbilityPipelinePhaseId phaseId, string phaseName, EAbilityPipelineState state)
        {
            var data = new PipelineTraceData(_sequence++, type, phaseId, state, phaseName ?? string.Empty);
            runtime.RecordTrace(owner, data);
        }
        
        /// <summary>
        /// 清除所有事件订阅并重置追踪序列。
        /// </summary>
        public void Clear()
        {
            OnPipelineStart = null;
            OnPipelineComplete = null;
            OnPipelineFailed = null;
            OnPipelineError = null;
            OnPipelineInterrupt = null;
            OnPipelinePause = null;
            OnPipelineResume = null;
            OnPhaseStart = null;
            OnPhaseComplete = null;
            OnPhaseError = null;
            OnTick = null;
            _sequence = 0;
        }
    }
}

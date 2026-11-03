namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 顺序执行阶段：按子阶段顺序依次执行。
    /// </summary>
    /// <typeparam name="TCtx">管线上下文类型。</typeparam>
    public class AbilitySequencePhase<TCtx> : AbilityCompositePhase<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        /// <summary>
        /// 创建默认顺序阶段。
        /// </summary>
        public AbilitySequencePhase() : base(new AbilityPipelinePhaseId("Sequence")) { }
        
        /// <summary>
        /// 使用指定阶段 ID 创建顺序阶段。
        /// </summary>
        public AbilitySequencePhase(AbilityPipelinePhaseId phaseId) : base(phaseId) { }

        /// <inheritdoc />
        public override IAbilityPipelinePhase<TCtx> CreateRunPhase()
        {
            var phase = new AbilitySequencePhase<TCtx>(PhaseId);
            CopySubPhasesTo(phase);
            return phase;
        }
    }
}

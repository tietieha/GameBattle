namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 延迟阶段：等待指定时间后继续。
    /// </summary>
    /// <typeparam name="TCtx">管线上下文类型。</typeparam>
    public class AbilityDelayPhase<TCtx> : AbilityDurationalPhaseBase<TCtx>, IAbilityPipelinePhaseInstanceFactory<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        /// <summary>
        /// 延迟时间（秒）。
        /// </summary>
        public float DelayTime
        {
            get => Duration;
            set => Duration = value;
        }

        /// <summary>
        /// 使用默认阶段名创建延迟阶段。
        /// </summary>
        public AbilityDelayPhase(float delayTime) : base("Delay")
        {
            DelayTime = delayTime;
        }

        /// <summary>
        /// 使用指定阶段 ID 创建延迟阶段。
        /// </summary>
        public AbilityDelayPhase(AbilityPipelinePhaseId phaseId, float delayTime) : base(phaseId)
        {
            DelayTime = delayTime;
        }

        /// <summary>
        /// 延迟阶段不执行额外逻辑。
        /// </summary>
        protected override void OnExecute(TCtx context)
        {
        }

        /// <inheritdoc />
        public IAbilityPipelinePhase<TCtx> CreateRunPhase()
        {
            return new AbilityDelayPhase<TCtx>(PhaseId, DelayTime);
        }

        /// <summary>
        /// 创建延迟阶段。
        /// </summary>
        public static AbilityDelayPhase<TCtx> Create(float delayTime)
        {
            return new AbilityDelayPhase<TCtx>(delayTime);
        }
    }
}

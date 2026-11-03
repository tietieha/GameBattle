namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 持续型阶段接口。
    /// </summary>
    public interface IDurationalPhase<TCtx> : IAbilityPipelinePhase<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        /// <summary>
        /// 阶段持续时间；小于 0 表示由阶段自身决定完成时机。
        /// </summary>
        float Duration { get; }
    }
}

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 瞬时阶段接口。实现应在执行调用内同步完成。
    /// </summary>
    public interface IAbilityInstantPhase<TCtx> : IAbilityPipelinePhase<TCtx>
        where TCtx : IAbilityPipelineContext
    {
    }
}

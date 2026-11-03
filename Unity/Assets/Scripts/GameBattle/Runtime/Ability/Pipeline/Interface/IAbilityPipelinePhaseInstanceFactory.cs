namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线阶段运行实例工厂。
    /// 用于把可复用的阶段定义转换成单次运行独占的阶段实例。
    /// </summary>
    /// <typeparam name="TCtx">管线上下文类型。</typeparam>
    public interface IAbilityPipelinePhaseInstanceFactory<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        /// <summary>
        /// 创建单次管线运行使用的阶段实例。
        /// </summary>
        IAbilityPipelinePhase<TCtx> CreateRunPhase();
    }
}

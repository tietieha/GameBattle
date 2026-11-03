namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线扩展点接口，用于观察阶段执行边界。
    /// </summary>
    public interface IAbilityPipelineExtensionPoint<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        /// <summary>
        /// 阶段开始时调用。
        /// </summary>
        void OnPhaseStart(TCtx context, IAbilityPipelinePhase<TCtx> phase);

        /// <summary>
        /// 阶段完成时调用。
        /// </summary>
        void OnPhaseComplete(TCtx context, IAbilityPipelinePhase<TCtx> phase);
    }
}

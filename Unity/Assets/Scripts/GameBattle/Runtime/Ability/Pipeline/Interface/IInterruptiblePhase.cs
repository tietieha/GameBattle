namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 可中断阶段接口。
    /// </summary>
    public interface IInterruptiblePhase<TCtx>
    {
        /// <summary>
        /// 中断当前阶段。
        /// </summary>
        void OnInterrupt(TCtx context);
    }
}

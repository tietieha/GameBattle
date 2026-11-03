namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 条件分支数据。
    /// </summary>
    public class AbilityConditionalBranch<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        /// <summary>
        /// 分支条件。
        /// </summary>
        public IAbilityConditionNode Condition { get; }

        /// <summary>
        /// 条件满足时执行的阶段。
        /// </summary>
        public IAbilityPipelinePhase<TCtx> Phase { get; }
    
        /// <summary>
        /// 创建条件分支。
        /// </summary>
        public AbilityConditionalBranch(IAbilityConditionNode condition, IAbilityPipelinePhase<TCtx> phase)
        {
            Condition = condition;
            Phase = phase;
        }
    }
}

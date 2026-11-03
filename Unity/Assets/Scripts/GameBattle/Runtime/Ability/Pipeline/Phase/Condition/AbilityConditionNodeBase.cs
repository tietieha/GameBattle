namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 条件节点基础实现。
    /// </summary>
    public abstract class AbilityConditionNodeBase : IAbilityConditionNode
    {
        /// <summary>
        /// 条件检查策略。
        /// </summary>
        public virtual EConditionCheckStrategy CheckStrategy { get; }
    
        /// <summary>
        /// 计算条件是否满足。
        /// </summary>
        public abstract bool Evaluate(IAbilityPipelineContext context);
    }
}

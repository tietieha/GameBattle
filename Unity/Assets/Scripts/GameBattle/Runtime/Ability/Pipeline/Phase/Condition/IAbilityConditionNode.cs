namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 条件节点接口，用于条件阶段选择分支。
    /// </summary>
    public interface IAbilityConditionNode
    {
        /// <summary>
        /// 判断当前上下文是否满足条件。
        /// </summary>
        bool Evaluate(IAbilityPipelineContext context);

        /// <summary>
        /// 条件检查策略。
        /// </summary>
        EConditionCheckStrategy CheckStrategy { get; }
    }
}

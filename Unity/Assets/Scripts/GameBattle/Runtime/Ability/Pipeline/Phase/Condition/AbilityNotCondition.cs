namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 条件非节点，对子条件结果取反。
    /// </summary>
    public class AbilityNotCondition : AbilityConditionNodeBase
    {
        private readonly IAbilityConditionNode _condition;

        /// <summary>
        /// 默认持续检查。
        /// </summary>
        public override EConditionCheckStrategy CheckStrategy => EConditionCheckStrategy.Continuous;
    
        /// <summary>
        /// 使用指定子条件创建非节点。
        /// </summary>
        public AbilityNotCondition(IAbilityConditionNode condition)
        {
            _condition = condition;
        }
    
        /// <summary>
        /// 计算子条件取反后的结果。
        /// </summary>
        public override bool Evaluate(IAbilityPipelineContext context)
        {
            if (!_condition.Evaluate(context))
                return true;
            return false;
        }
    }
}

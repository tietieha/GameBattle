using System.Collections.Generic;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 条件与节点，所有子条件都满足时返回满足。
    /// </summary>
    public class AbilityAndCondition : AbilityConditionNodeBase
    {
        private readonly IReadOnlyList<IAbilityConditionNode> _conditions;
        
        /// <summary>
        /// 默认持续检查。
        /// </summary>
        public override EConditionCheckStrategy CheckStrategy => EConditionCheckStrategy.Continuous;

        /// <summary>
        /// 使用可变条件列表创建与节点。
        /// </summary>
        public AbilityAndCondition(params IAbilityConditionNode[] conditions)
        {
            _conditions = conditions;
        }

        /// <summary>
        /// 使用条件列表创建与节点。
        /// </summary>
        public AbilityAndCondition(List<IAbilityConditionNode> conditions)
        {
            _conditions = conditions;
        }
    
        /// <summary>
        /// 计算所有子条件是否都满足。
        /// </summary>
        public override bool Evaluate(IAbilityPipelineContext context)
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (!_conditions[i].Evaluate(context))
                    return false;
            }
            return true;
        }
    }
}

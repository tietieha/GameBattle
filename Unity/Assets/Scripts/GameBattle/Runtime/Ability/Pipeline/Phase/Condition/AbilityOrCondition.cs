using System.Collections.Generic;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 条件或节点，任一子条件满足时返回满足。
    /// </summary>
    public class AbilityOrCondition : AbilityConditionNodeBase
    {
        private readonly IReadOnlyList<IAbilityConditionNode> _conditions;

        /// <summary>
        /// 默认持续检查。
        /// </summary>
        public override EConditionCheckStrategy CheckStrategy => EConditionCheckStrategy.Continuous;
    
        /// <summary>
        /// 使用可变条件列表创建或节点。
        /// </summary>
        public AbilityOrCondition(params IAbilityConditionNode[] conditions)
        {
            _conditions = conditions;
        }

        /// <summary>
        /// 使用条件列表创建或节点。
        /// </summary>
        public AbilityOrCondition(List<IAbilityConditionNode> conditions)
        {
            _conditions = conditions;
        }
    
        /// <summary>
        /// 计算是否存在满足的子条件。
        /// </summary>
        public override bool Evaluate(IAbilityPipelineContext context)
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].Evaluate(context))
                    return true;
            }
            return false;
        }
    }
}

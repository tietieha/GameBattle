using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Demo.Moba.Components;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    /// <summary>
    /// consume_resource 动作的强类型参数，用于技能释放时消耗资源。
    /// </summary>
    public readonly struct ConsumeResourceArgs
    {
        /// <summary>
        /// 资源类型。
        /// </summary>
        public readonly ResourceType ResourceType;

        /// <summary>
        /// 消耗量。
        /// </summary>
        public readonly float Amount;

        /// <summary>
        /// 消耗失败时的提示信息键。
        /// </summary>
        public readonly string FailMessageKey;

        public ConsumeResourceArgs(ResourceType resourceType, float amount, string failMessageKey = null)
        {
            ResourceType = resourceType;
            Amount = amount;
            FailMessageKey = failMessageKey ?? "not_enough_resource";
        }

        public static ConsumeResourceArgs Default => new ConsumeResourceArgs(ResourceType.Mana, 0f, "not_enough_mana");
    }
}

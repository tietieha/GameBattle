using System;
using System.Collections.Generic;
using AbilityKit.Demo.Moba.Components;
using AbilityKit.Ability.World.DI;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;
using AbilityKit.Demo.Moba.Systems;


namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    /// <summary>
    /// consume_resource 计划动作的结构描述。
    /// 从具名动作参数中解析配置的资源类型和消耗数量。
    /// </summary>
    public sealed class ConsumeResourceSchema : MobaPlanActionSchemaBase<ConsumeResourceArgs>
    {
        public static readonly ConsumeResourceSchema Instance = new ConsumeResourceSchema();

        protected override string ActionName => TriggeringConstants.Actions.ConsumeResource;

        public override ConsumeResourceArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            var amount = ReadFloat(namedArgs, ctx, 0f, "amount", "cost", "value");
            var resourceType = ReadEnum(namedArgs, ctx, ResourceType.Mana, "resource_type", "resourcetype", "type");
            var failMessageKey = "not_enough_resource";

            return new ConsumeResourceArgs(resourceType, amount, failMessageKey);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            error = null;
            return true;
        }
    }
}

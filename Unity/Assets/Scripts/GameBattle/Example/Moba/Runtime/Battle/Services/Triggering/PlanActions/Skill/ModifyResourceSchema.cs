using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Components;
using AbilityKit.Demo.Moba.Systems;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public sealed class ModifyResourceSchema : MobaPlanActionSchemaBase<ModifyResourceArgs>
    {
        public static readonly ModifyResourceSchema Instance = new ModifyResourceSchema();

        protected override string ActionName => TriggeringConstants.Actions.ModifyResource;

        public override ModifyResourceArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            return ParseArgs(namedArgs, ctx, default);
        }

        public override ModifyResourceArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx, in TriggerActionParseContext parseContext)
        {
            var resourceType = ReadEnum(namedArgs, ctx, ResourceType.None, "resource_type", "resourcetype", "type");
            var amount = ReadFloat(namedArgs, ctx, 0f, "amount", "delta", "value");
            var hasMin = TryReadNumber(namedArgs, ctx, out var min, "min", "min_value", "minvalue");
            var hasMax = TryReadNumber(namedArgs, ctx, out var max, "max", "max_value", "maxvalue");
            var targetRequest = MobaActionTargetSchemaReader.Read(namedArgs, ctx, in parseContext);
            return new ModifyResourceArgs(resourceType, amount, (float)min, (float)max, hasMin, hasMax, in targetRequest);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            return RequireAny(args, "amount", out error, "amount", "delta", "value");
        }
    }
}

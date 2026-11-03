using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba;
using AbilityKit.Demo.Moba.Components;
using AbilityKit.Demo.Moba.Systems;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public sealed class ConvertResourceToHealSchema : MobaPlanActionSchemaBase<ConvertResourceToHealArgs>
    {
        public static readonly ConvertResourceToHealSchema Instance = new ConvertResourceToHealSchema();

        protected override string ActionName => TriggeringConstants.Actions.ConvertResourceToHeal;

        public override ConvertResourceToHealArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            return ParseArgs(namedArgs, ctx, default);
        }

        public override ConvertResourceToHealArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx, in TriggerActionParseContext parseContext)
        {
            var resourceType = ReadEnum(namedArgs, ctx, ResourceType.None, "resource_type", "resourcetype", "type");
            var amount = ReadFloat(namedArgs, ctx, 0f, "amount", "resource_amount", "resourceamount", "value");
            var healRatio = ReadFloat(namedArgs, ctx, 1f, "heal_ratio", "healratio", "ratio");
            var outOfCombatSeconds = ReadFloat(namedArgs, ctx, 0f, "out_of_combat_seconds", "outofcombatseconds", "out_of_combat", "outofcombat");
            var healType = ReadEnum(namedArgs, ctx, DamageType.None, "heal_type", "healtype", "heal_damage_type");
            var reasonKind = ReadInt(namedArgs, ctx, (int)DamageReasonKind.Buff, "reason_kind", "reasonkind");
            var reasonParam = ReadInt(namedArgs, ctx, 0, "reason_param", "reasonparam");
            var targetRequest = MobaActionTargetSchemaReader.Read(namedArgs, ctx, in parseContext);
            return new ConvertResourceToHealArgs(resourceType, amount, healRatio, outOfCombatSeconds, healType, reasonKind, reasonParam, in targetRequest);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            if (!RequireAny(args, "amount", out error, "amount", "resource_amount", "resourceamount", "value"))
            {
                return false;
            }

            return true;
        }
    }
}

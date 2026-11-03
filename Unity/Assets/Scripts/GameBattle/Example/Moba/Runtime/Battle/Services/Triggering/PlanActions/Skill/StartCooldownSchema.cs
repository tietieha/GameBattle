using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Services;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public sealed class StartCooldownSchema : MobaPlanActionSchemaBase<StartCooldownArgs>
    {
        public static readonly StartCooldownSchema Instance = new StartCooldownSchema();

        protected override string ActionName => Systems.TriggeringConstants.Actions.StartCooldown;

        public override StartCooldownArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            return ParseArgs(namedArgs, ctx, default(TriggerActionParseContext));
        }

        public override StartCooldownArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx, in TriggerActionParseContext parseContext)
        {
            var skillId = ReadInt(namedArgs, ctx, 0, "skill_id", "skillid");
            var skillSlot = ReadInt(namedArgs, ctx, 0, "skill_slot", "skillslot", "slot");
            var cooldownMs = ReadInt(namedArgs, ctx, 0, "cooldown_ms", "cooldownms", "duration_ms", "durationms");

            if (skillId <= 0)
            {
                skillId = ReadCurrentPayloadInt(ctx, in parseContext, SkillRulePayloadFields.SkillId, SkillRulePayloadFields.FieldId);
            }

            if (skillSlot <= 0)
            {
                skillSlot = ReadCurrentPayloadInt(ctx, in parseContext, SkillRulePayloadFields.SkillSlot, SkillRulePayloadFields.FieldId);
            }

            return new StartCooldownArgs(skillId, skillSlot, cooldownMs);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            if (!RequireAny(args, "cooldown_ms", out error, "cooldown_ms", "cooldownms", "duration_ms", "durationms")) return false;
            error = null;
            return true;
        }

    }
}

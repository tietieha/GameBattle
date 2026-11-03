using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Systems;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public sealed class CancelSkillSchema : MobaPlanActionSchemaBase<CancelSkillArgs>
    {
        public static readonly CancelSkillSchema Instance = new CancelSkillSchema();

        protected override string ActionName => TriggeringConstants.Actions.CancelSkill;

        public override CancelSkillArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            var mode = ReadEnum(namedArgs, ctx, CancelSkillMode.Auto, "mode", "cancel_mode", "cancelmode");
            var skillId = ReadInt(namedArgs, ctx, 0, "skill_id", "skillid", "id");
            var skillSlot = ReadInt(namedArgs, ctx, 0, "skill_slot", "skillslot", "slot");
            var removeAll = ReadBool(namedArgs, ctx, true, "remove_all", "removeall", "all");
            var targetRequest = MobaActionTargetSchemaReader.Read(namedArgs, ctx);
            return new CancelSkillArgs(mode, skillId, skillSlot, removeAll, in targetRequest);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            error = null;
            return true;
        }
    }
}

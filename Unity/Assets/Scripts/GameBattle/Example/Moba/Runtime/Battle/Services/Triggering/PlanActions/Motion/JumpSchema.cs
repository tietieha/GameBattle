using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Services.Motion;
using AbilityKit.Demo.Moba.Systems;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public sealed class JumpSchema : MobaPlanActionSchemaBase<JumpArgs>
    {
        public static readonly JumpSchema Instance = new JumpSchema();

        protected override string ActionName => TriggeringConstants.Actions.Jump;

        public override JumpArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            var height = ReadFloat(namedArgs, ctx, 0f, "height", "jump_height", "jumpheight");
            var durationMs = ReadFloat(namedArgs, ctx, 0f, "duration_ms", "duration", "durationms");
            var priority = ReadInt(namedArgs, ctx, 10, "priority");
            var applyToCaster = ReadBool(namedArgs, ctx, true, "apply_to_caster", "applytocaster");
            var motionGroupId = ReadInt(namedArgs, ctx, 0, "motion_group_id", "motiongroupid", "motion_group", "motiongroup", "group_id", "groupid");
            var continuous = ReadContinuousSettings(namedArgs, ctx);
            var landingTriggerIds = ReadPositiveInts(namedArgs, ctx, "landing_trigger_ids", "landingtriggerids", "landing_trigger_id", "landingtriggerid", "on_landing_trigger_ids", "onlandingtriggerids");

            return new JumpArgs(height, durationMs, priority, applyToCaster, motionGroupId, continuous, landingTriggerIds);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            error = null;
            return true;
        }

        private static MobaMotionContinuousSettings ReadContinuousSettings(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            var continuousProcessId = ReadInt(namedArgs, ctx, 0, "continuous_process_id", "continuousprocessid", "continuous_id", "continuousid");
            var continuousTagTemplateId = ReadInt(namedArgs, ctx, 0, "continuous_tag_template_id", "continuoustagtemplateid", "tag_template_id", "tagtemplateid");
            var triggerIds = ReadPositiveInts(namedArgs, ctx, "trigger_ids", "triggerids", "owner_trigger_ids", "ownertriggerids");
            var intervalMs = ReadInt(namedArgs, ctx, 0, "interval_ms", "intervalms", "interval");
            var intervalTriggerIds = ReadPositiveInts(namedArgs, ctx, "interval_trigger_ids", "intervaltriggerids", "interval_effect_ids", "intervaleffectids");
            return new MobaMotionContinuousSettings(continuousProcessId, continuousTagTemplateId, triggerIds, intervalMs, intervalTriggerIds);
        }
    }
}

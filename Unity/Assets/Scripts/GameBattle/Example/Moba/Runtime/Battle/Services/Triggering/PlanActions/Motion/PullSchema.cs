using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Services.Motion;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;
using AbilityKit.Demo.Moba.Systems;


namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    /// <summary>
    /// pull Action 参数 Schema 定义。
    /// </summary>
    public sealed class PullSchema : MobaPlanActionSchemaBase<PullArgs>
    {
        public static readonly PullSchema Instance = new PullSchema();

        protected override string ActionName => TriggeringConstants.Actions.Pull;

        public override PullArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            var speed = ReadFloat(namedArgs, ctx, 0f, "speed");
            var durationMs = ReadFloat(namedArgs, ctx, 0f, "duration_ms", "duration", "durationms");
            var directionMode = ReadInt(namedArgs, ctx, 0, "direction_mode", "directionmode", "dir_mode");
            var targetDistance = ReadFloat(namedArgs, ctx, 0f, "target_distance", "targetdistance");
            var priority = ReadInt(namedArgs, ctx, 12, "priority");
            var motionGroupId = ReadInt(namedArgs, ctx, 0, "motion_group_id", "motiongroupid", "motion_group", "motiongroup", "group_id", "groupid");
            var continuous = ReadContinuousSettings(namedArgs, ctx);
            var targetRequest = MobaActionTargetSchemaReader.Read(namedArgs, ctx);

            return new PullArgs(speed, durationMs, directionMode, targetDistance, priority, motionGroupId, continuous, targetRequest);
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

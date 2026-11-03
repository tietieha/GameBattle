using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Gameplay.Triggering;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    internal sealed class MobaActionTargetSchemaReader : MobaPlanActionSchemaBase<MobaActionTargetRequest>
    {
        public static MobaActionTargetRequest Read(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            return Instance.ParseArgs(namedArgs, ctx);
        }

        public static MobaActionTargetRequest Read(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx, in TriggerActionParseContext parseContext)
        {
            return Instance.ParseArgs(namedArgs, ctx, in parseContext);
        }

        private static readonly MobaActionTargetSchemaReader Instance = new MobaActionTargetSchemaReader();

        protected override string ActionName => "target_query";

        public override MobaActionTargetRequest ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            return ParseArgs(namedArgs, ctx, default);
        }

        public override MobaActionTargetRequest ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx, in TriggerActionParseContext parseContext)
        {
            var queryTemplateId = ReadInt(namedArgs, ctx, 0,
                "query_template_id", "querytemplateid", "target_query_id", "targetqueryid", "target_query_template_id", "targetquerytemplateid");
            var targetActorId = ReadInt(namedArgs, ctx, 0, "target_actor_id", "targetactorid");
            var targetPayloadFieldId = ReadInt(namedArgs, ctx, 0,
                "target_payload_field_id", "targetpayloadfieldid", "target_actor_payload_field_id", "targetactorpayloadfieldid");
            var targetPayloadActorId = targetPayloadFieldId > 0
                ? ReadCurrentPayloadInt(ctx, in parseContext, targetPayloadFieldId, 0)
                : 0;
            var sourceCode = ReadEnum(namedArgs, ctx, MobaActionTargetSourceCode.ContextTarget,
                "target_source", "targetsource", "source_code", "sourcecode", "target_provider", "targetprovider", "target");
            var sourceParam = ReadInt(namedArgs, ctx, 0,
                "target_source_param", "targetsourceparam", "source_param", "sourceparam", "target_provider_param", "targetproviderparam");
            var filterCode = ReadEnum(namedArgs, ctx, MobaActionTargetFilterCode.None,
                "target_filter", "targetfilter", "filter_code", "filtercode", "target_rule", "targetrule");
            var filterParam = ReadInt(namedArgs, ctx, 0,
                "target_filter_param", "targetfilterparam", "filter_param", "filterparam", "target_rule_param", "targetruleparam");
            var radius = ReadFloat(namedArgs, ctx, 0f, "target_radius", "targetradius", "radius");
            var halfAngleDeg = ReadFloat(namedArgs, ctx, 0f,
                "target_half_angle_deg", "targethalfangledeg", "half_angle_deg", "halfangledeg");
            var orderCode = ReadEnum(namedArgs, ctx, MobaActionTargetOrderCode.DistanceToCaster,
                "target_order", "targetorder", "order_code", "ordercode", "target_scorer", "targetscorer");
            var orderParam = ReadInt(namedArgs, ctx, 0,
                "target_order_param", "targetorderparam", "order_param", "orderparam", "random_seed", "randomseed");
            var selectCode = ReadEnum(namedArgs, ctx, MobaActionTargetSelectCode.TopK,
                "target_select", "targetselect", "select_code", "selectcode", "target_selector", "targetselector");
            var maxCount = ReadInt(namedArgs, ctx, 1, "target_max_count", "targetmaxcount", "max_count", "maxcount", "count");

            var legacyMode = ReadInt(namedArgs, ctx, int.MinValue, "target_mode", "targetmode");
            if (legacyMode != int.MinValue)
            {
                sourceCode = ConvertLegacyMode(legacyMode, queryTemplateId, targetActorId);
            }

            var targetSelf = ReadBoolNonZero(namedArgs, ctx, false, "target_self", "targetself", "self");
            if (targetSelf) sourceCode = MobaActionTargetSourceCode.Self;
            if ((targetActorId > 0 || targetPayloadActorId > 0) && sourceCode == MobaActionTargetSourceCode.ContextTarget)
            {
                sourceCode = MobaActionTargetSourceCode.ExplicitActor;
            }
            if (queryTemplateId > 0 && sourceCode == MobaActionTargetSourceCode.ContextTarget)
            {
                sourceCode = MobaActionTargetSourceCode.SearchQueryTemplate;
            }

            return new MobaActionTargetRequest(
                sourceCode,
                sourceParam,
                filterCode,
                filterParam,
                radius,
                halfAngleDeg,
                orderCode,
                orderParam,
                selectCode,
                maxCount,
                queryTemplateId,
                targetActorId,
                targetPayloadActorId);
        }

        public override bool TryValidateArgs(System.ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            error = null;
            return true;
        }

        private static MobaActionTargetSourceCode ConvertLegacyMode(int legacyMode, int queryTemplateId, int targetActorId)
        {
            switch (legacyMode)
            {
                case 1:
                    return MobaActionTargetSourceCode.Self;
                case 2:
                    return MobaActionTargetSourceCode.ExplicitActor;
                case 3:
                    return MobaActionTargetSourceCode.SearchQueryTemplate;
                default:
                    if (queryTemplateId > 0) return MobaActionTargetSourceCode.SearchQueryTemplate;
                    if (targetActorId > 0) return MobaActionTargetSourceCode.ExplicitActor;
                    return MobaActionTargetSourceCode.ContextTarget;
            }
        }
    }
}

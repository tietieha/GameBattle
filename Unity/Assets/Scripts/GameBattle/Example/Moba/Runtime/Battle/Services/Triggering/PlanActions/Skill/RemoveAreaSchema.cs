using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Demo.Moba.Systems;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public sealed class RemoveAreaSchema : MobaPlanActionSchemaBase<RemoveAreaArgs>
    {
        public static readonly RemoveAreaSchema Instance = new RemoveAreaSchema();

        protected override string ActionName => TriggeringConstants.Actions.RemoveArea;

        public override RemoveAreaArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            var areaId = ReadInt(namedArgs, ctx, 0, "area_id", "areaid", "runtime_area_id", "runtimeareaid");
            var templateId = ReadInt(namedArgs, ctx, 0, "template_id", "templateid", "aoe_id", "aoeid", "id");
            var ownerActorId = ReadInt(namedArgs, ctx, 0, "owner_actor_id", "owneractorid", "owner_id", "ownerid", "source_actor_id", "sourceactorid");
            var removeAll = ReadBool(namedArgs, ctx, true, "remove_all", "removeall", "all");
            var targetRequest = MobaActionTargetSchemaReader.Read(namedArgs, ctx);
            return new RemoveAreaArgs(areaId, templateId, ownerActorId, removeAll, in targetRequest);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            if (HasAny(args,
                    "area_id", "areaid", "runtime_area_id", "runtimeareaid",
                    "template_id", "templateid", "aoe_id", "aoeid", "id",
                    "owner_actor_id", "owneractorid", "owner_id", "ownerid", "source_actor_id", "sourceactorid"))
            {
                error = null;
                return true;
            }

            error = "remove_area requires area_id, template_id, or owner_actor_id.";
            return false;
        }
    }
}

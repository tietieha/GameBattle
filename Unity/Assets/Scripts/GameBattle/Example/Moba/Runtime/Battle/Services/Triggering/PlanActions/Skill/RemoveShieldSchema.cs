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
    public sealed class RemoveShieldSchema : MobaPlanActionSchemaBase<RemoveShieldArgs>
    {
        public static readonly RemoveShieldSchema Instance = new RemoveShieldSchema();

        protected override string ActionName => TriggeringConstants.Actions.RemoveShield;

        public override RemoveShieldArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            var shieldId = ReadInt(namedArgs, ctx, 0, "shield_id", "shieldid", "id");
            var instanceId = ReadInt(namedArgs, ctx, 0, "instance_id", "instanceid", "shield_instance_id", "shieldinstanceid");
            var sourceActorId = ReadInt(namedArgs, ctx, 0, "source_actor_id", "sourceactorid", "source_id", "sourceid");
            var removeAll = ReadBool(namedArgs, ctx, true, "remove_all", "removeall", "all");
            var targetRequest = MobaActionTargetSchemaReader.Read(namedArgs, ctx);
            return new RemoveShieldArgs(shieldId, instanceId, sourceActorId, removeAll, in targetRequest);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            if (HasAny(args, "instance_id", "instanceid", "shield_instance_id", "shieldinstanceid") ||
                HasAny(args, "shield_id", "shieldid", "id"))
            {
                error = null;
                return true;
            }

            error = "remove_shield requires instance_id or shield_id.";
            return false;
        }
    }
}

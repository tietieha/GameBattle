using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Demo.Moba.Events.Summon;
using AbilityKit.Demo.Moba.Systems;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public sealed class RemoveSummonSchema : MobaPlanActionSchemaBase<RemoveSummonArgs>
    {
        public static readonly RemoveSummonSchema Instance = new RemoveSummonSchema();

        protected override string ActionName => TriggeringConstants.Actions.RemoveSummon;

        public override RemoveSummonArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            var summonId = ReadInt(namedArgs, ctx, 0, "summon_id", "summonid", "id");
            var summonActorId = ReadInt(namedArgs, ctx, 0, "summon_actor_id", "summonactorid", "actor_id", "actorid");
            var rootOwnerActorId = ReadInt(namedArgs, ctx, 0, "root_owner_actor_id", "rootowneractorid", "owner_actor_id", "owneractorid", "owner_id", "ownerid");
            var removeAll = ReadBool(namedArgs, ctx, true, "remove_all", "removeall", "all");
            var reason = ReadEnum(namedArgs, ctx, SummonDespawnReason.ManualRemove, "reason", "despawn_reason", "despawnreason");
            var targetRequest = MobaActionTargetSchemaReader.Read(namedArgs, ctx);
            return new RemoveSummonArgs(summonId, summonActorId, rootOwnerActorId, removeAll, reason, in targetRequest);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            error = null;
            return true;
        }
    }
}

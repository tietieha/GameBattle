using System;
using System.Collections.Generic;
using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Ability.World.DI;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;
using AbilityKit.Demo.Moba.Systems;


namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public sealed class AddBuffSchema : MobaPlanActionSchemaBase<AddBuffArgs>
    {
        public static readonly AddBuffSchema Instance = new AddBuffSchema();

        protected override string ActionName => TriggeringConstants.Actions.AddBuff;

        public override AddBuffArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            var buffIds = ReadPositiveInts(namedArgs, ctx, "buffids", "buff_ids", "buffid", "buff_id", "id", "ids");
            var targetRequest = MobaActionTargetSchemaReader.Read(namedArgs, ctx);
            return new AddBuffArgs(buffIds, in targetRequest);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            return RequireAny(args, "buffIds", out error, "buffids", "buff_ids", "buffid", "buff_id");
        }
    }
}

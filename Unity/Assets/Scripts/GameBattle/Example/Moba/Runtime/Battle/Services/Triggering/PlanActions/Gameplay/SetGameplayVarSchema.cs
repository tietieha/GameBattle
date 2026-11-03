using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Systems;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    public sealed class SetGameplayVarSchema : MobaPlanActionSchemaBase<SetGameplayVarArgs>
    {
        public static readonly SetGameplayVarSchema Instance = new SetGameplayVarSchema();

        protected override string ActionName => TriggeringConstants.Actions.SetGameplayVar;

        public override SetGameplayVarArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            var keyId = ReadInt(namedArgs, ctx, 0, "key_id", "keyid", "id");
            var value = ReadFloat(namedArgs, ctx, 0f, "value", "amount");
            return new SetGameplayVarArgs(keyId, value);
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            return RequireAny(args, "key_id", out error, "key_id", "keyid", "id");
        }
    }
}

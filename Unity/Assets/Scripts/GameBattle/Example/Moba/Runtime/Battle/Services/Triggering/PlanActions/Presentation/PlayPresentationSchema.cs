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
    public sealed class PlayPresentationSchema : MobaPlanActionSchemaBase<PlayPresentationArgs>
    {
        public static readonly PlayPresentationSchema Instance = new PlayPresentationSchema();

        protected override string ActionName => TriggeringConstants.Actions.PlayPresentation;

        public override PlayPresentationArgs ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            return new PlayPresentationArgs(ReadInt(namedArgs, ctx, 0, "template_id", "templateid", "id"))
            {
                TargetMode = ReadInt(namedArgs, ctx, 0, "target_mode", "targetmode", "target"),
                RequestKey = null,
                DurationMs = ReadInt(namedArgs, ctx, 0, "duration_ms", "durationms"),
                Stop = ReadBoolNonZero(namedArgs, ctx, false, "stop"),
                X = ReadFloat(namedArgs, ctx, 0f, "x"),
                Y = ReadFloat(namedArgs, ctx, 0f, "y"),
                Z = ReadFloat(namedArgs, ctx, 0f, "z"),
                Scale = ReadFloat(namedArgs, ctx, 1f, "scale"),
                Radius = ReadFloat(namedArgs, ctx, 0f, "radius")
            };
        }

        public override bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            return RequireAny(args, "templateId", out error, "template_id", "templateid", "id");
        }
    }
}

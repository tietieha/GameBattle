using System;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Ability.World.Services;
using AbilityKit.Ability.World.Services.Attributes;

namespace AbilityKit.Demo.Moba.Services
{
    [WorldService(typeof(MobaComponentTemplateService))]
    public sealed class MobaComponentTemplateService : IService
    {
        [WorldInject] private MobaConfigDatabase _config = null;
        [WorldInject(required: false)] private IFrameTime _frameTime = null;
        [WorldInject(required: false)] private IWorldClock _clock = null;

        public bool TryApply(global::ActorEntity entity, int templateId)
        {
            Apply(entity, templateId);
            return true;
        }

        public void Apply(global::ActorEntity entity, int templateId)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (templateId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(templateId), templateId, "Component template id must be positive.");
            }

            if (_config == null)
            {
                throw new InvalidOperationException("MobaComponentTemplateService requires MobaConfigDatabase.");
            }

            if (!_config.TryGetComponentTemplate(templateId, out var template) || template == null)
            {
                throw new InvalidOperationException($"Component template not found. templateId={templateId}");
            }

            if (template.Ops == null || template.Ops.Count == 0)
            {
                throw new InvalidOperationException($"Component template requires at least one operation. templateId={templateId}");
            }

            for (int i = 0; i < template.Ops.Count; i++)
            {
                var op = template.Ops[i];
                if (op == null)
                {
                    throw new InvalidOperationException($"Component template operation is null. templateId={templateId}, opIndex={i}");
                }

                ApplyOp(entity, templateId, i, op.Kind, op.IntValue, op.FloatValue, op.BoolValue);
            }
        }

        private void ApplyOp(global::ActorEntity entity, int templateId, int opIndex, int kind, int intValue, float floatValue, bool boolValue)
        {
            switch ((MobaComponentOpKind)kind)
            {
                case MobaComponentOpKind.SetModelId:
                {
                    if (intValue <= 0)
                    {
                        throw new InvalidOperationException($"SetModelId operation requires positive model id. templateId={templateId}, opIndex={opIndex}, value={intValue}");
                    }

                    if (entity.hasModelId) entity.ReplaceModelId(intValue);
                    else entity.AddModelId(intValue);
                    break;
                }
                case MobaComponentOpKind.SetLifetimeMs:
                {
                    if (intValue <= 0)
                    {
                        throw new InvalidOperationException($"SetLifetimeMs operation requires positive lifetime ms. templateId={templateId}, opIndex={opIndex}, value={intValue}");
                    }

                    var endMs = NowMs() + intValue;
                    if (entity.hasLifetime) entity.ReplaceLifetime(endMs);
                    else entity.AddLifetime(endMs);
                    break;
                }
                default:
                    throw new InvalidOperationException($"Unsupported component template operation. templateId={templateId}, opIndex={opIndex}, kind={kind}");
            }
        }

        private long NowMs()
        {
            if (_frameTime != null)
            {
                return (long)MathF.Round(_frameTime.Time * 1000f);
            }
            if (_clock != null)
            {
                return (long)MathF.Round(_clock.Time * 1000f);
            }
            throw new InvalidOperationException("MobaComponentTemplateService requires IFrameTime or IWorldClock for current time.");
        }

        public void Dispose()
        {
        }
    }
}

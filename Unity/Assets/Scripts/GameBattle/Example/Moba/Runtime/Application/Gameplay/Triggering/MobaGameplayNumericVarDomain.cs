using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Gameplay;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Variables.Numeric;

namespace AbilityKit.Demo.Moba.Gameplay.Triggering
{
    public sealed class MobaGameplayNumericVarDomain : INumericVarDomain
    {
        public const string Domain = "gameplay";

        public string DomainId => Domain;

        public bool TryGet<TCtx>(in ExecCtx<TCtx> ctx, string key, out double value)
        {
            value = 0d;
            if (string.IsNullOrEmpty(key)) return false;
            if (!(ctx.Context is IWorldResolver services)) return false;
            if (!services.TryResolve<MobaGameplayVariableService>(out var variables) || variables == null) return false;
            if (!int.TryParse(key, out var keyId)) return false;
            return variables.TryGet(keyId, out value);
        }

        public bool TrySet<TCtx>(in ExecCtx<TCtx> ctx, string key, double value)
        {
            if (string.IsNullOrEmpty(key)) return false;
            if (!(ctx.Context is IWorldResolver services)) return false;
            if (!services.TryResolve<MobaGameplayVariableService>(out var variables) || variables == null) return false;
            if (!int.TryParse(key, out var keyId)) return false;

            variables.Set(keyId, value);
            return true;
        }
    }
}

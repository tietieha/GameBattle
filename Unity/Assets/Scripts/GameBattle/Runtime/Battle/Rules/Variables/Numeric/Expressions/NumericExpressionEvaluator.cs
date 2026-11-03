using AbilityKit.Triggering.Variables.Numeric;

namespace AbilityKit.Triggering.Variables.Numeric.Expression
{
    public static class NumericExpressionEvaluator
    {
        public static bool TryEvaluate<TCtx>(in AbilityKit.Triggering.Runtime.ExecCtx<TCtx> ctx, NumericRpnProgram program, out double value)
        {
            var localCtx = ctx;
            return NumericRpnTokenEvaluator.TryEvaluate(
                program,
                (string domainId, string key, out double resolved) => localCtx.TryGetNumericVar(domainId, key, out resolved),
                localCtx.NumericFunctions ?? DefaultNumericRpnFunctionRegistry.Instance,
                out value);
        }
    }
}

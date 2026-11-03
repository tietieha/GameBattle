using System;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Variables.Numeric;

namespace AbilityKit.Triggering.Runtime.Plan
{
    /// <summary>
    /// Formal NumericValueRef resolver for plan/action schema code using ExecCtx<TCtx>.
    /// </summary>
    public static class NumericValueRefResolver
    {
        public static bool TryResolve<TArgs, TCtx>(in NumericValueRef valueRef, in TArgs args, in ExecCtx<TCtx> ctx, out double value)
        {
            return ActionSchemaRegistry.TryResolveNumericRef(in valueRef, in args, in ctx, out value);
        }

        public static double Resolve<TArgs, TCtx>(in NumericValueRef valueRef, in TArgs args, in ExecCtx<TCtx> ctx)
        {
            if (TryResolve(in valueRef, in args, in ctx, out var value))
                return value;

            throw new InvalidOperationException($"Failed to resolve numeric value reference: {Describe(in valueRef)}");
        }

        private static string Describe(in NumericValueRef valueRef)
        {
            return valueRef.Kind switch
            {
                ENumericValueRefKind.Const => $"kind=Const const={valueRef.ConstValue}",
                ENumericValueRefKind.Blackboard => $"kind=Blackboard boardId={valueRef.BoardId} keyId={valueRef.KeyId} required={valueRef.Required} fallback={valueRef.HasFallback}:{valueRef.FallbackValue}",
                ENumericValueRefKind.PayloadField => $"kind=PayloadField fieldId={valueRef.FieldId} required={valueRef.Required} fallback={valueRef.HasFallback}:{valueRef.FallbackValue}",
                ENumericValueRefKind.Var => $"kind=Var domain={valueRef.DomainId} key={valueRef.Key} required={valueRef.Required} fallback={valueRef.HasFallback}:{valueRef.FallbackValue}",
                ENumericValueRefKind.Expr => $"kind=Expr expr={valueRef.ExprText} required={valueRef.Required} fallback={valueRef.HasFallback}:{valueRef.FallbackValue}",
                _ => $"kind={(int)valueRef.Kind} required={valueRef.Required} fallback={valueRef.HasFallback}:{valueRef.FallbackValue}"
            };
        }
    }
}

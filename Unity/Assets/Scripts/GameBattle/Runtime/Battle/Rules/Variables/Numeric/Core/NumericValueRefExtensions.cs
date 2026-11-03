using System;
using AbilityKit.Triggering.Runtime.Abstractions;
using AbilityKit.Triggering.Runtime.Context;
using AbilityKit.Triggering.Runtime.Plan;
using AbilityKit.Triggering.Variables.Numeric.Expression;

namespace AbilityKit.Triggering.Variables.Numeric
{
    /// <summary>
    /// NumericValueRef 解析扩展方法
    /// </summary>
    public static class NumericValueRefExtensions
    {
        public static double Resolve(this in NumericValueRef ref_, ActionContext context)
        {
            if (TryResolve(in ref_, context, out var value))
                return value;

            throw new InvalidOperationException("Failed to resolve numeric value reference: " + Describe(in ref_));
        }

        public static bool TryResolve(this in NumericValueRef ref_, ActionContext context, out double value)
        {
            if (!TryResolveSource(in ref_, context, out value))
            {
                if (!ref_.HasFallback || ref_.Required)
                {
                    value = 0.0;
                    return false;
                }

                value = ref_.FallbackValue;
            }

            value = ApplyNumericValuePolicy(in ref_, value);
            return true;
        }

        private static bool TryResolveSource(in NumericValueRef ref_, ActionContext context, out double value)
        {
            value = 0.0;
            return ref_.Kind switch
            {
                ENumericValueRefKind.Const => TryResolveConst(in ref_, out value),
                ENumericValueRefKind.Blackboard => TryResolveBlackboard(in ref_, context, out value),
                ENumericValueRefKind.PayloadField => TryResolvePayloadField(in ref_, context, out value),
                ENumericValueRefKind.Var => TryResolveVar(in ref_, context, out value),
                ENumericValueRefKind.Expr => TryResolveExpr(in ref_, context, out value),
                _ => false
            };
        }

        private static bool TryResolveConst(in NumericValueRef ref_, out double value)
        {
            value = ref_.ConstValue;
            return true;
        }

        private static bool TryResolveBlackboard(in NumericValueRef ref_, ActionContext context, out double value)
        {
            value = 0.0;
            return context?.Blackboard != null && context.Blackboard.TryGetDouble(ref_.BoardId, ref_.KeyId, out value);
        }

        private static bool TryResolvePayloadField(in NumericValueRef ref_, ActionContext context, out double value)
        {
            value = 0.0;
            return context?.Payloads != null && context.Payloads.TryGetDouble(ref_.FieldId, out value);
        }

        private static bool TryResolveVar(in NumericValueRef ref_, ActionContext context, out double value)
        {
            return TryResolveVar(context, ref_.DomainId, ref_.Key, out value);
        }

        private static bool TryResolveExpr(in NumericValueRef ref_, ActionContext context, out double value)
        {
            value = 0.0;
            if (string.IsNullOrEmpty(ref_.ExprText))
                return false;

            if (!NumericExpressionCompiler.TryCompileCached(ref_.ExprText, out var program) || program == null)
                return false;

            return NumericRpnTokenEvaluator.TryEvaluate(
                program,
                (string domainId, string key, out double resolved) => TryResolveVar(context, domainId, key, out resolved),
                DefaultNumericRpnFunctionRegistry.Instance,
                out value);
        }

        private static bool TryResolveVar(ActionContext context, string domainId, string key, out double value)
        {
            value = 0.0;
            if (string.IsNullOrEmpty(domainId) || string.IsNullOrEmpty(key))
                return false;

            if (context?.Variables != null && context.Variables.TryGet(domainId, key, out var resolved))
            {
                value = resolved;
                return true;
            }

            return false;
        }

        private static double ApplyNumericValuePolicy(in NumericValueRef ref_, double value)
        {
            if (ref_.HasScale) value *= ref_.Scale;
            if (ref_.Offset != 0d) value += ref_.Offset;
            if (ref_.HasMin && value < ref_.MinValue) value = ref_.MinValue;
            if (ref_.HasMax && value > ref_.MaxValue) value = ref_.MaxValue;
            return value;
        }

        private static string Describe(in NumericValueRef ref_)
        {
            var source = ref_.Kind switch
            {
                ENumericValueRefKind.Const => $"Const({ref_.ConstValue})",
                ENumericValueRefKind.Blackboard => $"Blackboard(boardId={ref_.BoardId}, keyId={ref_.KeyId})",
                ENumericValueRefKind.PayloadField => $"PayloadField(fieldId={ref_.FieldId})",
                ENumericValueRefKind.Var => $"Var(domainId='{ref_.DomainId}', key='{ref_.Key}')",
                ENumericValueRefKind.Expr => "Expr(" + ref_.ExprText + ")",
                _ => "Unsupported(" + ref_.Kind + ")"
            };

            if (!string.IsNullOrEmpty(ref_.Label)) source += $", label='{ref_.Label}'";
            if (!string.IsNullOrEmpty(ref_.Scope)) source += $", scope='{ref_.Scope}'";
            if (ref_.Required) source += ", required=true";
            if (ref_.HasFallback) source += $", fallback={ref_.FallbackValue}";
            if (ref_.HasScale) source += $", scale={ref_.Scale}";
            if (ref_.Offset != 0d) source += $", offset={ref_.Offset}";
            if (ref_.HasMin) source += $", min={ref_.MinValue}";
            if (ref_.HasMax) source += $", max={ref_.MaxValue}";
            return source;
        }
    }
}

using System;
using System.Buffers;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Variables.Numeric;
using AbilityKit.Triggering.Variables.Numeric.Expression;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public static class RpnNumericExprEval
    {
        public static double Eval<TArgs, TCtx>(RpnNumericNode[] nodes, in TArgs args, in ExecCtx<TCtx> ctx)
        {
            if (nodes == null || nodes.Length == 0) return 0d;

            if (nodes.Length <= 64)
            {
                Span<double> stack = stackalloc double[64];
                var sp = 0;
                EvalNodes(nodes, in args, in ctx, ref stack, ref sp);
                if (sp != 1) throw new InvalidOperationException("Invalid RPN stack depth: " + sp);
                return stack[0];
            }

            var rented = ArrayPool<double>.Shared.Rent(nodes.Length);
            try
            {
                Span<double> stack = rented;
                var sp = 0;
                EvalNodes(nodes, in args, in ctx, ref stack, ref sp);
                if (sp != 1) throw new InvalidOperationException("Invalid RPN stack depth: " + sp);
                return stack[0];
            }
            finally
            {
                ArrayPool<double>.Shared.Return(rented);
            }
        }

        private static void EvalNodes<TArgs, TCtx>(RpnNumericNode[] nodes, in TArgs args, in ExecCtx<TCtx> ctx, ref Span<double> stack, ref int sp)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                var n = nodes[i];
                switch (n.Kind)
                {
                    case ERpnNumericNodeKind.Push:
                        stack[sp++] = ResolveNumericValueRef(in args, in n.Value, in ctx);
                        break;
                    case ERpnNumericNodeKind.Add:
                    {
                        if (sp < 2) throw new InvalidOperationException("Invalid RPN: ADD stack underflow");
                        var b = stack[--sp];
                        var a = stack[--sp];
                        stack[sp++] = a + b;
                        break;
                    }
                    case ERpnNumericNodeKind.Sub:
                    {
                        if (sp < 2) throw new InvalidOperationException("Invalid RPN: SUB stack underflow");
                        var b = stack[--sp];
                        var a = stack[--sp];
                        stack[sp++] = a - b;
                        break;
                    }
                    case ERpnNumericNodeKind.Mul:
                    {
                        if (sp < 2) throw new InvalidOperationException("Invalid RPN: MUL stack underflow");
                        var b = stack[--sp];
                        var a = stack[--sp];
                        stack[sp++] = a * b;
                        break;
                    }
                    case ERpnNumericNodeKind.Div:
                    {
                        if (sp < 2) throw new InvalidOperationException("Invalid RPN: DIV stack underflow");
                        var b = stack[--sp];
                        var a = stack[--sp];
                        if (b == 0d) throw new DivideByZeroException();
                        stack[sp++] = a / b;
                        break;
                    }
                    default:
                        throw new InvalidOperationException("Unsupported RPN node kind: " + n.Kind);
                }
            }
        }

        private static double ResolveNumericValueRef<TArgs, TCtx>(in TArgs args, in NumericValueRef valueRef, in ExecCtx<TCtx> ctx)
        {
            if (ActionSchemaRegistry.TryResolveNumericRef(in valueRef, in args, in ctx, out var value))
                return value;

            throw new InvalidOperationException("NumericValueRef resolve failed. kind=" + valueRef.Kind);
        }
    }
}

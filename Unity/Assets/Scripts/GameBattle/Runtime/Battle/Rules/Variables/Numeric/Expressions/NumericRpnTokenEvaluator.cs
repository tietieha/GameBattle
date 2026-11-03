using System;
using System.Buffers;

namespace AbilityKit.Triggering.Variables.Numeric.Expression
{
    public delegate bool NumericRpnVariableResolver(string domainId, string key, out double value);

    public static class NumericRpnTokenEvaluator
    {
        public static bool TryEvaluate(
            NumericRpnProgram program,
            NumericRpnVariableResolver variableResolver,
            INumericRpnFunctionRegistry functionRegistry,
            out double value)
        {
            value = 0d;
            if (program == null)
                return false;

            var tokens = program.Tokens;
            if (tokens == null || tokens.Length == 0)
                return false;

            if (tokens.Length <= 64)
            {
                Span<double> stack = stackalloc double[64];
                var sp = 0;
                return EvalTokens(tokens, variableResolver, functionRegistry, ref stack, ref sp, out value);
            }

            var rented = ArrayPool<double>.Shared.Rent(tokens.Length);
            try
            {
                Span<double> stack = rented;
                var sp = 0;
                return EvalTokens(tokens, variableResolver, functionRegistry, ref stack, ref sp, out value);
            }
            finally
            {
                ArrayPool<double>.Shared.Return(rented);
            }
        }

        private static bool EvalTokens(
            NumericRpnToken[] tokens,
            NumericRpnVariableResolver variableResolver,
            INumericRpnFunctionRegistry functionRegistry,
            ref Span<double> stack,
            ref int sp,
            out double value)
        {
            value = 0d;

            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                switch (token.Kind)
                {
                    case NumericRpnTokenKind.Number:
                        stack[sp++] = token.Number;
                        break;

                    case NumericRpnTokenKind.Var:
                        if (variableResolver == null || string.IsNullOrEmpty(token.DomainId) || string.IsNullOrEmpty(token.Key))
                            return false;
                        if (!variableResolver(token.DomainId, token.Key, out var varValue))
                            return false;
                        stack[sp++] = varValue;
                        break;

                    case NumericRpnTokenKind.Add:
                    case NumericRpnTokenKind.Sub:
                    case NumericRpnTokenKind.Mul:
                    case NumericRpnTokenKind.Div:
                        if (sp < 2)
                            return false;
                        var b = stack[--sp];
                        var a = stack[--sp];
                        if (token.Kind == NumericRpnTokenKind.Add) stack[sp++] = a + b;
                        else if (token.Kind == NumericRpnTokenKind.Sub) stack[sp++] = a - b;
                        else if (token.Kind == NumericRpnTokenKind.Mul) stack[sp++] = a * b;
                        else
                        {
                            if (b == 0d)
                                return false;
                            stack[sp++] = a / b;
                        }
                        break;

                    case NumericRpnTokenKind.Func:
                    {
                        if (sp < token.FuncArgCount)
                            return false;

                        var registry = functionRegistry ?? DefaultNumericRpnFunctionRegistry.Instance;
                        if (!registry.TryGet(token.FuncName, out var function) || function == null)
                            return false;
                        if (function.ArgCount != token.FuncArgCount)
                            return false;

                        var args = new double[token.FuncArgCount];
                        for (int ai = token.FuncArgCount - 1; ai >= 0; ai--)
                        {
                            args[ai] = stack[--sp];
                        }

                        if (!function.TryInvoke(args, out var functionValue))
                            return false;
                        stack[sp++] = functionValue;
                        break;
                    }

                    default:
                        return false;
                }
            }

            if (sp != 1)
                return false;

            value = stack[0];
            return true;
        }
    }
}

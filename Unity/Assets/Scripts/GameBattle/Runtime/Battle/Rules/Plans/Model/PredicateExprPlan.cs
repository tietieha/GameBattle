using System;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime.Config;

namespace AbilityKit.Triggering.Runtime.Plan
{
    using ECompareOp = AbilityKit.Triggering.Runtime.Config.ECompareOp;

    public enum EPredicateKind : byte
    {
        None = 0,
        Function = 1,
        Expr = 2,
    }

    public enum EBoolExprNodeKind : byte
    {
        Const = 0,
        Not = 1,
        And = 2,
        Or = 3,
        CompareNumeric = 4,
        Function = 5,
    }

    public readonly struct BoolExprNode
    {
        public readonly EBoolExprNodeKind Kind;
        public readonly bool ConstValue;
        public readonly ECompareOp CompareOp;
        public readonly NumericValueRef Left;
        public readonly NumericValueRef Right;
        public readonly FunctionId FunctionId;
        public readonly byte FunctionArity;

        private BoolExprNode(EBoolExprNodeKind kind, bool constValue, ECompareOp compareOp, NumericValueRef left, NumericValueRef right, FunctionId functionId, byte functionArity)
        {
            Kind = kind;
            ConstValue = constValue;
            CompareOp = compareOp;
            Left = left;
            Right = right;
            FunctionId = functionId;
            FunctionArity = functionArity;
        }

        public static BoolExprNode Const(bool value) => new BoolExprNode(EBoolExprNodeKind.Const, value, default, default, default, default, default);
        public static BoolExprNode Not() => new BoolExprNode(EBoolExprNodeKind.Not, default, default, default, default, default, default);
        public static BoolExprNode And() => new BoolExprNode(EBoolExprNodeKind.And, default, default, default, default, default, default);
        public static BoolExprNode Or() => new BoolExprNode(EBoolExprNodeKind.Or, default, default, default, default, default, default);
        public static BoolExprNode Compare(ECompareOp op, NumericValueRef left, NumericValueRef right) => new BoolExprNode(EBoolExprNodeKind.CompareNumeric, default, op, left, right, default, default);
        public static BoolExprNode Function(FunctionId functionId, byte arity, NumericValueRef arg0 = default, NumericValueRef arg1 = default) => new BoolExprNode(EBoolExprNodeKind.Function, default, default, arg0, arg1, functionId, arity);
    }

    public readonly struct PredicateExprPlan
    {
        public readonly BoolExprNode[] Nodes;

        public PredicateExprPlan(BoolExprNode[] nodes)
        {
            Nodes = nodes;
        }
    }
}

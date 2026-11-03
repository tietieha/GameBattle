using System;

namespace AbilityKit.Demo.Moba.Share.Config
{
    [Serializable]
    public enum ENumericRefKind : byte
    {
        Const = 0,
        Blackboard = 1,
        PayloadField = 2,
        Var = 3,
        Expr = 4,

        SkillLevelCost = 20,
        SkillLevelCooldownMs = 21,
        ActorAttribute = 30,
        ActorResourceCurrent = 31,
        ActorResourceMax = 32,
        ActorResourcePercent = 33,
    }

    [Serializable]
    public enum ECompareOp : byte
    {
        Equal = 0,
        NotEqual = 1,
        GreaterThan = 2,
        GreaterThanOrEqual = 3,
        LessThan = 4,
        LessThanOrEqual = 5,
    }

    [Serializable]
    public enum NumericRefActor : byte
    {
        Caster = 0,
        Target = 1,
    }

    [Serializable]
    public class NumericRefDTO
    {
        public ENumericRefKind Kind;
        public double ConstValue;
        public int BoardId;
        public int KeyId;
        public int FieldId;
        public string DomainId;
        public string Key;
        public string ExprText;

        public int Actor;
        public int AttributeType;
        public int ResourceType;
        public double Coefficient = 1d;
        public double Add;

        public static NumericRefDTO Const(double value) => new NumericRefDTO { Kind = ENumericRefKind.Const, ConstValue = value };
        public static NumericRefDTO Blackboard(int boardId, int keyId) => new NumericRefDTO { Kind = ENumericRefKind.Blackboard, BoardId = boardId, KeyId = keyId };
        public static NumericRefDTO PayloadField(int fieldId) => new NumericRefDTO { Kind = ENumericRefKind.PayloadField, FieldId = fieldId };
        public static NumericRefDTO Var(string domainId, string key) => new NumericRefDTO { Kind = ENumericRefKind.Var, DomainId = domainId, Key = key };
        public static NumericRefDTO Expr(string exprText) => new NumericRefDTO { Kind = ENumericRefKind.Expr, ExprText = exprText };
        public static NumericRefDTO SkillCost() => new NumericRefDTO { Kind = ENumericRefKind.SkillLevelCost };
        public static NumericRefDTO SkillCooldownMs() => new NumericRefDTO { Kind = ENumericRefKind.SkillLevelCooldownMs };
        public static NumericRefDTO ActorAttribute(int attributeType, int actor = (int)NumericRefActor.Caster, double coefficient = 1d, double add = 0d)
            => new NumericRefDTO { Kind = ENumericRefKind.ActorAttribute, Actor = actor, AttributeType = attributeType, Coefficient = coefficient, Add = add };
    }
}

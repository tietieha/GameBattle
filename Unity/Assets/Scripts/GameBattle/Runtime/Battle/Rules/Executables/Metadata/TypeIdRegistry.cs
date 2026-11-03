using System;

namespace AbilityKit.Triggering.Runtime.Executable
{
    public static class TypeIdRegistry
    {
        public static class Executable
        {
            public const int Sequence = 1001;
            public const int Selector = 1002;
            public const int Parallel = 1003;
            public const int If = 1004;
            public const int IfElse = 1005;
            public const int Switch = 1006;
            public const int Repeat = 1007;
            public const int Until = 1008;
            public const int Timed = 1009;
            public const int Periodic = 1010;
            public const int External = 1011;
            public const int ActionCall = 1012;
            public const int Delay = 1013;
            public const int RandomSelector = 1014;
        }

        public static class Condition
        {
            public const int Multi = 2001;
            public const int Not = 2002;
            public const int And = 2003;
            public const int Or = 2004;
            public const int NumericCompare = 2005;
            public const int PayloadCompare = 2006;
            public const int Const = 2008;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExecutableTypeIdAttribute : Attribute
    {
        public int TypeId { get; }
        public string TypeName { get; }
        public bool IsComposite { get; }
        public bool IsScheduled { get; }
        public float DefaultDurationMs { get; }
        public float DefaultPeriodMs { get; }

        public ExecutableTypeIdAttribute(int typeId, string typeName, bool isComposite = false, bool isScheduled = false, float defaultDurationMs = 0f, float defaultPeriodMs = 0f)
        {
            TypeId = typeId;
            TypeName = typeName;
            IsComposite = isComposite;
            IsScheduled = isScheduled;
            DefaultDurationMs = defaultDurationMs;
            DefaultPeriodMs = defaultPeriodMs;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ConditionTypeIdAttribute : Attribute
    {
        public int TypeId { get; }
        public string TypeName { get; }

        public ConditionTypeIdAttribute(int typeId, string typeName)
        {
            TypeId = typeId;
            TypeName = typeName;
        }
    }

}

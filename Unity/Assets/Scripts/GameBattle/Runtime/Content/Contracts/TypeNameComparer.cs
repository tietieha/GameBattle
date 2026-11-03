using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.Config
{
    /// <summary>
    /// 基于 Type.FullName 的 Dictionary 键相等比较器。
    /// 用于规避 IL2CPP/AOT 下同一类型对应的两个 Type 对象可能既非引用相等也非 Equals 相等的问题
    /// （它们可能有相同的 GetHashCode，但因 AOT 类型重建导致 Equals 返回 false）。
    /// 使用 FullName 可在程序集加载边界之间保持一致的字符串身份标识。
    /// </summary>
    public sealed class TypeNameComparer : IEqualityComparer<Type>
    {
        public static readonly TypeNameComparer Instance = new TypeNameComparer();

        private TypeNameComparer() { }

        public bool Equals(Type x, Type y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.FullName == y.FullName;
        }

        public int GetHashCode(Type obj) => obj != null ? obj.FullName.GetHashCode() : 0;
    }
}

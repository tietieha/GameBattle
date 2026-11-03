using System;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线阶段执行结果标识。
    /// </summary>
    public readonly struct AbilityPipelinePhaseResult : IEquatable<AbilityPipelinePhaseResult>
    {
        /// <summary>
        /// 原始结果值。
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// 创建阶段结果标识。
        /// </summary>
        public AbilityPipelinePhaseResult(string value)
        {
            Value = value ?? string.Empty;
        }

        /// <summary>
        /// 成功结果名称。
        /// </summary>
        public const string SuccessName = "Success";
        
        /// <summary>
        /// 成功结果。
        /// </summary>
        public static AbilityPipelinePhaseResult Success => new AbilityPipelinePhaseResult(SuccessName);

        /// <summary>
        /// 是否为有效结果标识。
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(Value);

        /// <summary>
        /// 返回字符串形式。
        /// </summary>
        public override string ToString() => Value ?? string.Empty;

        /// <summary>
        /// 比较两个结果标识是否相等。
        /// </summary>
        public bool Equals(AbilityPipelinePhaseResult other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is AbilityPipelinePhaseResult other && Equals(other);
        }

        /// <summary>
        /// 获取哈希值。
        /// </summary>
        public override int GetHashCode()
        {
            return Value != null ? StringComparer.Ordinal.GetHashCode(Value) : 0;
        }

        /// <summary>
        /// 判断两个结果标识是否相等。
        /// </summary>
        public static bool operator ==(AbilityPipelinePhaseResult a, AbilityPipelinePhaseResult b) => a.Equals(b);

        /// <summary>
        /// 判断两个结果标识是否不相等。
        /// </summary>
        public static bool operator !=(AbilityPipelinePhaseResult a, AbilityPipelinePhaseResult b) => !a.Equals(b);
    }
}

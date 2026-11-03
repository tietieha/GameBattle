using System;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 能力管线阶段 ID。
    /// </summary>
    public readonly struct AbilityPipelinePhaseId
    {
        /// <summary>
        /// 原始 ID 值。
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// 创建阶段 ID。
        /// </summary>
        public AbilityPipelinePhaseId(string value)
        {
            Value = value ?? string.Empty;
        }

        /// <summary>
        /// 是否为有效 ID。
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(Value);

        /// <summary>
        /// 返回阶段 ID 的字符串形式。
        /// </summary>
        public override string ToString() => Value ?? string.Empty;

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is AbilityPipelinePhaseId other && Equals(other);
        }

        /// <summary>
        /// 比较两个阶段 ID 是否相等。
        /// </summary>
        public bool Equals(AbilityPipelinePhaseId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        /// <summary>
        /// 获取哈希值。
        /// </summary>
        public override int GetHashCode()
        {
            return Value != null ? StringComparer.Ordinal.GetHashCode(Value) : 0;
        }

        /// <summary>
        /// 判断两个阶段 ID 是否相等。
        /// </summary>
        public static bool operator ==(AbilityPipelinePhaseId a, AbilityPipelinePhaseId b) => a.Equals(b);

        /// <summary>
        /// 判断两个阶段 ID 是否不相等。
        /// </summary>
        public static bool operator !=(AbilityPipelinePhaseId a, AbilityPipelinePhaseId b) => !a.Equals(b);
    }
}

using System;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public enum ENumericValueRefKind : byte
    {
        Const = 0,
        Blackboard = 1,
        PayloadField = 2,
        Var = 3,
        Expr = 4,
    }

    public readonly struct NumericValueRef : IEquatable<NumericValueRef>
    {
        public readonly ENumericValueRefKind Kind;
        public readonly double ConstValue;
        public readonly int BoardId;
        public readonly int KeyId;
        public readonly int FieldId;
        public readonly string DomainId;
        public readonly string Key;
        public readonly string ExprText;
        public readonly bool Required;
        public readonly bool HasFallback;
        public readonly double FallbackValue;
        public readonly bool HasMin;
        public readonly double MinValue;
        public readonly bool HasMax;
        public readonly double MaxValue;
        public readonly bool HasScale;
        public readonly double Scale;
        public readonly double Offset;
        public readonly string Label;
        public readonly string Scope;

        private NumericValueRef(
            ENumericValueRefKind kind,
            double constValue,
            int boardId,
            int keyId,
            int fieldId,
            string domainId,
            string key,
            string exprText,
            bool required,
            bool hasFallback,
            double fallbackValue,
            bool hasMin,
            double minValue,
            bool hasMax,
            double maxValue,
            bool hasScale,
            double scale,
            double offset,
            string label,
            string scope)
        {
            Kind = kind;
            ConstValue = constValue;
            BoardId = boardId;
            KeyId = keyId;
            FieldId = fieldId;
            DomainId = domainId;
            Key = key;
            ExprText = exprText;
            Required = required;
            HasFallback = hasFallback;
            FallbackValue = fallbackValue;
            HasMin = hasMin;
            MinValue = minValue;
            HasMax = hasMax;
            MaxValue = maxValue;
            HasScale = hasScale;
            Scale = scale;
            Offset = offset;
            Label = label;
            Scope = scope;
        }

        public static NumericValueRef Const(double value) => new NumericValueRef(ENumericValueRefKind.Const, value, 0, 0, 0, null, null, null, false, false, 0d, false, 0d, false, 0d, false, 1d, 0d, null, null);
        public static NumericValueRef Blackboard(int boardId, int keyId) => new NumericValueRef(ENumericValueRefKind.Blackboard, 0d, boardId, keyId, 0, null, null, null, false, false, 0d, false, 0d, false, 0d, false, 1d, 0d, null, null);
        public static NumericValueRef PayloadField(int fieldId) => new NumericValueRef(ENumericValueRefKind.PayloadField, 0d, 0, 0, fieldId, null, null, null, false, false, 0d, false, 0d, false, 0d, false, 1d, 0d, null, null);
        public static NumericValueRef Var(string domainId, string key) => new NumericValueRef(ENumericValueRefKind.Var, 0d, 0, 0, 0, domainId, key, null, false, false, 0d, false, 0d, false, 0d, false, 1d, 0d, null, null);
        public static NumericValueRef Expr(string exprText) => new NumericValueRef(ENumericValueRefKind.Expr, 0d, 0, 0, 0, null, null, exprText, false, false, 0d, false, 0d, false, 0d, false, 1d, 0d, null, null);

        public NumericValueRef AsRequired(bool required = true) => Copy(required: required);
        public NumericValueRef WithFallback(double value) => Copy(hasFallback: true, fallbackValue: value);
        public NumericValueRef WithoutFallback() => Copy(hasFallback: false, fallbackValue: 0d);
        public NumericValueRef WithMin(double minValue) => Copy(hasMin: true, minValue: minValue);
        public NumericValueRef WithMax(double maxValue) => Copy(hasMax: true, maxValue: maxValue);
        public NumericValueRef WithClamp(double minValue, double maxValue) => Copy(hasMin: true, minValue: minValue, hasMax: true, maxValue: maxValue);
        public NumericValueRef WithScale(double scale) => Copy(hasScale: true, scale: scale);
        public NumericValueRef WithOffset(double offset) => Copy(offset: offset);
        public NumericValueRef WithLabel(string label) => Copy(label: label);
        public NumericValueRef WithScope(string scope) => Copy(scope: scope);

        private NumericValueRef Copy(
            bool? required = null,
            bool? hasFallback = null,
            double? fallbackValue = null,
            bool? hasMin = null,
            double? minValue = null,
            bool? hasMax = null,
            double? maxValue = null,
            bool? hasScale = null,
            double? scale = null,
            double? offset = null,
            string label = null,
            string scope = null)
        {
            return new NumericValueRef(
                Kind,
                ConstValue,
                BoardId,
                KeyId,
                FieldId,
                DomainId,
                Key,
                ExprText,
                required ?? Required,
                hasFallback ?? HasFallback,
                fallbackValue ?? FallbackValue,
                hasMin ?? HasMin,
                minValue ?? MinValue,
                hasMax ?? HasMax,
                maxValue ?? MaxValue,
                hasScale ?? HasScale,
                scale ?? Scale,
                offset ?? Offset,
                label ?? Label,
                scope ?? Scope);
        }

        public bool Equals(NumericValueRef other)
        {
            return Kind == other.Kind
                   && ConstValue == other.ConstValue
                   && BoardId == other.BoardId
                   && KeyId == other.KeyId
                   && FieldId == other.FieldId
                   && Required == other.Required
                   && HasFallback == other.HasFallback
                   && FallbackValue == other.FallbackValue
                   && HasMin == other.HasMin
                   && MinValue == other.MinValue
                   && HasMax == other.HasMax
                   && MaxValue == other.MaxValue
                   && HasScale == other.HasScale
                   && Scale == other.Scale
                   && Offset == other.Offset
                   && string.Equals(DomainId, other.DomainId, StringComparison.Ordinal)
                   && string.Equals(Key, other.Key, StringComparison.Ordinal)
                   && string.Equals(ExprText, other.ExprText, StringComparison.Ordinal)
                   && string.Equals(Label, other.Label, StringComparison.Ordinal)
                   && string.Equals(Scope, other.Scope, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => obj is NumericValueRef other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                var h = (int)Kind;
                h = (h * 397) ^ ConstValue.GetHashCode();
                h = (h * 397) ^ BoardId;
                h = (h * 397) ^ KeyId;
                h = (h * 397) ^ FieldId;
                h = (h * 397) ^ Required.GetHashCode();
                h = (h * 397) ^ HasFallback.GetHashCode();
                h = (h * 397) ^ FallbackValue.GetHashCode();
                h = (h * 397) ^ HasMin.GetHashCode();
                h = (h * 397) ^ MinValue.GetHashCode();
                h = (h * 397) ^ HasMax.GetHashCode();
                h = (h * 397) ^ MaxValue.GetHashCode();
                h = (h * 397) ^ HasScale.GetHashCode();
                h = (h * 397) ^ Scale.GetHashCode();
                h = (h * 397) ^ Offset.GetHashCode();
                h = (h * 397) ^ (DomainId != null ? DomainId.GetHashCode() : 0);
                h = (h * 397) ^ (Key != null ? Key.GetHashCode() : 0);
                h = (h * 397) ^ (ExprText != null ? ExprText.GetHashCode() : 0);
                h = (h * 397) ^ (Label != null ? Label.GetHashCode() : 0);
                h = (h * 397) ^ (Scope != null ? Scope.GetHashCode() : 0);
                return h;
            }
        }
    }
}

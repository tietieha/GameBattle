using System;
using AbilityKit.Triggering.Eventing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    internal static class TriggerPlanSourceJsonUtility
    {
        public static string ReadString(JObject obj, params string[] aliases)
        {
            if (obj == null || aliases == null) return null;
            for (int i = 0; i < aliases.Length; i++)
            {
                if (obj.TryGetValue(aliases[i], StringComparison.OrdinalIgnoreCase, out var token))
                {
                    return token?.ToString();
                }
            }
            return null;
        }

        public static int ReadInt(JObject obj, int defaultValue, params string[] aliases)
        {
            if (obj == null || aliases == null) return defaultValue;
            for (int i = 0; i < aliases.Length; i++)
            {
                if (obj.TryGetValue(aliases[i], StringComparison.OrdinalIgnoreCase, out var token))
                {
                    return token.Value<int?>() ?? defaultValue;
                }
            }
            return defaultValue;
        }

        public static float ReadFloat(JObject obj, float defaultValue, params string[] aliases)
        {
            if (obj == null || aliases == null) return defaultValue;
            for (int i = 0; i < aliases.Length; i++)
            {
                if (obj.TryGetValue(aliases[i], StringComparison.OrdinalIgnoreCase, out var token))
                {
                    return token.Value<float?>() ?? defaultValue;
                }
            }
            return defaultValue;
        }

        public static bool IsKind(string kind, params string[] values)
        {
            if (string.IsNullOrEmpty(kind) || values == null) return false;
            for (var i = 0; i < values.Length; i++)
            {
                if (string.Equals(kind, values[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string NormalizeCompareOp(string op)
        {
            if (string.IsNullOrEmpty(op)) return "Equal";
            switch (op.Trim().ToLowerInvariant())
            {
                case "eq":
                case "=":
                case "==":
                case "equal":
                    return "Equal";
                case "ne":
                case "neq":
                case "!=":
                case "not_equal":
                case "not-equal":
                case "notequal":
                    return "NotEqual";
                case "gt":
                case ">":
                case "greater_than":
                case "greater-than":
                case "greaterthan":
                    return "GreaterThan";
                case "ge":
                case "gte":
                case ">=":
                case "greater_than_or_equal":
                case "greater-than-or-equal":
                case "greaterthanorequal":
                    return "GreaterThanOrEqual";
                case "lt":
                case "<":
                case "less_than":
                case "less-than":
                case "lessthan":
                    return "LessThan";
                case "le":
                case "lte":
                case "<=":
                case "less_than_or_equal":
                case "less-than-or-equal":
                case "lessthanorequal":
                    return "LessThanOrEqual";
                default:
                    return op;
            }
        }
    }

    internal static class TriggerPlanSourceValueWriter
    {
        public static void WriteConstValue(JsonTextWriter writer, double value)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Kind");
            writer.WriteValue("Const");
            writer.WritePropertyName("ConstValue");
            writer.WriteValue(value);
            writer.WriteEndObject();
        }

        public static void WritePayloadFieldValue(JsonTextWriter writer, string payloadField)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Kind");
            writer.WriteValue("PayloadField");
            writer.WritePropertyName("FieldId");
            writer.WriteValue(string.IsNullOrEmpty(payloadField) ? 0 : StableStringId.Get("payload:" + payloadField));
            writer.WriteEndObject();
        }

        public static void WriteVarValue(JsonTextWriter writer, string domain, string key)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Kind");
            writer.WriteValue("Var");
            writer.WritePropertyName("DomainId");
            writer.WriteValue(domain ?? string.Empty);
            writer.WritePropertyName("Key");
            writer.WriteValue(key ?? string.Empty);
            writer.WriteEndObject();
        }

        public static void WriteExprValue(JsonTextWriter writer, string expr)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Kind");
            writer.WriteValue("Expr");
            writer.WritePropertyName("ExprText");
            writer.WriteValue(expr ?? string.Empty);
            writer.WriteEndObject();
        }
    }
}

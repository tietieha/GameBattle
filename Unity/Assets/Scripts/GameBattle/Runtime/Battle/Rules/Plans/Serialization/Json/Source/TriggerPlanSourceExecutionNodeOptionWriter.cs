using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// Writes source-format execution-node options and extracts node-local predicates.
    /// </summary>
    internal sealed class TriggerPlanSourceExecutionNodeOptionWriter
    {
        private readonly TriggerPlanSourceConditionWriter _conditionWriter;
        private readonly TriggerPlanSourceConditionWriter.ParamValueWriter _writeParamValue;

        public TriggerPlanSourceExecutionNodeOptionWriter(
            TriggerPlanSourceConditionWriter conditionWriter,
            TriggerPlanSourceConditionWriter.ParamValueWriter writeParamValue)
        {
            _conditionWriter = conditionWriter ?? throw new ArgumentNullException(nameof(conditionWriter));
            _writeParamValue = writeParamValue ?? throw new ArgumentNullException(nameof(writeParamValue));
        }

        public void WriteOptions(JsonTextWriter writer, JObject node, string normalizedKind, Dictionary<string, JToken> conditionCatalog)
        {
            if (string.Equals(normalizedKind, "Repeat", StringComparison.OrdinalIgnoreCase))
            {
                writer.WritePropertyName("Count");
                writer.WriteValue(ReadPositiveInt(node, "count", "times", "repeatCount", "repeat_count"));
            }

            if (string.Equals(normalizedKind, "Until", StringComparison.OrdinalIgnoreCase))
            {
                writer.WritePropertyName("MaxIterations");
                writer.WriteValue(ReadPositiveInt(node, "maxIterations", "max_iterations", "limit", "count"));

                if (TryGetUntilCondition(node, conditionCatalog, out var untilConditions))
                {
                    writer.WritePropertyName("UntilCondition");
                    _conditionWriter.WritePredicate(writer, untilConditions, _writeParamValue);
                }
            }

            if (string.Equals(normalizedKind, "Fail", StringComparison.OrdinalIgnoreCase))
            {
                var reason = node["reason"]?.ToString();
                if (!string.IsNullOrEmpty(reason))
                {
                    writer.WritePropertyName("Reason");
                    writer.WriteValue(reason);
                }
            }
        }

        public bool TryGetNodeCondition(JObject node, Dictionary<string, JToken> conditionCatalog, out List<JObject> conditions)
        {
            conditions = null;
            if (node == null) return false;

            var token = node["condition"] ?? node["conditions"] ?? node["when"];
            if (!TryBuildConditionList(token, out conditions))
            {
                return false;
            }

            conditions = TriggerPlanSourceFragmentResolver.ResolveConditionList(conditions, null, null, conditionCatalog, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            return conditions.Count > 0;
        }

        private static int ReadPositiveInt(JObject node, params string[] aliases)
        {
            if (node == null || aliases == null) return 1;
            for (int i = 0; i < aliases.Length; i++)
            {
                if (node.TryGetValue(aliases[i], StringComparison.OrdinalIgnoreCase, out var token))
                {
                    var value = token.Value<int?>() ?? 1;
                    return value > 0 ? value : 1;
                }
            }

            return 1;
        }

        private static bool TryGetUntilCondition(JObject node, Dictionary<string, JToken> conditionCatalog, out List<JObject> conditions)
        {
            conditions = null;
            if (node == null) return false;

            var token = node["until"] ?? node["untilCondition"] ?? node["until_condition"];
            if (!TryBuildConditionList(token, out conditions))
            {
                return false;
            }

            conditions = TriggerPlanSourceFragmentResolver.ResolveConditionList(conditions, null, null, conditionCatalog, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            return conditions.Count > 0;
        }

        private static bool TryBuildConditionList(JToken token, out List<JObject> conditions)
        {
            conditions = null;
            if (token == null || token.Type == JTokenType.Null)
                return false;

            conditions = new List<JObject>();
            if (token is JArray arr)
            {
                foreach (var item in arr)
                {
                    if (item is JObject obj)
                    {
                        conditions.Add(obj);
                    }
                }
            }
            else if (token is JObject obj)
            {
                conditions.Add(obj);
            }

            return conditions.Count > 0;
        }
    }
}

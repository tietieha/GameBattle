using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.Config.Plans;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// Writes runtime execution-node JSON from source-format execution-node objects.
    /// </summary>
    internal sealed class TriggerPlanSourceExecutionNodeWriter
    {
        private readonly TriggerPlanSourceConditionWriter _conditionWriter;
        private readonly TriggerPlanSourceActionWriter _actionWriter;
        private readonly TriggerPlanSourceBehaviorResolver _behaviorResolver;
        private readonly TriggerPlanSourceExecutionNodeOptionWriter _executionNodeOptionWriter;
        private readonly TriggerPlanSourceExecutionNodeShape _executionNodeShape;

        public TriggerPlanSourceExecutionNodeWriter(
            TriggerPlanSourceConditionWriter conditionWriter,
            TriggerPlanSourceActionWriter actionWriter,
            TriggerPlanSourceBehaviorResolver behaviorResolver,
            TriggerPlanSourceExecutionNodeOptionWriter executionNodeOptionWriter,
            TriggerPlanSourceExecutionNodeShape executionNodeShape)
        {
            _conditionWriter = conditionWriter ?? throw new ArgumentNullException(nameof(conditionWriter));
            _actionWriter = actionWriter ?? throw new ArgumentNullException(nameof(actionWriter));
            _behaviorResolver = behaviorResolver ?? throw new ArgumentNullException(nameof(behaviorResolver));
            _executionNodeOptionWriter = executionNodeOptionWriter ?? throw new ArgumentNullException(nameof(executionNodeOptionWriter));
            _executionNodeShape = executionNodeShape ?? throw new ArgumentNullException(nameof(executionNodeShape));
        }

        public void Write(
            JsonTextWriter writer,
            JObject node,
            Dictionary<string, ActionSourceDefJson> actionSchemas,
            Dictionary<string, JToken> conditionCatalog,
            Dictionary<string, JToken> actionCatalog,
            Dictionary<string, JObject> behaviorCatalog,
            string sourcePath,
            string sourceKind,
            string sourceId,
            HashSet<string> behaviorResolving)
        {
            var resolvedBehaviorId = default(string);
            node = _behaviorResolver.Resolve(node, behaviorCatalog, behaviorResolving, ref sourceKind, ref sourceId, ref sourcePath, out resolvedBehaviorId);

            try
            {
                writer.WriteStartObject();

                if (node == null)
                {
                    writer.WritePropertyName("Kind");
                    writer.WriteValue("Sequence");
                    writer.WritePropertyName("Children");
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    return;
                }

                var type = (node["kind"] ?? node["type"])?.ToString();
                var normalizedKind = _executionNodeShape.NormalizeKind(type, node);

                writer.WritePropertyName("Kind");
                writer.WriteValue(normalizedKind);
                _executionNodeShape.WriteSource(writer, sourceKind, sourceId, sourcePath);

                if (_executionNodeOptionWriter.TryGetNodeCondition(node, conditionCatalog, out var conditionNodes))
                {
                    writer.WritePropertyName("Condition");
                    _conditionWriter.WritePredicate(writer, conditionNodes, _actionWriter.WriteParamValue);
                }

                var weight = node["weight"]?.Value<float?>();
                if (weight.HasValue)
                {
                    writer.WritePropertyName("Weight");
                    writer.WriteValue(weight.Value);
                }

                _executionNodeOptionWriter.WriteOptions(writer, node, normalizedKind, conditionCatalog);

                if (string.Equals(normalizedKind, "Action", StringComparison.OrdinalIgnoreCase))
                {
                    var action = node["action"] as JObject ?? node;
                    writer.WritePropertyName("Action");
                    _actionWriter.WriteAction(writer, TriggerPlanSourceFragmentResolver.ResolveAction(action, actionCatalog, new HashSet<string>(StringComparer.OrdinalIgnoreCase)) ?? action, actionSchemas, actionCatalog, TriggerPlanSourceFragmentResolver.ResolveAction);
                    writer.WriteEndObject();
                    return;
                }

                var children = _executionNodeShape.GetChildren(node, behaviorCatalog);
                writer.WritePropertyName("Children");
                writer.WriteStartArray();
                if (children != null)
                {
                    var index = 0;
                    foreach (var child in children)
                    {
                        if (child is JObject childObj)
                        {
                            Write(writer, childObj, actionSchemas, conditionCatalog, actionCatalog, behaviorCatalog, $"{sourcePath}/children[{index}]", sourceKind, sourceId, behaviorResolving);
                        }
                        index++;
                    }
                }
                writer.WriteEndArray();

                if (string.Equals(normalizedKind, "If", StringComparison.OrdinalIgnoreCase))
                {
                    var elseChildren = _executionNodeShape.GetElseChildren(node);
                    if (elseChildren != null)
                    {
                        writer.WritePropertyName("ElseChildren");
                        writer.WriteStartArray();
                        var index = 0;
                        foreach (var child in elseChildren)
                        {
                            if (child is JObject childObj)
                            {
                                Write(writer, childObj, actionSchemas, conditionCatalog, actionCatalog, behaviorCatalog, $"{sourcePath}/else[{index}]", sourceKind, sourceId, behaviorResolving);
                            }
                            index++;
                        }
                        writer.WriteEndArray();
                    }
                }

                writer.WriteEndObject();
            }
            finally
            {
                _behaviorResolver.EndResolve(resolvedBehaviorId, behaviorResolving);
            }
        }
    }
}

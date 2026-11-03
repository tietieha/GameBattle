using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// Reads source-format execution-node shape metadata such as kind, children and source trace fields.
    /// </summary>
    internal sealed class TriggerPlanSourceExecutionNodeShape
    {
        public string NormalizeKind(string type, JObject node)
        {
            if (node != null && node["action"] is JObject)
                return "Action";

            if (string.IsNullOrEmpty(type))
                return HasCompositeChildren(node) ? "Sequence" : "Action";

            switch (type.Trim().ToLowerInvariant())
            {
                case "sequence":
                case "seq":
                    return "Sequence";
                case "selector":
                case "select":
                    return "Selector";
                case "random":
                case "random_selector":
                case "randomselector":
                    return "Random";
                case "if":
                case "ifelse":
                case "if_else":
                    return "If";
                case "parallel":
                case "all":
                    return "Parallel";
                case "repeat":
                case "loop":
                    return "Repeat";
                case "until":
                case "repeat_until":
                case "repeatuntil":
                    return "Until";
                case "invert":
                case "not":
                    return "Invert";
                case "succeed":
                case "success":
                case "always_success":
                case "alwayssuccess":
                    return "Succeed";
                case "fail":
                case "failure":
                case "always_fail":
                case "alwaysfail":
                    return "Fail";
                case "action":
                    return "Action";
                default:
                    throw new InvalidOperationException($"Unsupported execution node kind: {type}");
            }
        }

        public JArray GetChildren(JObject node, Dictionary<string, JObject> behaviorCatalog)
        {
            if (node == null) return null;

            var merged = new JArray();
            TriggerPlanSourceBehaviorResolver.AddReferenceChildren(merged, TriggerPlanSourceFragmentResolver.ReadStringList(node, "behaviorRefs"), behaviorCatalog);
            TriggerPlanSourceBehaviorResolver.AddReferenceChildren(merged, TriggerPlanSourceFragmentResolver.ReadStringList(node, "behavior_refs"), behaviorCatalog);

            var inline = GetInlineChildren(node);
            if (inline != null)
            {
                foreach (var child in inline)
                {
                    merged.Add(child);
                }
            }

            return merged.Count > 0 ? merged : null;
        }

        public JArray GetElseChildren(JObject node)
        {
            if (node == null) return null;
            if (node["elseChildren"] is JArray elseChildren) return elseChildren;
            if (node["else"] is JArray elseItems) return elseItems;
            if (node["else"] is JObject elseOne) return new JArray(elseOne);
            return null;
        }

        public void WriteSource(JsonTextWriter writer, string sourceKind, string sourceId, string sourcePath)
        {
            if (!string.IsNullOrEmpty(sourceKind))
            {
                writer.WritePropertyName("SourceKind");
                writer.WriteValue(sourceKind);
            }

            if (!string.IsNullOrEmpty(sourceId))
            {
                writer.WritePropertyName("SourceId");
                writer.WriteValue(sourceId);
            }

            if (!string.IsNullOrEmpty(sourcePath))
            {
                writer.WritePropertyName("SourcePath");
                writer.WriteValue(sourcePath);
            }
        }

        private static bool HasCompositeChildren(JObject node)
        {
            return node != null && (node["children"] is JArray || node["items"] is JArray || node["then"] is JArray || node["child"] is JObject || TriggerPlanSourceBehaviorResolver.HasReferenceList(node));
        }

        private static JArray GetInlineChildren(JObject node)
        {
            if (node == null) return null;
            if (node["children"] is JArray children) return children;
            if (node["items"] is JArray items) return items;
            if (node["then"] is JArray thenChildren) return thenChildren;
            if (node["then"] is JObject thenOne) return new JArray(thenOne);
            if (node["child"] is JObject childOne) return new JArray(childOne);
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// Resolves source-format behavior catalogs and references while writing runtime execution nodes.
    /// </summary>
    internal sealed class TriggerPlanSourceBehaviorResolver
    {
        public static Dictionary<string, JObject> BuildCatalog(Dictionary<string, BehaviorSourceDefJson> behaviors)
        {
            var catalog = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
            if (behaviors == null)
            {
                return catalog;
            }

            foreach (var kvp in behaviors)
            {
                var id = kvp.Key;
                var behavior = kvp.Value;
                var root = behavior?.behavior ?? behavior?.root;
                if (root == null)
                {
                    continue;
                }

                catalog[id] = root;
                if (!string.IsNullOrEmpty(behavior.id))
                {
                    catalog[behavior.id] = root;
                }
            }

            return catalog;
        }

        public JObject Resolve(
            JObject node,
            Dictionary<string, JObject> behaviorCatalog,
            HashSet<string> resolving,
            ref string sourceKind,
            ref string sourceId,
            ref string sourcePath,
            out string resolvedBehaviorId)
        {
            resolvedBehaviorId = null;
            var behaviorId = GetReferenceId(node);
            if (string.IsNullOrEmpty(behaviorId))
            {
                return node;
            }

            if (behaviorCatalog == null || !behaviorCatalog.TryGetValue(behaviorId, out var root) || root == null)
            {
                throw new InvalidOperationException($"Behavior reference not found: {behaviorId}");
            }

            if (resolving != null && !resolving.Add(behaviorId))
            {
                throw new InvalidOperationException($"Cyclic behavior reference detected: {behaviorId}");
            }

            resolvedBehaviorId = behaviorId;
            sourceKind = "behavior";
            sourceId = behaviorId;
            sourcePath = $"behavior:{behaviorId}";
            return (JObject)root.DeepClone();
        }

        public void EndResolve(string behaviorId, HashSet<string> resolving)
        {
            if (!string.IsNullOrEmpty(behaviorId) && resolving != null)
            {
                resolving.Remove(behaviorId);
            }
        }

        public static bool HasReferenceList(JObject node)
        {
            return node != null && (node["behaviorRefs"] is JArray || node["behavior_refs"] is JArray || node["behaviorRefs"] is JValue || node["behavior_refs"] is JValue);
        }

        public static void AddReferenceChildren(JArray children, List<string> refs, Dictionary<string, JObject> behaviorCatalog)
        {
            if (children == null || refs == null)
            {
                return;
            }

            for (int i = 0; i < refs.Count; i++)
            {
                var refId = refs[i];
                if (string.IsNullOrEmpty(refId))
                {
                    continue;
                }

                if (behaviorCatalog == null || !behaviorCatalog.ContainsKey(refId))
                {
                    throw new InvalidOperationException($"Behavior reference not found: {refId}");
                }

                children.Add(new JObject { ["behaviorRef"] = refId });
            }
        }

        private static string GetReferenceId(JObject node)
        {
            if (node == null)
            {
                return null;
            }

            return node["behaviorRef"]?.ToString()
                ?? node["behaviorId"]?.ToString()
                ?? node["behavior"]?.ToString()
                ?? node["ref"]?.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// Resolves source-format condition/action fragments and reference groups into normalized inline nodes.
    /// </summary>
    internal static class TriggerPlanSourceFragmentResolver
    {
        public static Dictionary<string, JToken> BuildCatalog(params Dictionary<string, JToken>[] sources)
        {
            var catalog = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
            if (sources == null) return catalog;

            for (int i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                if (source == null) continue;

                foreach (var kvp in source)
                {
                    if (string.IsNullOrEmpty(kvp.Key) || kvp.Value == null) continue;
                    catalog[kvp.Key] = kvp.Value;
                }
            }

            return catalog;
        }

        public static List<JObject> ResolveConditionList(
            List<JObject> inlineConditions,
            List<string> conditionRefs,
            List<string> conditionRefsSnake,
            Dictionary<string, JToken> catalog,
            HashSet<string> resolving)
        {
            var result = new List<JObject>();
            AddReferencedConditions(result, conditionRefs, catalog, resolving);
            AddReferencedConditions(result, conditionRefsSnake, catalog, resolving);

            if (inlineConditions != null)
            {
                for (int i = 0; i < inlineConditions.Count; i++)
                {
                    AddCondition(result, inlineConditions[i], catalog, resolving);
                }
            }

            return result;
        }

        public static List<JObject> ResolveActionList(
            List<JObject> inlineActions,
            List<string> actionRefs,
            List<string> actionRefsSnake,
            Dictionary<string, JToken> catalog,
            HashSet<string> resolving)
        {
            var result = new List<JObject>();
            AddReferencedActions(result, actionRefs, catalog, resolving);
            AddReferencedActions(result, actionRefsSnake, catalog, resolving);

            if (inlineActions != null)
            {
                for (int i = 0; i < inlineActions.Count; i++)
                {
                    AddAction(result, inlineActions[i], catalog, resolving);
                }
            }

            return result;
        }

        public static JObject ResolveAction(JObject action, Dictionary<string, JToken> catalog, HashSet<string> resolving)
        {
            if (action == null) return null;

            var refId = GetFragmentReferenceId(action, "actionRef", "actionId", "actionGroup", "actionGroupId");
            if (string.IsNullOrEmpty(refId))
            {
                return NormalizeActionNode(action, catalog, resolving);
            }

            var fragment = BeginResolveFragment(refId, catalog, resolving, "action group");
            try
            {
                if (fragment is JObject obj)
                {
                    if (TryReadObjectList(obj, out var items, "actions", "items"))
                    {
                        if (items.Count != 1)
                        {
                            throw new InvalidOperationException($"Action reference must resolve to exactly one action in this context: {refId}");
                        }

                        return ResolveAction(items[0], catalog, resolving);
                    }

                    return (JObject)obj.DeepClone();
                }

                if (fragment is JArray arr && arr.Count == 1 && arr[0] is JObject single)
                {
                    return ResolveAction(single, catalog, resolving);
                }
            }
            finally
            {
                resolving.Remove(refId);
            }

            throw new InvalidOperationException($"Action reference must resolve to one action: {refId}");
        }

        public static List<string> ReadStringList(JObject obj, params string[] aliases)
        {
            var result = new List<string>();
            if (obj == null || aliases == null) return result;

            for (int i = 0; i < aliases.Length; i++)
            {
                if (!obj.TryGetValue(aliases[i], StringComparison.OrdinalIgnoreCase, out var token)) continue;

                if (token is JArray arr)
                {
                    foreach (var item in arr)
                    {
                        var value = item?.ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            result.Add(value);
                        }
                    }
                }
                else
                {
                    var value = token?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        result.Add(value);
                    }
                }
            }

            return result;
        }

        public static bool TryReadObjectList(JObject obj, out List<JObject> items, params string[] aliases)
        {
            items = null;
            if (obj == null || aliases == null) return false;

            for (int i = 0; i < aliases.Length; i++)
            {
                if (!obj.TryGetValue(aliases[i], StringComparison.OrdinalIgnoreCase, out var token)) continue;

                items = new List<JObject>();
                if (token is JArray arr)
                {
                    foreach (var item in arr)
                    {
                        if (item is JObject child)
                        {
                            items.Add(child);
                        }
                    }
                }
                else if (token is JObject child)
                {
                    items.Add(child);
                }

                return items.Count > 0;
            }

            return false;
        }

        private static void AddReferencedConditions(List<JObject> result, List<string> refs, Dictionary<string, JToken> catalog, HashSet<string> resolving)
        {
            if (refs == null) return;
            for (int i = 0; i < refs.Count; i++)
            {
                AddConditionRef(result, refs[i], catalog, resolving);
            }
        }

        private static void AddCondition(List<JObject> result, JObject condition, Dictionary<string, JToken> catalog, HashSet<string> resolving)
        {
            if (condition == null) return;

            var refId = GetFragmentReferenceId(condition, "conditionRef", "conditionId", "conditionGroup", "conditionGroupId");
            if (!string.IsNullOrEmpty(refId))
            {
                AddConditionRef(result, refId, catalog, resolving);
                return;
            }

            result.Add(NormalizeConditionNode(condition, catalog, resolving));
        }

        private static JObject NormalizeConditionNode(JObject condition, Dictionary<string, JToken> catalog, HashSet<string> resolving)
        {
            var normalized = (JObject)condition.DeepClone();
            var type = normalized["type"]?.ToString();
            if (!TriggerPlanSourceJsonUtility.IsKind(type, "all", "and", "any", "or", "not"))
            {
                return normalized;
            }

            var inlineItems = TriggerPlanSourceConditionWriter.ReadConditionItems(normalized);
            var conditionRefs = ReadStringList(normalized, "conditionRefs");
            var conditionRefsSnake = ReadStringList(normalized, "condition_refs");
            var mergedItems = ResolveConditionList(inlineItems, conditionRefs, conditionRefsSnake, catalog, resolving);

            RemoveProperties(normalized, "conditionRefs", "condition_refs", "conditions", "condition", "item");
            normalized["items"] = new JArray(mergedItems);
            return normalized;
        }

        private static void AddConditionRef(List<JObject> result, string refId, Dictionary<string, JToken> catalog, HashSet<string> resolving)
        {
            var fragment = BeginResolveFragment(refId, catalog, resolving, "condition group");
            if (fragment == null) return;

            try
            {
                if (fragment is JArray arr)
                {
                    foreach (var item in arr)
                    {
                        if (item is JObject obj)
                        {
                            AddCondition(result, obj, catalog, resolving);
                        }
                    }
                    return;
                }

                if (fragment is JObject single)
                {
                    if (TryReadObjectList(single, out var items, "conditions", "items"))
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            AddCondition(result, items[i], catalog, resolving);
                        }
                        return;
                    }

                    AddCondition(result, single, catalog, resolving);
                }
            }
            finally
            {
                resolving.Remove(refId);
            }
        }

        private static void AddReferencedActions(List<JObject> result, List<string> refs, Dictionary<string, JToken> catalog, HashSet<string> resolving)
        {
            if (refs == null) return;
            for (int i = 0; i < refs.Count; i++)
            {
                AddActionRef(result, refs[i], catalog, resolving);
            }
        }

        private static void AddAction(List<JObject> result, JObject action, Dictionary<string, JToken> catalog, HashSet<string> resolving)
        {
            if (action == null) return;

            var resolved = ResolveAction(action, catalog, resolving);
            if (resolved == null) return;

            if (IsCompositeAction(resolved))
            {
                var inlineItems = ReadActionItems(resolved);
                var actionRefs = ReadStringList(resolved, "actionRefs");
                var actionRefsSnake = ReadStringList(resolved, "action_refs");
                result.AddRange(ResolveActionList(inlineItems, actionRefs, actionRefsSnake, catalog, resolving));
                return;
            }

            result.Add(resolved);
        }

        private static void AddActionRef(List<JObject> result, string refId, Dictionary<string, JToken> catalog, HashSet<string> resolving)
        {
            var fragment = BeginResolveFragment(refId, catalog, resolving, "action group");
            if (fragment == null) return;

            try
            {
                if (fragment is JArray arr)
                {
                    foreach (var item in arr)
                    {
                        if (item is JObject obj)
                        {
                            AddAction(result, obj, catalog, resolving);
                        }
                    }
                    return;
                }

                if (fragment is JObject single)
                {
                    if (TryReadObjectList(single, out var items, "actions", "items"))
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            AddAction(result, items[i], catalog, resolving);
                        }
                        return;
                    }

                    AddAction(result, single, catalog, resolving);
                }
            }
            finally
            {
                resolving.Remove(refId);
            }
        }

        private static JObject NormalizeActionNode(JObject action, Dictionary<string, JToken> catalog, HashSet<string> resolving)
        {
            var normalized = (JObject)action.DeepClone();
            if (!IsCompositeAction(normalized))
            {
                return normalized;
            }

            var inlineItems = ReadActionItems(normalized);
            var actionRefs = ReadStringList(normalized, "actionRefs");
            var actionRefsSnake = ReadStringList(normalized, "action_refs");
            var mergedItems = ResolveActionList(inlineItems, actionRefs, actionRefsSnake, catalog, resolving);

            RemoveProperties(normalized, "actionRefs", "action_refs", "actions");
            normalized["items"] = new JArray(mergedItems);
            return normalized;
        }

        private static bool IsCompositeAction(JObject action)
        {
            var type = action?["type"]?.ToString();
            return TriggerPlanSourceJsonUtility.IsKind(type, "seq", "sequence");
        }

        private static List<JObject> ReadActionItems(JObject action)
        {
            var items = new List<JObject>();
            if (TryReadObjectList(action, out var fromList, "items", "actions"))
            {
                items.AddRange(fromList);
            }

            return items;
        }

        private static void RemoveProperties(JObject obj, params string[] names)
        {
            if (obj == null || names == null) return;
            for (int i = 0; i < names.Length; i++)
            {
                obj.Remove(names[i]);
            }
        }

        private static JToken BeginResolveFragment(string refId, Dictionary<string, JToken> catalog, HashSet<string> resolving, string kind)
        {
            if (string.IsNullOrEmpty(refId)) return null;
            if (catalog == null || !catalog.TryGetValue(refId, out var fragment) || fragment == null)
            {
                throw new InvalidOperationException($"{kind} reference not found: {refId}");
            }

            if (!resolving.Add(refId))
            {
                throw new InvalidOperationException($"Cyclic {kind} reference detected: {refId}");
            }

            return fragment.DeepClone();
        }

        private static string GetFragmentReferenceId(JObject obj, params string[] aliases)
        {
            if (obj == null || aliases == null) return null;
            for (int i = 0; i < aliases.Length; i++)
            {
                if (obj.TryGetValue(aliases[i], StringComparison.OrdinalIgnoreCase, out var token))
                {
                    return token?.ToString();
                }
            }

            return obj["ref"]?.ToString();
        }
    }
}

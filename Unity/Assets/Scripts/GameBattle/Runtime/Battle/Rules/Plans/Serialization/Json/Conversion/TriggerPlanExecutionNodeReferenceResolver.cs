using System;
using System.Collections.Generic;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// Resolves execution-node behavior/node references for runtime plan JSON conversion.
    /// </summary>
    internal sealed class TriggerPlanExecutionNodeReferenceResolver
    {
        private HashSet<string> _resolving;

        public TriggerPlanJsonDatabase.ExecutionNodeDto Resolve(
            TriggerPlanJsonDatabase.ExecutionNodeDto dto,
            TriggerPlanJsonDatabase.TriggerPlanDatabaseDto databaseDto,
            List<string> resolvingKeys)
        {
            var behaviorCatalog = BuildCatalog(databaseDto?.Behaviors);
            var nodeCatalog = BuildCatalog(databaseDto?.Nodes);

            while (dto != null && TryGetReference(dto, out var id, out var kind))
            {
                var key = kind + ":" + id;
                if (_resolving == null)
                {
                    _resolving = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                if (!_resolving.Add(key))
                {
                    throw new InvalidOperationException($"Cyclic execution node reference detected: {key}");
                }

                resolvingKeys?.Add(key);
                dto = ResolveTarget(id, kind, behaviorCatalog, nodeCatalog);
            }

            return dto;
        }

        public void EndResolve(List<string> resolvingKeys)
        {
            if (resolvingKeys == null || _resolving == null)
            {
                return;
            }

            for (int i = resolvingKeys.Count - 1; i >= 0; i--)
            {
                _resolving.Remove(resolvingKeys[i]);
            }
        }

        private static Dictionary<string, TriggerPlanJsonDatabase.ExecutionNodeDto> BuildCatalog(Dictionary<string, TriggerPlanJsonDatabase.ExecutionNodeDto> catalog)
        {
            if (catalog == null || catalog.Count == 0)
            {
                return null;
            }

            return new Dictionary<string, TriggerPlanJsonDatabase.ExecutionNodeDto>(catalog, StringComparer.OrdinalIgnoreCase);
        }

        private static TriggerPlanJsonDatabase.ExecutionNodeDto ResolveTarget(
            string id,
            string kind,
            Dictionary<string, TriggerPlanJsonDatabase.ExecutionNodeDto> behaviorCatalog,
            Dictionary<string, TriggerPlanJsonDatabase.ExecutionNodeDto> nodeCatalog)
        {
            if (string.Equals(kind, "behavior", StringComparison.OrdinalIgnoreCase))
            {
                if (behaviorCatalog != null && behaviorCatalog.TryGetValue(id, out var behavior) && behavior != null)
                {
                    return behavior;
                }

                throw new InvalidOperationException($"Behavior reference not found: {id}");
            }

            if (string.Equals(kind, "node", StringComparison.OrdinalIgnoreCase))
            {
                if (nodeCatalog != null && nodeCatalog.TryGetValue(id, out var node) && node != null)
                {
                    return node;
                }

                throw new InvalidOperationException($"Node reference not found: {id}");
            }

            if (behaviorCatalog != null && behaviorCatalog.TryGetValue(id, out var behaviorRef) && behaviorRef != null)
            {
                return behaviorRef;
            }

            if (nodeCatalog != null && nodeCatalog.TryGetValue(id, out var nodeRef) && nodeRef != null)
            {
                return nodeRef;
            }

            throw new InvalidOperationException($"Execution node reference not found: {id}");
        }

        private static bool TryGetReference(TriggerPlanJsonDatabase.ExecutionNodeDto dto, out string id, out string kind)
        {
            id = null;
            kind = null;
            if (dto == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(dto.BehaviorRef))
            {
                id = dto.BehaviorRef;
                kind = "behavior";
                return true;
            }

            if (!string.IsNullOrEmpty(dto.BehaviorId))
            {
                id = dto.BehaviorId;
                kind = "behavior";
                return true;
            }

            if (!string.IsNullOrEmpty(dto.NodeRef))
            {
                id = dto.NodeRef;
                kind = "node";
                return true;
            }

            if (!string.IsNullOrEmpty(dto.NodeId))
            {
                id = dto.NodeId;
                kind = "node";
                return true;
            }

            if (!string.IsNullOrEmpty(dto.Ref))
            {
                id = dto.Ref;
                kind = "any";
                return true;
            }

            return false;
        }
    }
}

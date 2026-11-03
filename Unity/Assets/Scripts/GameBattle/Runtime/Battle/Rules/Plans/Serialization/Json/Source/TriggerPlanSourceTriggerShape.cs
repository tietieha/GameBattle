using System;
using AbilityKit.Triggering.Runtime.Config.Plans;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// Reads source-format trigger metadata and normalizes trigger-level source aliases.
    /// </summary>
    internal sealed class TriggerPlanSourceTriggerShape
    {
        public JObject GetExecutionRoot(TriggerSourceTriggerJson trigger)
        {
            if (trigger == null) return null;
            if (trigger.behavior != null) return trigger.behavior;

            if (trigger.executables == null || trigger.executables.Count == 0)
                return null;

            return new JObject
            {
                ["type"] = "sequence",
                ["children"] = new JArray(trigger.executables)
            };
        }

        public int ParsePhase(string phase)
        {
            if (string.IsNullOrEmpty(phase)) return 0;

            return phase.Trim().ToLowerInvariant() switch
            {
                "immediate" => 0,
                "delayed" => 1,
                "precondition" => 2,
                "postcondition" => 3,
                _ => throw new InvalidOperationException($"Unsupported trigger phase: {phase}")
            };
        }

        public TriggerPlanScope ParseScope(string scope)
        {
            if (string.IsNullOrEmpty(scope)) return TriggerPlanScope.Global;
            switch (scope.Trim().ToLowerInvariant())
            {
                case "owner":
                case "ownerbound":
                case "owner_bound":
                case "owner-bound":
                    return TriggerPlanScope.OwnerBound;
                case "global":
                    return TriggerPlanScope.Global;
                default:
                    throw new InvalidOperationException($"Unsupported trigger scope: {scope}");
            }
        }
    }
}

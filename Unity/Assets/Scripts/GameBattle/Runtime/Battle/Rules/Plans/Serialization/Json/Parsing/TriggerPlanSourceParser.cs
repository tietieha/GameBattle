using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// Parses source-format trigger plan JSON and normalizes supported source roots into DTOs.
    /// </summary>
    internal sealed class TriggerPlanSourceParser
    {
        public TriggerPlanSourceJson Parse(string sourceJson)
        {
            var root = JObject.Parse(sourceJson);
            if (IsSourceDocument(root))
            {
                var source = root.ToObject<TriggerPlanSourceJson>();
                if (source?.triggers != null && source.triggers.Count > 0)
                {
                    return source;
                }
            }

            var singleTrigger = root.ToObject<TriggerSourceTriggerJson>();
            return new TriggerPlanSourceJson
            {
                triggers = singleTrigger != null && singleTrigger.id > 0
                    ? new List<TriggerSourceTriggerJson> { singleTrigger }
                    : new List<TriggerSourceTriggerJson>()
            };
        }

        private static bool IsSourceDocument(JObject root)
        {
            return root["triggers"] != null
                || root["actions"] is JObject
                || root["conditions"] is JObject
                || root["version"] != null
                || root["metadata"] != null;
        }
    }
}

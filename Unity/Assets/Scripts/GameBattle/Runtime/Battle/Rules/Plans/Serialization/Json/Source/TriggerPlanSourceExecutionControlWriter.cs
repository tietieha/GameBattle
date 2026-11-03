using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// Writes execution-control settings for source-format triggers.
    /// </summary>
    internal sealed class TriggerPlanSourceExecutionControlWriter
    {
        public void WriteExecutionControl(JsonTextWriter writer, TriggerSourceTriggerJson trigger)
        {
            var executionToken = trigger.execution ?? trigger.executionControl ?? trigger.execution_control;
            var mode = trigger.once ? "once" : trigger.repeat ? "repeat" : null;
            var maxExecutions = trigger.max_executions > 0 ? trigger.max_executions : trigger.maxExecutions;
            var cooldownMs = trigger.cooldown_ms > 0f ? trigger.cooldown_ms : trigger.cooldownMs;

            if (executionToken != null && executionToken.Type != JTokenType.Null)
            {
                if (executionToken.Type == JTokenType.String)
                {
                    mode = executionToken.Value<string>();
                }
                else if (executionToken is JObject obj)
                {
                    mode = TriggerPlanSourceJsonUtility.ReadString(obj, "mode", "type") ?? mode;
                    maxExecutions = TriggerPlanSourceJsonUtility.ReadInt(obj, maxExecutions, "max_executions", "maxExecutions", "count", "times");
                    cooldownMs = TriggerPlanSourceJsonUtility.ReadFloat(obj, cooldownMs, "cooldown_ms", "cooldownMs", "cooldown", "interval_ms", "intervalMs");
                }
            }

            if (string.IsNullOrEmpty(mode) && cooldownMs > 0f)
            {
                mode = "cooldown";
            }

            if (string.IsNullOrEmpty(mode))
            {
                return;
            }

            writer.WritePropertyName("ExecutionControl");
            writer.WriteStartObject();
            writer.WritePropertyName("Mode");
            writer.WriteValue(mode);
            if (maxExecutions > 0)
            {
                writer.WritePropertyName("MaxExecutions");
                writer.WriteValue(maxExecutions);
            }
            if (cooldownMs > 0f)
            {
                writer.WritePropertyName("CooldownMs");
                writer.WriteValue(cooldownMs);
            }
            writer.WriteEndObject();
        }
    }
}

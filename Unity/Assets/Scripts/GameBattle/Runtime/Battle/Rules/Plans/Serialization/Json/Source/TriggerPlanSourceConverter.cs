using System;
using System.Collections.Generic;
using System.IO;
using AbilityKit.Triggering.Eventing;
using AbilityKit.Triggering.Runtime.Config.Plans;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// 瑙﹀彂鍣ㄨ鍒掓簮鏍煎紡杞崲鍣?    /// 灏嗕汉绫诲彲璇荤殑婧愭牸寮?JSON 杞崲涓鸿繍琛屾椂鏍煎紡 JSON
    /// </summary>
    public sealed class TriggerPlanSourceConverter
    {
        private readonly TriggerPlanSourceConditionWriter _conditionWriter = new TriggerPlanSourceConditionWriter();
        private readonly Dictionary<int, string> _strings = new Dictionary<int, string>();
        private readonly TriggerPlanSourceActionWriter _actionWriter;
        private readonly TriggerPlanSourceBehaviorResolver _behaviorResolver = new TriggerPlanSourceBehaviorResolver();
        private readonly TriggerPlanSourceExecutionControlWriter _executionControlWriter = new TriggerPlanSourceExecutionControlWriter();
        private readonly TriggerPlanSourceExecutionNodeOptionWriter _executionNodeOptionWriter;
        private readonly TriggerPlanSourceExecutionNodeShape _executionNodeShape = new TriggerPlanSourceExecutionNodeShape();
        private readonly TriggerPlanSourceExecutionNodeWriter _executionNodeWriter;
        private readonly TriggerPlanSourceParser _sourceParser = new TriggerPlanSourceParser();
        private readonly TriggerPlanSourceTriggerShape _triggerShape = new TriggerPlanSourceTriggerShape();

        public TriggerPlanSourceConverter()
        {
            _actionWriter = new TriggerPlanSourceActionWriter(_strings);
            _executionNodeOptionWriter = new TriggerPlanSourceExecutionNodeOptionWriter(_conditionWriter, _actionWriter.WriteParamValue);
            _executionNodeWriter = new TriggerPlanSourceExecutionNodeWriter(_conditionWriter, _actionWriter, _behaviorResolver, _executionNodeOptionWriter, _executionNodeShape);
        }
        /// <summary>
        /// 灏嗘簮鏍煎紡 JSON 瀛楃涓茶浆鎹负杩愯鏃舵牸寮?JSON 瀛楃涓?        /// </summary>
        public string ConvertSourceToRuntimeJson(string sourceJson)
        {
            if (string.IsNullOrEmpty(sourceJson))
            {
                return "{\"Triggers\":[],\"Strings\":{}}";
            }

            var source = _sourceParser.Parse(sourceJson);
            return ConvertSource(source);
        }

        private string ConvertSource(TriggerPlanSourceJson source)
        {
            var behaviorCatalog = TriggerPlanSourceBehaviorResolver.BuildCatalog(source?.behaviors);
            var conditionCatalog = TriggerPlanSourceFragmentResolver.BuildCatalog(source?.conditionGroups, source?.condition_groups);
            var actionCatalog = TriggerPlanSourceFragmentResolver.BuildCatalog(source?.actionGroups, source?.action_groups);
            using (var sw = new StringWriter())
            {
                using (var writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartObject();
                    writer.WritePropertyName("Triggers");
                    writer.WriteStartArray();

                    if (source?.triggers != null)
                    {
                        foreach (var trigger in source.triggers)
                        {
                            if (trigger == null) continue;
                            if (trigger.enabled == false) continue;
                            WriteTrigger(writer, trigger, source.actions, conditionCatalog, actionCatalog, behaviorCatalog);
                        }
                    }

                    writer.WriteEndArray();
                    writer.WritePropertyName("Strings");
                    writer.WriteStartObject();
                    foreach (var kv in _strings)
                    {
                        writer.WritePropertyName(kv.Key.ToString());
                        writer.WriteValue(kv.Value);
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                return sw.ToString();
            }
        }

        private void WriteTrigger(
            JsonTextWriter writer,
            TriggerSourceTriggerJson trigger,
            Dictionary<string, ActionSourceDefJson> actionSchemas,
            Dictionary<string, JToken> conditionCatalog,
            Dictionary<string, JToken> actionCatalog,
            Dictionary<string, JObject> behaviorCatalog)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("TriggerId");
            writer.WriteValue(trigger.id);

            if (!string.IsNullOrEmpty(trigger.@event))
            {
                writer.WritePropertyName("EventName");
                writer.WriteValue(trigger.@event);
                writer.WritePropertyName("EventId");
                writer.WriteValue(StableStringId.Get("event:" + trigger.@event));
            }
            else
            {
                writer.WritePropertyName("EventId");
                writer.WriteValue(0);
            }

            writer.WritePropertyName("AllowExternal");
            writer.WriteValue(trigger.allowExternal);

            writer.WritePropertyName("Phase");
            writer.WriteValue(_triggerShape.ParsePhase(trigger.phase));

            writer.WritePropertyName("Priority");
            writer.WriteValue(trigger.priority);

            writer.WritePropertyName("Scope");
            writer.WriteValue((int)_triggerShape.ParseScope(trigger.scope));

            writer.WritePropertyName("Predicate");
            _conditionWriter.WritePredicate(writer, TriggerPlanSourceFragmentResolver.ResolveConditionList(trigger.conditions, trigger.conditionRefs, trigger.condition_refs, conditionCatalog, new HashSet<string>(StringComparer.OrdinalIgnoreCase)), _actionWriter.WriteParamValue);

            _executionControlWriter.WriteExecutionControl(writer, trigger);

            writer.WritePropertyName("Actions");
            writer.WriteStartArray();
            var actions = TriggerPlanSourceFragmentResolver.ResolveActionList(trigger.actions, trigger.actionRefs, trigger.action_refs, actionCatalog, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            if (actions != null)
            {
                foreach (var action in actions)
                {
                    _actionWriter.WriteAction(writer, action, actionSchemas, actionCatalog, TriggerPlanSourceFragmentResolver.ResolveAction);
                }
            }
            writer.WriteEndArray();

            var root = _triggerShape.GetExecutionRoot(trigger);
            if (root != null)
            {
                writer.WritePropertyName("ExecutionRoot");
                _executionNodeWriter.Write(writer, root, actionSchemas, conditionCatalog, actionCatalog, behaviorCatalog, $"trigger:{trigger.id}", "inline", trigger.id.ToString(), new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }
 
            writer.WriteEndObject();
        }

    }
}

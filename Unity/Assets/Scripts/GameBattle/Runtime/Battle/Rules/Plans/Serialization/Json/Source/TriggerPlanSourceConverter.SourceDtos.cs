using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// 源格式 JSON 结构。
    /// </summary>
#pragma warning disable 0649 // DTO fields are populated by JSON deserialization.
    internal sealed class TriggerPlanSourceJson
    {
        public string version;
        public TriggerSourceMetadataJson metadata;
        public List<TriggerSourceVariableJson> variables;
        public Dictionary<string, ActionSourceDefJson> actions;
        public Dictionary<string, ConditionSourceDefJson> conditions;
        public Dictionary<string, BehaviorSourceDefJson> behaviors;
        public Dictionary<string, JToken> conditionGroups;
        public Dictionary<string, JToken> condition_groups;
        public Dictionary<string, JToken> actionGroups;
        public Dictionary<string, JToken> action_groups;
        public List<TriggerSourceTriggerJson> triggers;
    }

    internal sealed class TriggerSourceMetadataJson
    {
        public string author;
        public string created_at;
        public string last_modified;
        public string description;
    }

    internal sealed class TriggerSourceVariableJson
    {
        public string name;
        public string description;
    }

    internal sealed class ActionSourceDefJson
    {
        public string type;
        public string displayName;
        public string description;
        public string category;
        public bool isComposite;
        public List<ActionSourceParamJson> @params;
    }

    internal sealed class ActionSourceParamJson
    {
        public string name;
        public string type;
        public bool required;
        public object defaultValue;
    }

    internal sealed class ConditionSourceDefJson
    {
        public string type;
        public string displayName;
        public string description;
        public string category;
        public bool isComposite;
        public List<ConditionSourceParamJson> @params;
    }

    internal sealed class ConditionSourceParamJson
    {
        public string name;
        public string type;
        public bool required;
        public object defaultValue;
    }

    internal sealed class BehaviorSourceDefJson
    {
        public string id;
        public string displayName;
        public string description;
        public JObject behavior;
        public JObject root;
    }
#pragma warning restore 0649

    internal readonly struct ActionArgSource
    {
        public readonly string Name;
        public readonly JToken Value;

        public ActionArgSource(string name, JToken value)
        {
            Name = name;
            Value = value;
        }
    }

    internal sealed class TriggerSourceTriggerJson
    {
        public int id;
        public string name;
        public string @event;
        public int priority;
        public string phase;
        public string scope;
        public bool? enabled;
        public bool allowExternal;
        public string comment;
        public List<JObject> conditions;
        public List<JObject> actions;
        public List<string> conditionRefs;
        public List<string> condition_refs;
        public List<string> actionRefs;
        public List<string> action_refs;
        public JObject behavior;
        public List<JObject> executables;
        public JToken execution;
        public JToken executionControl;
        public JToken execution_control;
        public bool once;
        public bool repeat;
        public int maxExecutions;
        public int max_executions;
        public float cooldownMs;
        public float cooldown_ms;
    }
}

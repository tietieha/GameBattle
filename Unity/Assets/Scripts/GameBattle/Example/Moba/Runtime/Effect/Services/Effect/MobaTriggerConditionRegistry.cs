using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.Services;
using AbilityKit.Ability.World.Services.Attributes;

namespace AbilityKit.Demo.Moba.Services
{
    public readonly struct MobaTriggerConditionCheckResult
    {
        private MobaTriggerConditionCheckResult(bool passed, string reason, string failureKey)
        {
            Passed = passed;
            Reason = reason;
            FailureKey = failureKey;
        }

        public bool Passed { get; }
        public string Reason { get; }
        public string FailureKey { get; }

        public static MobaTriggerConditionCheckResult Pass => new MobaTriggerConditionCheckResult(true, null, null);

        public static MobaTriggerConditionCheckResult Fail(string reason, string failureKey = null)
        {
            return new MobaTriggerConditionCheckResult(false, reason, failureKey);
        }
    }

    public interface IMobaTriggerCondition
    {
        string Id { get; }
        MobaTriggerConditionCheckResult Check(in MobaTriggerConditionContext context);
    }

    public abstract class MobaTriggerConditionBase : IMobaTriggerCondition
    {
        public abstract string Id { get; }
        public abstract MobaTriggerConditionCheckResult Check(in MobaTriggerConditionContext context);

        protected static MobaTriggerConditionCheckResult Pass => MobaTriggerConditionCheckResult.Pass;

        protected static MobaTriggerConditionCheckResult Fail(string reason, string failureKey = null)
        {
            return MobaTriggerConditionCheckResult.Fail(reason, failureKey);
        }
    }

    [WorldService(typeof(MobaTriggerConditionRegistry))]
    public sealed class MobaTriggerConditionRegistry : IService
    {
        private readonly Dictionary<string, IMobaTriggerCondition> _conditionsById = new Dictionary<string, IMobaTriggerCondition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, List<IMobaTriggerCondition>> _conditionsByTriggerId = new Dictionary<int, List<IMobaTriggerCondition>>();

        public void Register(IMobaTriggerCondition condition)
        {
            if (condition == null || string.IsNullOrWhiteSpace(condition.Id)) return;
            _conditionsById[condition.Id] = condition;
        }

        public bool TryGet(string id, out IMobaTriggerCondition condition)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                condition = null;
                return false;
            }

            return _conditionsById.TryGetValue(id, out condition);
        }

        public void BindTriggerCondition(int triggerId, IMobaTriggerCondition condition)
        {
            if (triggerId <= 0 || condition == null) return;
            Register(condition);
            if (!_conditionsByTriggerId.TryGetValue(triggerId, out var list))
            {
                list = new List<IMobaTriggerCondition>();
                _conditionsByTriggerId[triggerId] = list;
            }

            if (!list.Contains(condition)) list.Add(condition);
        }

        public void BindTriggerCondition(int triggerId, string conditionId)
        {
            if (triggerId <= 0) return;
            if (!TryGet(conditionId, out var condition)) return;
            BindTriggerCondition(triggerId, condition);
        }

        public bool HasConditions(int triggerId)
        {
            return triggerId > 0 && _conditionsByTriggerId.TryGetValue(triggerId, out var list) && list.Count > 0;
        }

        public MobaTriggerConditionCheckResult Evaluate(int triggerId, in MobaTriggerConditionContext context)
        {
            if (!HasConditions(triggerId)) return MobaTriggerConditionCheckResult.Pass;

            var list = _conditionsByTriggerId[triggerId];
            for (int i = 0; i < list.Count; i++)
            {
                var result = list[i]?.Check(in context) ?? MobaTriggerConditionCheckResult.Pass;
                if (!result.Passed) return result;
            }

            return MobaTriggerConditionCheckResult.Pass;
        }

        public void Dispose()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using AbilityKit.Core.Eventing;
using AbilityKit.Triggering.Eventing;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime.Plan;
using AbilityKit.Triggering.Runtime.RuleScheduler;
using AbilityKit.Triggering.Validation;

namespace AbilityKit.Triggering.Runtime.Example
{
    public static class FormalTriggeringMainlineExample
    {
        public sealed class DamageEvent
        {
            public readonly int Amount;

            public DamageEvent(int amount)
            {
                Amount = amount;
            }
        }

        public sealed class ExampleContext
        {
            public int MinDamage { get; set; }
            public int HitCount { get; set; }
            public List<string> Logs { get; } = new List<string>();
        }

        private sealed class ExampleContextSource : ITriggerContextSource<ExampleContext>
        {
            public ExampleContext Current { get; set; }

            public ExampleContext GetContext()
            {
                return Current;
            }
        }

        public static void RunOnce()
        {
            var bus = new EventBus();
            var functions = new FunctionRegistry();
            var actions = new ActionRegistry();
            var contextSource = new ExampleContextSource
            {
                Current = new ExampleContext { MinDamage = 10 }
            };

            var triggerKey = new EventKey<DamageEvent>(Eventing.StableStringId.Get("sample:formal:damage"));
            var functionId = new FunctionId(Eventing.StableStringId.Get("sample:formal:is_strong_hit"));
            var actionId = new ActionId(Eventing.StableStringId.Get("sample:formal:record_hit"));

            functions.Register<Predicate0<DamageEvent, ExampleContext>>(
                functionId,
                (evt, ctx) => evt.Amount >= ctx.Context.MinDamage,
                isDeterministic: true);

            actions.Register<NamedAction1<DamageEvent, object, ExampleContext>>(
                actionId,
                (evt, actionArgs, ctx) =>
                {
                    ctx.Context.HitCount++;
                    ctx.Context.Logs.Add($"hit:{evt.Amount}");
                },
                isDeterministic: true);

            var plan = TriggerPlanFactory.When<DamageEvent>(
                phase: 0,
                priority: 0,
                predicateId: functionId,
                actions: new[]
                {
                    new ActionCallPlan(actionId, NumericValueRef.Const(0d))
                });

            var database = new TriggerPlanDatabase<DamageEvent>(new[]
            {
                new TriggerPlanEntry<DamageEvent>(triggerKey, plan, id: "sample:formal:damage")
            });

            var validator = CompositeTriggerValidator<DamageEvent>.CreateMinimal();
            var validationContext = ValidationContext<DamageEvent>.CreateForDevelopment(
                definedFunctionIds: new HashSet<string> { functionId.Value.ToString() },
                definedActionIds: new HashSet<string> { actionId.Value.ToString() },
                definedEventKeys: new HashSet<string> { triggerKey.StringId });
            var validationResult = validator.Validate(in database, in validationContext);
            validationResult.ThrowIfInvalid("Formal trigger sample validation failed");

            var runner = new TriggerRunner<ExampleContext>(bus, functions, actions, contextSource: contextSource);
            runner.RegisterPlan(triggerKey, in plan);
            bus.Publish(triggerKey, new DamageEvent(12));
            bus.Flush();

            var scheduler = new RuleSchedulerRegistry();
            var handle = scheduler.Schedule(
                RuleSchedulePlan.After(250f, groupId: "sample:formal", subjectId: "damage", label: "formal-mainline"),
                new DelegateRuleScheduleEffect(_ => contextSource.Current.Logs.Add($"scheduled:{contextSource.Current.HitCount}")));
            scheduler.Update(250f, contextSource.Current);

            contextSource.Current.Logs.Add(handle.ToString());
        }
    }
}

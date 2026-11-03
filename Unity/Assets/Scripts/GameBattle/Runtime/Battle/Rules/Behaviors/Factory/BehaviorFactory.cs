using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.Abstractions;
using AbilityKit.Triggering.Runtime.Behavior;
using AbilityKit.Triggering.Runtime.Behavior.Actions;
using AbilityKit.Triggering.Runtime.Behavior.Predicates;
using AbilityKit.Triggering.Runtime.Behavior.Schedule;
using AbilityKit.Triggering.Runtime.Config;
using AbilityKit.Triggering.Runtime.Config.Actions;
using AbilityKit.Triggering.Runtime.Config.Cue;
using AbilityKit.Triggering.Runtime.Config.Plans;
using AbilityKit.Triggering.Runtime.Config.Predicates;
using AbilityKit.Triggering.Runtime.Config.Schedule;
using AbilityKit.Triggering.Runtime.Config.Values;
using AbilityKit.Triggering.Runtime.Executable;
using AbilityKit.Triggering.Variables.Numeric;
using AbilityKit.Triggering.Variables.Numeric.Expression;

namespace AbilityKit.Triggering.Runtime.Factory
{
    /// <summary>
    /// 行为工厂接口
    /// </summary>
    public interface IBehaviorFactory
    {
        ITriggerBehavior Create(ITriggerPlanConfig planConfig);
        ISchedulableBehavior CreateScheduled(ITriggerPlanConfig planConfig);
        ISimpleTriggerBehavior CreateSimple(ITriggerPlanConfig planConfig);
        IConditionalBehavior CreatePredicate(IPredicateConfig predicateConfig);
        IActionBehavior CreateAction(IActionCallConfig actionConfig);
        List<ITriggerBehavior> CreateActions(IReadOnlyList<IActionCallConfig> actions);
    }

    /// <summary>
    /// 行为工厂实现
    /// </summary>
    public class BehaviorFactory : IBehaviorFactory
    {
        private readonly IValueResolver _valueResolver;
        private readonly IActionRegistry _actionRegistry;
        private readonly IConditionalBehaviorResolver _predicateResolver;
        private readonly ITriggerCueFactory _cueFactory;

        public BehaviorFactory(
            IValueResolver valueResolver,
            IActionRegistry actionRegistry,
            IConditionalBehaviorResolver predicateResolver,
            ITriggerCueFactory cueFactory)
        {
            _valueResolver = valueResolver ?? throw new ArgumentNullException(nameof(valueResolver));
            _actionRegistry = actionRegistry ?? throw new ArgumentNullException(nameof(actionRegistry));
            _predicateResolver = predicateResolver;
            _cueFactory = cueFactory;
        }

        public ITriggerBehavior Create(ITriggerPlanConfig planConfig)
        {
            if (planConfig == null)
                throw new ArgumentNullException(nameof(planConfig));

            var scheduleConfig = planConfig.Schedule;
            if (scheduleConfig != null && scheduleConfig.IsEmpty)
            {
                return CreateSimple(planConfig);
            }

            return CreateScheduled(planConfig);
        }

        public ISchedulableBehavior CreateScheduled(ITriggerPlanConfig planConfig)
        {
            var schedule = planConfig.Schedule;
            return schedule.Mode switch
            {
                EScheduleMode.Timed => new TimedTriggerBehavior(planConfig, this, _valueResolver, _actionRegistry, _cueFactory),
                EScheduleMode.Periodic => new PeriodicTriggerBehavior(planConfig, this, _valueResolver, _actionRegistry, _cueFactory),
                _ => throw new ArgumentException($"Unsupported schedule mode: {schedule.Mode}")
            };
        }

        public ISimpleTriggerBehavior CreateSimple(ITriggerPlanConfig planConfig)
        {
            return new SimpleTriggerBehavior(planConfig, this, _valueResolver, _actionRegistry, _cueFactory);
        }

        public IConditionalBehavior CreatePredicate(IPredicateConfig predicateConfig)
        {
            if (predicateConfig == null || predicateConfig.IsEmpty)
                return NullConditionalBehavior.Instance;

            return predicateConfig.Kind switch
            {
                EPredicateKind.Function => CreateFunctionPredicate((FunctionPredicateConfig)predicateConfig),
                EPredicateKind.Expression => CreateExpressionPredicate((ExpressionPredicateConfig)predicateConfig),
                EPredicateKind.Blackboard => CreateBlackboardPredicate((FunctionPredicateConfig)predicateConfig),
                _ => NullConditionalBehavior.Instance
            };
        }

        public IActionBehavior CreateAction(IActionCallConfig actionConfig)
        {
            return new ActionBehavior(actionConfig, _valueResolver, _actionRegistry);
        }

        private IConditionalBehavior CreateFunctionPredicate(FunctionPredicateConfig config)
        {
            return new FunctionPredicateBehavior(config, _valueResolver, _actionRegistry);
        }

        private IConditionalBehavior CreateExpressionPredicate(ExpressionPredicateConfig config)
        {
            return new ExpressionPredicateBehavior(config, _valueResolver);
        }

        private IConditionalBehavior CreateBlackboardPredicate(FunctionPredicateConfig config)
        {
            return new BlackboardPredicateBehavior(config, _valueResolver);
        }

        public List<ITriggerBehavior> CreateActions(IReadOnlyList<IActionCallConfig> actions)
        {
            var behaviors = new List<ITriggerBehavior>(actions.Count);
            foreach (var action in actions)
            {
                behaviors.Add(new ActionBehavior(action, _valueResolver, _actionRegistry));
            }
            return behaviors;
        }
    }

    /// <summary>
    /// 值解析器实现
    /// </summary>
    public class ValueResolver : IValueResolver
    {
        public double Resolve(IValueRefConfig valueRef, IBehaviorContext context)
        {
            if (valueRef == null) throw new ArgumentNullException(nameof(valueRef));
            if (context == null) throw new ArgumentNullException(nameof(context));

            return valueRef.Kind switch
            {
                EValueRefKind.Const => valueRef.ConstValue,
                EValueRefKind.Blackboard => ResolveBlackboard(valueRef, context),
                EValueRefKind.PayloadField => ResolvePayloadField(valueRef, context),
                EValueRefKind.Var => ResolveVar(valueRef, context),
                EValueRefKind.Expr => ResolveExpr(valueRef, context),
                EValueRefKind.ContextField => ResolvePayloadField(valueRef, context),
                _ => throw new NotSupportedException($"Unsupported value reference kind: {valueRef.Kind}")
            };
        }

        private double ResolveBlackboard(IValueRefConfig valueRef, IBehaviorContext context)
        {
            if (context.Blackboards == null)
                throw new InvalidOperationException($"Blackboard resolver is not available for boardId={valueRef.BlackboardId}, key='{valueRef.BlackboardKey}'.");

            if (context.Blackboards.TryGetValue<double>(valueRef.BlackboardId, valueRef.BlackboardKey, out var value))
                return value;

            throw new InvalidOperationException($"Blackboard value not found. boardId={valueRef.BlackboardId}, key='{valueRef.BlackboardKey}'.");
        }

        private double ResolvePayloadField(IValueRefConfig valueRef, IBehaviorContext context)
        {
            if (context.Args is IHasPayload payload && payload.TryGetPayloadDouble(valueRef.PayloadFieldId, out var value))
                return value;

            throw new InvalidOperationException($"Payload field value not found or payload access is not available. fieldId={valueRef.PayloadFieldId}.");
        }

        private double ResolveVar(IValueRefConfig valueRef, IBehaviorContext context)
        {
            if (TryResolveVar(context, valueRef.DomainId, valueRef.BlackboardKey, out var value))
                return value;

            throw new InvalidOperationException($"Numeric var not found. domainId='{valueRef.DomainId}', key='{valueRef.BlackboardKey}'.");
        }

        private double ResolveExpr(IValueRefConfig valueRef, IBehaviorContext context)
        {
            if (string.IsNullOrEmpty(valueRef.ExprText))
                throw new InvalidOperationException("Expression value reference has empty ExprText.");

            if (!NumericExpressionCompiler.TryCompileCached(valueRef.ExprText, out var program) || program == null)
                throw new InvalidOperationException("Expression compile failed: " + valueRef.ExprText);

            if (NumericRpnTokenEvaluator.TryEvaluate(
                program,
                (string domainId, string key, out double value) => TryResolveVar(context, domainId, key, out value),
                DefaultNumericRpnFunctionRegistry.Instance,
                out var result))
            {
                return result;
            }

            throw new InvalidOperationException("Expression evaluate failed: " + valueRef.ExprText);
        }

        private static bool TryResolveVar(IBehaviorContext context, string domainId, string key, out double value)
        {
            value = 0.0;
            if (string.IsNullOrEmpty(domainId) || string.IsNullOrEmpty(key))
                return false;

            if (context is IVarResolvable contextResolvable)
                return contextResolvable.TryResolveVarValue(domainId, key, out value);

            if (context.Args is IVarResolvable argsResolvable)
                return argsResolvable.TryResolveVarValue(domainId, key, out value);

            return false;
        }
    }

    /// <summary>
    /// 条件行为解析器接口。
    /// </summary>
    public interface IConditionalBehaviorResolver
    {
        IConditionalBehavior Resolve(IPredicateConfig config);
    }

    /// <summary>
    /// 触发器 Cue 工厂接口。
    /// </summary>
    public interface ITriggerCueFactory
    {
        ITriggerCue Create(in TriggerCueDescriptor descriptor);

        ITriggerCue Create(ICueConfig cueConfig)
        {
            var descriptor = TriggerCueDescriptor.FromConfig(cueConfig);
            return Create(in descriptor);
        }
    }
}

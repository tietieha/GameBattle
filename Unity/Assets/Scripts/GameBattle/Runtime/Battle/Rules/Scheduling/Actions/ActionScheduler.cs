using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.Executable;
using AbilityKit.Triggering.Runtime.Config;
using AbilityKit.Triggering.Runtime.Plan;
using AbilityKit.Triggering.Runtime.Dispatcher;
using AbilityKit.Triggering.Runtime.Pooling;

namespace AbilityKit.Triggering.Runtime.ActionScheduler
{
    /// <summary>
    /// Action 调度主线入口。
    /// 按触发器维度管理已注册 Action 的生命周期、延迟/周期执行、条件等待与中断清理。
    /// 与 Runtime/Schedule 的通用业务调度分离；新触发器 Action 调度逻辑应优先接入此体系。
    /// </summary>
    public sealed class ActionScheduler
    {
        private readonly List<ActionInstance> _actions = new();
        private readonly Dictionary<int, ActionInstance> _instancesById = new();
        private readonly Dictionary<int, ActionInstance> _instancesByPlanIndex = new();
        private readonly Dictionary<int, int> _planIndexByInstanceId = new();
        private readonly TriggeringRuntimePools _pools;
        private int _nextInstanceId;
        private bool _isActive = true;
        private float _elapsedMs;

        public int ActionCount => _actions.Count;
        public int ActiveCount { get; private set; }
        public bool IsActive => _isActive;
        public float ElapsedMs => _elapsedMs;

        /// <summary>
        /// 创建指定触发器的 Action 调度器。
        /// </summary>
        public ActionScheduler(int triggerId, TriggeringRuntimePools pools = null)
        {
            TriggerId = triggerId;
            _pools = pools;
        }

        public int TriggerId { get; }

        internal TriggeringRuntimePools Pools => _pools;

        /// <summary>
        /// 注册单个 Action 实例；通常由触发器激活时调用。
        /// </summary>
        public ActionInstance Register(
            ActionCallPlan plan,
            Action<object, ITriggerDispatcherContext> actionDelegate,
            TriggerPredicate<object> conditionDelegate,
            object boundArgs,
            IActionExecutor executor,
            bool ownsExecutor = false)
        {
            if (!_isActive) throw new InvalidOperationException("ActionScheduler 已停用，无法注册新的 Action。");
            if (executor == null && actionDelegate == null) throw new ArgumentNullException(nameof(actionDelegate));

            var effectiveExecutor = executor;
            var effectiveOwnsExecutor = ownsExecutor;
            if (effectiveExecutor == null)
            {
                effectiveExecutor = CreateDefaultExecutor(actionDelegate, out effectiveOwnsExecutor);
            }

            var instanceId = _nextInstanceId++;
            var instance = _pools != null
                ? _pools.RentActionInstance(instanceId, TriggerId, in plan, effectiveExecutor, boundArgs, _elapsedMs, effectiveOwnsExecutor)
                : new ActionInstance(
                    instanceId: instanceId,
                    triggerId: TriggerId,
                    plan: plan,
                    executor: effectiveExecutor,
                    globalContext: boundArgs,
                    createdAtMs: _elapsedMs)
                {
                    ActionDelegate = actionDelegate,
                    ConditionDelegate = conditionDelegate,
                    BoundArgs = boundArgs
                };

            if (_pools != null)
            {
                instance.ActionDelegate = actionDelegate;
                instance.ConditionDelegate = conditionDelegate;
                instance.BoundArgs = boundArgs;
            }

            _actions.Add(instance);
            _instancesById[instance.InstanceId] = instance;
            ActiveCount++;

            return instance;
        }

        /// <summary>
        /// 按计划索引注册或替换 Action，确保同一触发器的同一计划槽位只保留一个活跃实例。
        /// </summary>
        public ActionInstance RegisterOrReplace(
            int planIndex,
            ActionCallPlan plan,
            Action<object, ITriggerDispatcherContext> actionDelegate,
            TriggerPredicate<object> conditionDelegate,
            object boundArgs,
            IActionExecutor executor,
            bool ownsExecutor = false)
        {
            if (planIndex < 0) throw new ArgumentOutOfRangeException(nameof(planIndex));

            if (_instancesByPlanIndex.TryGetValue(planIndex, out var existing))
            {
                existing.RequestInterrupt($"被计划索引 {planIndex} 的新实例替换。");
                RemoveInstance(existing);
            }

            var instance = Register(plan, actionDelegate, conditionDelegate, boundArgs, executor, ownsExecutor);
            _instancesByPlanIndex[planIndex] = instance;
            _planIndexByInstanceId[instance.InstanceId] = planIndex;
            return instance;
        }

        /// <summary>
        /// 批量注册 Action；按数组索引作为计划索引，复用 RegisterOrReplace 的替换语义。
        /// </summary>
        public void RegisterRange(ActionCallPlan[] plans, Action<object, ITriggerDispatcherContext>[] actionDelegates, TriggerPredicate<object>[] conditionDelegates, object boundArgs)
        {
            if (plans == null) throw new ArgumentNullException(nameof(plans));
            if (actionDelegates == null) throw new ArgumentNullException(nameof(actionDelegates));
            if (actionDelegates.Length < plans.Length) throw new ArgumentException("Action 委托数组长度不能小于计划数组长度。", nameof(actionDelegates));
            if (conditionDelegates != null && conditionDelegates.Length < plans.Length) throw new ArgumentException("条件委托数组长度不能小于计划数组长度。", nameof(conditionDelegates));

            for (int i = 0; i < plans.Length; i++)
            {
                var executor = CreateExecutor(in plans[i], actionDelegates[i], out var ownsExecutor);
                RegisterOrReplace(i, plans[i], actionDelegates[i], conditionDelegates?[i], boundArgs, executor, ownsExecutor);
            }
        }

        /// <summary>
        /// 每帧更新，由 ActionSchedulerManager 调用。
        /// </summary>
        /// <param name="deltaTimeMs">帧间隔，单位毫秒。</param>
        /// <param name="ctx">当前调度执行上下文。</param>
        public void Update(float deltaTimeMs, ActionExecutionContext ctx)
        {
            if (!_isActive) return;

            _elapsedMs += Math.Max(0f, deltaTimeMs);

            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                var action = _actions[i];

                if (!action.IsActive)
                {
                    RemoveInstanceAt(i, action);
                    continue;
                }

                try
                {
                    var actionCtx = new ActionExecutionContext(
                        instance: action,
                        globalContext: ctx.GlobalContext,
                        dispatcherContext: ctx.DispatcherContext,
                        control: ctx.Control);

                    action.Update(deltaTimeMs, actionCtx);
                }
                catch (Exception ex)
                {
                    MarkActionFailed(action, ex);
                }
            }
        }

        internal IActionExecutor CreateExecutor(in ActionCallPlan plan, Action<object, ITriggerDispatcherContext> action, out bool ownsExecutor)
        {
            var baseExecutor = CreateDefaultExecutor(action, out ownsExecutor);
            var execution = plan.Execution;

            switch (execution.Policy)
            {
                case EActionExecutionPolicy.Queued:
                    if (_pools != null && baseExecutor is ActionExecutorBase pooledQueuedInner)
                    {
                        ownsExecutor = true;
                        return _pools.RentQueuedActionExecutor(pooledQueuedInner);
                    }

                    ownsExecutor = false;
                    return new QueuedActionExecutor((ActionExecutorBase)baseExecutor);

                case EActionExecutionPolicy.WithRetry:
                    if (_pools != null && baseExecutor is ActionExecutorBase pooledRetryInner)
                    {
                        ownsExecutor = true;
                        return _pools.RentRetryActionExecutor(pooledRetryInner, execution.RetryMaxRetries, execution.RetryDelayMs);
                    }

                    ownsExecutor = false;
                    return new RetryActionExecutor((ActionExecutorBase)baseExecutor, execution.RetryMaxRetries, execution.RetryDelayMs);

                case EActionExecutionPolicy.Parallel:
                case EActionExecutionPolicy.Conditional:
                default:
                    return baseExecutor;
            }
        }

        private DefaultActionExecutor CreateDefaultExecutor(Action<object, ITriggerDispatcherContext> action, out bool ownsExecutor)
        {
            if (_pools != null)
            {
                ownsExecutor = true;
                return _pools.RentDefaultActionExecutor(action);
            }

            ownsExecutor = false;
            return new DefaultActionExecutor(action);
        }

        private void MarkActionFailed(ActionInstance action, Exception ex)
        {
            action.State = EActionInstanceState.Failed;
            action.InterruptReason = ex.Message;
            action.Executor.Cancel($"Action 调度更新异常：{ex.Message}");
        }

        private void RemoveInstance(ActionInstance action)
        {
            var index = _actions.IndexOf(action);
            if (index >= 0)
            {
                RemoveInstanceAt(index, action);
                return;
            }

            RemoveInstanceIndexes(action);
            ReleaseInstance(action);
        }

        private void RemoveInstanceAt(int index, ActionInstance action)
        {
            _actions.RemoveAt(index);
            RemoveInstanceIndexes(action);
            if (ActiveCount > 0)
            {
                ActiveCount--;
            }

            ReleaseInstance(action);
        }

        private void RemoveInstanceIndexes(ActionInstance action)
        {
            _instancesById.Remove(action.InstanceId);
            if (_planIndexByInstanceId.TryGetValue(action.InstanceId, out var planIndex))
            {
                if (_instancesByPlanIndex.TryGetValue(planIndex, out var indexed) && ReferenceEquals(indexed, action))
                {
                    _instancesByPlanIndex.Remove(planIndex);
                }

                _planIndexByInstanceId.Remove(action.InstanceId);
            }
        }

        private void ReleaseInstance(ActionInstance action)
        {
            if (_pools != null)
            {
                _pools.ReleaseActionInstance(action);
            }
        }

        /// <summary>
        /// 获取指定实例。
        /// </summary>
        public ActionInstance GetInstance(int instanceId)
        {
            _instancesById.TryGetValue(instanceId, out var instance);
            return instance;
        }

        /// <summary>
        /// 获取全部当前保留的实例。
        /// </summary>
        public IReadOnlyList<ActionInstance> GetAllInstances() => _actions;

        /// <summary>
        /// 请求中断所有可中断的 Action。
        /// </summary>
        public void InterruptAll(string reason)
        {
            foreach (var action in _actions)
            {
                if (action.CanBeInterrupted)
                {
                    action.RequestInterrupt(reason);
                }
            }
        }

        /// <summary>
        /// 暂停调度器及当前正在执行的 Action。
        /// </summary>
        public void PauseAll()
        {
            _isActive = false;
            foreach (var action in _actions)
            {
                if (action.State == EActionInstanceState.Executing)
                {
                    action.State = EActionInstanceState.WaitingDelay;
                }
            }
        }

        /// <summary>
        /// 恢复调度器，并恢复连续型等待实例。
        /// </summary>
        public void ResumeAll()
        {
            _isActive = true;
            foreach (var action in _actions)
            {
                if (action.State == EActionInstanceState.WaitingDelay && action.Plan.Schedule.Mode == EActionScheduleMode.Continuous)
                {
                    action.State = EActionInstanceState.Executing;
                }
            }
        }

        /// <summary>
        /// 销毁调度器并清空所有实例索引。
        /// </summary>
        public void Dispose()
        {
            _isActive = false;
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                ReleaseInstance(_actions[i]);
            }

            _actions.Clear();
            _instancesById.Clear();
            _instancesByPlanIndex.Clear();
            _planIndexByInstanceId.Clear();
            ActiveCount = 0;
        }
    }

    /// <summary>
    /// 默认 Action 执行器，直接调用已绑定的调度委托。
    /// </summary>
    internal sealed class DefaultActionExecutor : ActionExecutorBase
    {
        private Action<object, ITriggerDispatcherContext> _action;

        internal DefaultActionExecutor()
        {
        }

        public DefaultActionExecutor(Action<object, ITriggerDispatcherContext> action)
        {
            Initialize(action);
        }

        internal void Initialize(Action<object, ITriggerDispatcherContext> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        protected override ExecutionResult ExecuteCore(ActionExecutionContext ctx)
        {
            if (_action == null)
            {
                return ExecutionResult.Failed("Default action executor is not initialized.");
            }

            try
            {
                _action(ctx.Instance.BoundArgs, ctx.DispatcherContext);
                return ExecutionResult.Success();
            }
            catch (Exception ex)
            {
                return ExecutionResult.Failed($"Action execution error: {ex.Message}");
            }
        }

        internal override void ResetForPool()
        {
            _action = null;
        }
    }
}

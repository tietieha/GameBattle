using System;
using System.Collections.Generic;
using AbilityKit.Core.Pooling;
using AbilityKit.Triggering.Runtime.ActionScheduler;
using AbilityKit.Triggering.Runtime.Dispatcher;
using AbilityKit.Triggering.Runtime.Plan;
using AbilityKit.Triggering.Runtime.Schedule.Behavior;
using AbilityKit.Triggering.Runtime.Schedule.Data;

namespace AbilityKit.Triggering.Runtime.Pooling
{
    /// <summary>
    /// Triggering 运行时对象池门面。
    ///
    /// 该类型只管理 Triggering 内部短生命周期对象，不改变触发器、计划和调度的正式语义。
    /// 推荐按世界、战斗或玩法域创建独立实例，并在生命周期结束时 Dispose。
    /// </summary>
    public sealed class TriggeringRuntimePools : IDisposable
    {
        public const string DefaultScopeName = "triggering-runtime";

        private static readonly PoolKey ActionInstanceKey = new PoolKey("triggering.action-instance");
        private static readonly PoolKey DefaultExecutorKey = new PoolKey("triggering.action-executor.default");
        private static readonly PoolKey QueuedExecutorKey = new PoolKey("triggering.action-executor.queued");
        private static readonly PoolKey RetryExecutorKey = new PoolKey("triggering.action-executor.retry");
        private static readonly PoolKey ScheduleToBehaviorContextAdapterKey = new PoolKey("triggering.schedule.behavior-context-adapter");
        private static readonly PoolKey IntListKey = new PoolKey("triggering.temp.int-list");
        private static readonly PoolKey IntHashSetKey = new PoolKey("triggering.temp.int-hashset");
        private static readonly PoolKey ScheduleItemListKey = new PoolKey("triggering.temp.schedule-item-list");
        private static readonly PoolKey ScheduleEffectListKey = new PoolKey("triggering.temp.schedule-effect-list");
        private static readonly PoolKey ScheduleEffectCallbackListKey = new PoolKey("triggering.temp.schedule-effect-callback-list");
        private static readonly PoolKey InstanceKeyListKey = new PoolKey("triggering.temp.instance-key-list");

        private readonly bool _ownsScope;
        private bool _disposed;

        public TriggeringRuntimePools(PoolScope scope = null, bool ownsScope = false, TriggeringPoolProfile profile = default)
        {
            Scope = scope ?? new PoolScope(DefaultScopeName);
            _ownsScope = scope == null || ownsScope;
            Profile = profile.IsSpecified ? profile : TriggeringPoolProfile.Default;
        }

        public PoolScope Scope { get; }

        public TriggeringPoolProfile Profile { get; }

        public static TriggeringRuntimePools CreateDefault(string scopeName = DefaultScopeName, TriggeringPoolProfile profile = default)
        {
            return new TriggeringRuntimePools(new PoolScope(scopeName), ownsScope: true, profile: profile);
        }

        public void PrewarmActionScheduler()
        {
            ThrowIfDisposed();
            GetActionInstancePool().Prewarm(Profile.ActionInstancePrewarmCount);
            GetDefaultExecutorPool().Prewarm(Profile.DefaultExecutorPrewarmCount);
            GetQueuedExecutorPool().Prewarm(Profile.QueuedExecutorPrewarmCount);
            GetRetryExecutorPool().Prewarm(Profile.RetryExecutorPrewarmCount);
        }

        public void PrewarmScheduleAdapters()
        {
            ThrowIfDisposed();
            GetScheduleToBehaviorContextAdapterPool().Prewarm(Profile.ScheduleAdapterPrewarmCount);
        }

        public void PrewarmTemporaryCollections()
        {
            ThrowIfDisposed();
            GetIntListPool().Prewarm(Profile.IntListPrewarmCount);
            GetIntHashSetPool().Prewarm(Profile.IntHashSetPrewarmCount);
            GetScheduleItemListPool().Prewarm(Profile.ScheduleItemListPrewarmCount);
            GetScheduleEffectListPool().Prewarm(Profile.ScheduleEffectListPrewarmCount);
            GetScheduleEffectCallbackListPool().Prewarm(Profile.ScheduleEffectCallbackListPrewarmCount);
            GetInstanceKeyListPool().Prewarm(Profile.InstanceKeyListPrewarmCount);
        }

        public void PrewarmAll()
        {
            PrewarmActionScheduler();
            PrewarmScheduleAdapters();
            PrewarmTemporaryCollections();
        }

        public int TrimAll()
        {
            ThrowIfDisposed();
            return Scope.TrimAll();
        }

        public int ForceTrimAll(PoolTrimPolicy policy)
        {
            ThrowIfDisposed();
            return Scope.ForceTrimAll(policy);
        }

#if UNITY_EDITOR
        public System.Collections.Generic.IReadOnlyList<PoolDebugSnapshot> GetDebugSnapshots()
        {
            ThrowIfDisposed();
            return Scope.GetDebugSnapshots();
        }
#endif

        internal ActionInstance RentActionInstance(
            int instanceId,
            int triggerId,
            in ActionCallPlan plan,
            IActionExecutor executor,
            object globalContext,
            float createdAtMs,
            bool ownsExecutor)
        {
            ThrowIfDisposed();
            var instance = GetActionInstancePool().Get();
            instance.Initialize(instanceId, triggerId, in plan, executor, globalContext, createdAtMs, ownsExecutor);
            return instance;
        }

        internal DefaultActionExecutor RentDefaultActionExecutor(Action<object, ITriggerDispatcherContext> action)
        {
            ThrowIfDisposed();
            var executor = GetDefaultExecutorPool().Get();
            executor.Initialize(action);
            return executor;
        }

        internal QueuedActionExecutor RentQueuedActionExecutor(ActionExecutorBase inner, int queuePriority = 0)
        {
            ThrowIfDisposed();
            var executor = GetQueuedExecutorPool().Get();
            executor.Initialize(inner, queuePriority);
            return executor;
        }

        internal RetryActionExecutor RentRetryActionExecutor(ActionExecutorBase inner, int maxRetries = 3, float retryDelayMs = 0f)
        {
            ThrowIfDisposed();
            var executor = GetRetryExecutorPool().Get();
            executor.Initialize(inner, maxRetries, retryDelayMs);
            return executor;
        }

        internal void ReleaseActionInstance(ActionInstance instance)
        {
            if (instance == null) return;
            ThrowIfDisposed();

            var executor = instance.Executor;
            var ownsExecutor = instance.OwnsExecutor;
            instance.ResetForPool();
            GetActionInstancePool().Release(instance);

            if (ownsExecutor)
            {
                ReleaseActionExecutor(executor);
            }
        }

        internal ScheduleToBehaviorContextAdapter RentScheduleToBehaviorContextAdapter(
            in ScheduleContext scheduleContext,
            AbilityKit.Triggering.Runtime.Behavior.IBehaviorContext innerContext = null,
            object args = null)
        {
            ThrowIfDisposed();
            var adapter = GetScheduleToBehaviorContextAdapterPool().Get();
            adapter.Initialize(in scheduleContext, innerContext, args);
            return adapter;
        }

        internal void ReleaseScheduleToBehaviorContextAdapter(ScheduleToBehaviorContextAdapter adapter)
        {
            if (adapter == null) return;
            ThrowIfDisposed();
            adapter.ResetForPool();
            GetScheduleToBehaviorContextAdapterPool().Release(adapter);
        }

        internal List<int> RentIntList()
        {
            ThrowIfDisposed();
            var list = GetIntListPool().Get();
            list.Clear();
            return list;
        }

        internal void ReleaseIntList(List<int> list)
        {
            if (list == null) return;
            ThrowIfDisposed();
            list.Clear();
            GetIntListPool().Release(list);
        }

        internal HashSet<int> RentIntHashSet()
        {
            ThrowIfDisposed();
            var set = GetIntHashSetPool().Get();
            set.Clear();
            return set;
        }

        internal void ReleaseIntHashSet(HashSet<int> set)
        {
            if (set == null) return;
            ThrowIfDisposed();
            set.Clear();
            GetIntHashSetPool().Release(set);
        }

        internal List<ScheduleItemData> RentScheduleItemList()
        {
            ThrowIfDisposed();
            var list = GetScheduleItemListPool().Get();
            list.Clear();
            return list;
        }

        internal void ReleaseScheduleItemList(List<ScheduleItemData> list)
        {
            if (list == null) return;
            ThrowIfDisposed();
            list.Clear();
            GetScheduleItemListPool().Release(list);
        }

        internal List<IScheduleEffect> RentScheduleEffectList()
        {
            ThrowIfDisposed();
            var list = GetScheduleEffectListPool().Get();
            list.Clear();
            return list;
        }

        internal void ReleaseScheduleEffectList(List<IScheduleEffect> list)
        {
            if (list == null) return;
            ThrowIfDisposed();
            list.Clear();
            GetScheduleEffectListPool().Release(list);
        }

        internal List<IScheduleEffectCallbacks> RentScheduleEffectCallbackList()
        {
            ThrowIfDisposed();
            var list = GetScheduleEffectCallbackListPool().Get();
            list.Clear();
            return list;
        }

        internal void ReleaseScheduleEffectCallbackList(List<IScheduleEffectCallbacks> list)
        {
            if (list == null) return;
            ThrowIfDisposed();
            list.Clear();
            GetScheduleEffectCallbackListPool().Release(list);
        }

        internal List<(int, int)> RentInstanceKeyList()
        {
            ThrowIfDisposed();
            var list = GetInstanceKeyListPool().Get();
            list.Clear();
            return list;
        }

        internal void ReleaseInstanceKeyList(List<(int, int)> list)
        {
            if (list == null) return;
            ThrowIfDisposed();
            list.Clear();
            GetInstanceKeyListPool().Release(list);
        }

        internal void ReleaseActionExecutor(IActionExecutor executor)
        {
            if (executor == null) return;
            ThrowIfDisposed();

            switch (executor)
            {
                case RetryActionExecutor retry:
                {
                    var inner = retry.Inner;
                    retry.ResetForPool();
                    GetRetryExecutorPool().Release(retry);
                    ReleaseActionExecutor(inner);
                    break;
                }
                case QueuedActionExecutor queued:
                {
                    var inner = queued.Inner;
                    queued.ResetForPool();
                    GetQueuedExecutorPool().Release(queued);
                    ReleaseActionExecutor(inner);
                    break;
                }
                case DefaultActionExecutor defaultExecutor:
                    defaultExecutor.ResetForPool();
                    GetDefaultExecutorPool().Release(defaultExecutor);
                    break;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_ownsScope)
            {
                Scope.Dispose();
            }
            else
            {
                Scope.Clear(destroy: false);
            }
        }

        private ObjectPool<ActionInstance> GetActionInstancePool()
        {
            return Scope.GetPool(ActionInstanceKey, () => new ActionInstance(), Profile.ActionInstanceConfig);
        }

        private ObjectPool<DefaultActionExecutor> GetDefaultExecutorPool()
        {
            return Scope.GetPool(DefaultExecutorKey, () => new DefaultActionExecutor(), Profile.DefaultExecutorConfig);
        }

        private ObjectPool<QueuedActionExecutor> GetQueuedExecutorPool()
        {
            return Scope.GetPool(QueuedExecutorKey, () => new QueuedActionExecutor(), Profile.QueuedExecutorConfig);
        }

        private ObjectPool<RetryActionExecutor> GetRetryExecutorPool()
        {
            return Scope.GetPool(RetryExecutorKey, () => new RetryActionExecutor(), Profile.RetryExecutorConfig);
        }

        private ObjectPool<ScheduleToBehaviorContextAdapter> GetScheduleToBehaviorContextAdapterPool()
        {
            return Scope.GetPool(ScheduleToBehaviorContextAdapterKey, () => new ScheduleToBehaviorContextAdapter(), Profile.ScheduleAdapterConfig);
        }

        private ObjectPool<List<int>> GetIntListPool()
        {
            return Scope.GetPool(IntListKey, () => new List<int>(16), Profile.IntListConfig);
        }

        private ObjectPool<HashSet<int>> GetIntHashSetPool()
        {
            return Scope.GetPool(IntHashSetKey, () => new HashSet<int>(), Profile.IntHashSetConfig);
        }

        private ObjectPool<List<ScheduleItemData>> GetScheduleItemListPool()
        {
            return Scope.GetPool(ScheduleItemListKey, () => new List<ScheduleItemData>(16), Profile.ScheduleItemListConfig);
        }

        private ObjectPool<List<IScheduleEffect>> GetScheduleEffectListPool()
        {
            return Scope.GetPool(ScheduleEffectListKey, () => new List<IScheduleEffect>(16), Profile.ScheduleEffectListConfig);
        }

        private ObjectPool<List<IScheduleEffectCallbacks>> GetScheduleEffectCallbackListPool()
        {
            return Scope.GetPool(ScheduleEffectCallbackListKey, () => new List<IScheduleEffectCallbacks>(16), Profile.ScheduleEffectCallbackListConfig);
        }

        private ObjectPool<List<(int, int)>> GetInstanceKeyListPool()
        {
            return Scope.GetPool(InstanceKeyListKey, () => new List<(int, int)>(16), Profile.InstanceKeyListConfig);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TriggeringRuntimePools));
        }
    }

    /// <summary>
    /// Triggering 默认对象池配置档。
    /// </summary>
    public readonly struct TriggeringPoolProfile
    {
        private readonly bool _specified;

        public readonly PoolItemConfig ActionInstanceConfig;
        public readonly PoolItemConfig DefaultExecutorConfig;
        public readonly PoolItemConfig QueuedExecutorConfig;
        public readonly PoolItemConfig RetryExecutorConfig;
        public readonly PoolItemConfig ScheduleAdapterConfig;
        public readonly PoolItemConfig IntListConfig;
        public readonly PoolItemConfig IntHashSetConfig;
        public readonly PoolItemConfig ScheduleItemListConfig;
        public readonly PoolItemConfig ScheduleEffectListConfig;
        public readonly PoolItemConfig ScheduleEffectCallbackListConfig;
        public readonly PoolItemConfig InstanceKeyListConfig;

        public TriggeringPoolProfile(
            PoolItemConfig actionInstanceConfig,
            PoolItemConfig defaultExecutorConfig,
            PoolItemConfig queuedExecutorConfig,
            PoolItemConfig retryExecutorConfig,
            PoolItemConfig scheduleAdapterConfig,
            PoolItemConfig intListConfig,
            PoolItemConfig intHashSetConfig,
            PoolItemConfig scheduleItemListConfig,
            PoolItemConfig scheduleEffectListConfig,
            PoolItemConfig scheduleEffectCallbackListConfig,
            PoolItemConfig instanceKeyListConfig)
        {
            _specified = true;
            ActionInstanceConfig = actionInstanceConfig;
            DefaultExecutorConfig = defaultExecutorConfig;
            QueuedExecutorConfig = queuedExecutorConfig;
            RetryExecutorConfig = retryExecutorConfig;
            ScheduleAdapterConfig = scheduleAdapterConfig;
            IntListConfig = intListConfig;
            IntHashSetConfig = intHashSetConfig;
            ScheduleItemListConfig = scheduleItemListConfig;
            ScheduleEffectListConfig = scheduleEffectListConfig;
            ScheduleEffectCallbackListConfig = scheduleEffectCallbackListConfig;
            InstanceKeyListConfig = instanceKeyListConfig;
        }

        public bool IsSpecified => _specified;

        public int ActionInstancePrewarmCount => ActionInstanceConfig.PrewarmCount;
        public int DefaultExecutorPrewarmCount => DefaultExecutorConfig.PrewarmCount;
        public int QueuedExecutorPrewarmCount => QueuedExecutorConfig.PrewarmCount;
        public int RetryExecutorPrewarmCount => RetryExecutorConfig.PrewarmCount;
        public int ScheduleAdapterPrewarmCount => ScheduleAdapterConfig.PrewarmCount;
        public int IntListPrewarmCount => IntListConfig.PrewarmCount;
        public int IntHashSetPrewarmCount => IntHashSetConfig.PrewarmCount;
        public int ScheduleItemListPrewarmCount => ScheduleItemListConfig.PrewarmCount;
        public int ScheduleEffectListPrewarmCount => ScheduleEffectListConfig.PrewarmCount;
        public int ScheduleEffectCallbackListPrewarmCount => ScheduleEffectCallbackListConfig.PrewarmCount;
        public int InstanceKeyListPrewarmCount => InstanceKeyListConfig.PrewarmCount;

        public static TriggeringPoolProfile Default => new TriggeringPoolProfile(
            PoolItemConfig.Default(defaultCapacity: 32, maxSize: 4096, prewarmCount: 32),
            PoolItemConfig.Default(defaultCapacity: 32, maxSize: 4096, prewarmCount: 32),
            PoolItemConfig.Default(defaultCapacity: 8, maxSize: 1024, prewarmCount: 8),
            PoolItemConfig.Default(defaultCapacity: 8, maxSize: 1024, prewarmCount: 8),
            PoolItemConfig.Default(defaultCapacity: 32, maxSize: 4096, prewarmCount: 32),
            PoolItemConfig.Default(defaultCapacity: 32, maxSize: 4096, prewarmCount: 32),
            PoolItemConfig.Default(defaultCapacity: 16, maxSize: 2048, prewarmCount: 16),
            PoolItemConfig.Default(defaultCapacity: 16, maxSize: 2048, prewarmCount: 16),
            PoolItemConfig.Default(defaultCapacity: 16, maxSize: 2048, prewarmCount: 16),
            PoolItemConfig.Default(defaultCapacity: 16, maxSize: 2048, prewarmCount: 16),
            PoolItemConfig.Default(defaultCapacity: 16, maxSize: 2048, prewarmCount: 16));

        public static TriggeringPoolProfile Small => new TriggeringPoolProfile(
            PoolItemConfig.Default(defaultCapacity: 4, maxSize: 256, prewarmCount: 4),
            PoolItemConfig.Default(defaultCapacity: 4, maxSize: 256, prewarmCount: 4),
            PoolItemConfig.Default(defaultCapacity: 2, maxSize: 128, prewarmCount: 2),
            PoolItemConfig.Default(defaultCapacity: 2, maxSize: 128, prewarmCount: 2),
            PoolItemConfig.Default(defaultCapacity: 4, maxSize: 256, prewarmCount: 4),
            PoolItemConfig.Default(defaultCapacity: 4, maxSize: 256, prewarmCount: 4),
            PoolItemConfig.Default(defaultCapacity: 4, maxSize: 256, prewarmCount: 4),
            PoolItemConfig.Default(defaultCapacity: 4, maxSize: 256, prewarmCount: 4),
            PoolItemConfig.Default(defaultCapacity: 4, maxSize: 256, prewarmCount: 4),
            PoolItemConfig.Default(defaultCapacity: 4, maxSize: 256, prewarmCount: 4),
            PoolItemConfig.Default(defaultCapacity: 4, maxSize: 256, prewarmCount: 4));
    }
}

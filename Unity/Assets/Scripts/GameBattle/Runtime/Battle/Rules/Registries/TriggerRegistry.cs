using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Behavior;
using AbilityKit.Triggering.Runtime.Config.Actions;
using AbilityKit.Triggering.Runtime.Config.Cue;
using AbilityKit.Triggering.Runtime.Config.Plans;
using AbilityKit.Triggering.Runtime.Config.Predicates;
using AbilityKit.Triggering.Runtime.Config.Schedule;
using AbilityKit.Triggering.Runtime.Instance;

namespace AbilityKit.Triggering.Runtime.Registry
{
    /// <summary>
    /// 触发器注册表接口。
    /// </summary>
    public interface ITriggerRegistry
    {
        int Count { get; }
        bool Unregister(int triggerId);
        bool TryGet(int triggerId, out ITriggerInstance trigger);
        IEnumerable<ITriggerInstance> GetAllTriggers();
        void Clear();
    }

    /// <summary>
    /// 触发器注册表。
    /// 保存静态配置、运行时实例和已编译触发器引用，避免注册阶段生成空壳实例。
    /// </summary>
    public sealed class TriggerRegistry : ITriggerRegistry, AbilityKit.Triggering.Runtime.ITriggerRegistry, IDisposable
    {
        private readonly Dictionary<int, Entry> _byId = new Dictionary<int, Entry>();
        private int _nextId = 1;
        private bool _disposed;

        public int Count => _byId.Count;

        /// <summary>
        /// 注册静态配置并创建正式运行时实例。
        /// </summary>
        public ITriggerHandle RegisterConfig(ITriggerPlanConfig config, long serverTime = 0, ITriggerBehavior behavior = null)
        {
            ThrowIfDisposed();
            if (config == null) throw new ArgumentNullException(nameof(config));

            var normalizedConfig = NormalizeConfig(config);
            var instance = new TriggerInstance(normalizedConfig, normalizedConfig.TriggerId, serverTime)
            {
                Behavior = behavior
            };

            return AddEntry(normalizedConfig.TriggerId, normalizedConfig, instance, null, null, null);
        }

        /// <summary>
        /// 注册已经编译好的触发器，保留对应配置和运行时实例。
        /// </summary>
        public ITriggerHandle RegisterCompiled<TArgs, TCtx>(
            ITriggerPlanConfig config,
            ITrigger<TArgs, TCtx> trigger,
            long serverTime = 0,
            ITriggerBehavior behavior = null)
            where TArgs : class
        {
            ThrowIfDisposed();
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            var normalizedConfig = NormalizeConfig(config);
            var instance = new TriggerInstance(normalizedConfig, normalizedConfig.TriggerId, serverTime)
            {
                Behavior = behavior
            };

            return AddEntry(normalizedConfig.TriggerId, normalizedConfig, instance, trigger, typeof(TArgs), typeof(TCtx));
        }

        /// <summary>
        /// 注册外部已经创建好的实例。
        /// </summary>
        public ITriggerHandle RegisterInstance(ITriggerInstance instance)
        {
            ThrowIfDisposed();
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (instance.Spec == null) throw new ArgumentException("Trigger instance must have a non-null Spec.", nameof(instance));

            var normalizedConfig = NormalizeConfig(instance.Spec);
            if (!ReferenceEquals(normalizedConfig, instance.Spec))
            {
                instance = CloneInstanceWithConfig(instance, normalizedConfig);
            }

            return AddEntry(normalizedConfig.TriggerId, normalizedConfig, instance, null, null, null);
        }

        public bool TryGet(int triggerId, out ITriggerInstance trigger)
        {
            if (_byId.TryGetValue(triggerId, out var entry))
            {
                trigger = entry.Instance;
                return true;
            }

            trigger = null;
            return false;
        }

        public bool TryGetConfig(int triggerId, out ITriggerPlanConfig config)
        {
            if (_byId.TryGetValue(triggerId, out var entry))
            {
                config = entry.Config;
                return true;
            }

            config = null;
            return false;
        }

        public bool TryGetCompiled<TArgs, TCtx>(int triggerId, out ITrigger<TArgs, TCtx> trigger)
            where TArgs : class
        {
            if (_byId.TryGetValue(triggerId, out var entry)
                && entry.CompiledTrigger is ITrigger<TArgs, TCtx> typed)
            {
                trigger = typed;
                return true;
            }

            trigger = null;
            return false;
        }

        public bool TryGetEntry(int triggerId, out TriggerRegistryEntry entry)
        {
            if (_byId.TryGetValue(triggerId, out var stored))
            {
                entry = stored.ToPublicEntry();
                return true;
            }

            entry = default;
            return false;
        }

        public IEnumerable<ITriggerInstance> GetAllTriggers()
        {
            foreach (var entry in _byId.Values)
            {
                yield return entry.Instance;
            }
        }

        public IEnumerable<TriggerRegistryEntry> GetAllEntries()
        {
            foreach (var entry in _byId.Values)
            {
                yield return entry.ToPublicEntry();
            }
        }

        public bool Unregister(int triggerId)
        {
            if (_byId.TryGetValue(triggerId, out var entry))
            {
                entry.Instance.Dispose();
                _byId.Remove(triggerId);
                return true;
            }

            return false;
        }

        public TriggerSnapshot CreateSnapshot(int triggerId)
        {
            if (!_byId.TryGetValue(triggerId, out var entry))
                throw new KeyNotFoundException($"Trigger not registered: {triggerId}");

            return entry.Instance.CreateSnapshot();
        }

        public bool TryRestoreSnapshot(TriggerSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (!_byId.TryGetValue(snapshot.TriggerId, out var entry))
                return false;

            entry.Instance.RestoreFromSnapshot(snapshot);
            return true;
        }

        public void Clear()
        {
            foreach (var entry in _byId.Values)
            {
                entry.Instance.Dispose();
            }

            _byId.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            Clear();
            _disposed = true;
        }

        private ITriggerHandle AddEntry(
            int triggerId,
            ITriggerPlanConfig config,
            ITriggerInstance instance,
            object compiledTrigger,
            Type argsType,
            Type contextType)
        {
            if (triggerId <= 0) throw new ArgumentOutOfRangeException(nameof(triggerId), "TriggerId must be positive.");
            if (_byId.ContainsKey(triggerId)) throw new InvalidOperationException($"Trigger already registered: {triggerId}");

            var entry = new Entry(config, instance, compiledTrigger, argsType, contextType);
            _byId.Add(triggerId, entry);
            if (triggerId >= _nextId) _nextId = triggerId + 1;
            return new TriggerHandle(triggerId, this);
        }

        private ITriggerPlanConfig NormalizeConfig(ITriggerPlanConfig config)
        {
            if (config.TriggerId > 0) return config;
            return new RegisteredTriggerPlanConfig(_nextId++, config);
        }

        private static ITriggerInstance CloneInstanceWithConfig(ITriggerInstance source, ITriggerPlanConfig config)
        {
            var clone = new TriggerInstance(config, config.TriggerId, source.StartServerTime)
            {
                CurrentState = source.CurrentState,
                ElapsedMs = source.ElapsedMs,
                ExecutionCount = source.ExecutionCount,
                Behavior = source.Behavior
            };

            if (source.InstanceData != null)
            {
                foreach (var kvp in source.InstanceData)
                {
                    clone.SetInstanceData(kvp.Key, kvp.Value);
                }
            }

            return clone;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TriggerRegistry));
        }

        private sealed class TriggerHandle : ITriggerHandle
        {
            private readonly TriggerRegistry _registry;
            private bool _disposed;

            public int TriggerId { get; }

            public TriggerHandle(int triggerId, TriggerRegistry registry)
            {
                TriggerId = triggerId;
                _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            }

            public void Dispose()
            {
                if (_disposed) return;
                _registry.Unregister(TriggerId);
                _disposed = true;
            }
        }

        private readonly struct Entry
        {
            public readonly ITriggerPlanConfig Config;
            public readonly ITriggerInstance Instance;
            public readonly object CompiledTrigger;
            public readonly Type ArgsType;
            public readonly Type ContextType;

            public Entry(ITriggerPlanConfig config, ITriggerInstance instance, object compiledTrigger, Type argsType, Type contextType)
            {
                Config = config;
                Instance = instance;
                CompiledTrigger = compiledTrigger;
                ArgsType = argsType;
                ContextType = contextType;
            }

            public TriggerRegistryEntry ToPublicEntry()
            {
                return new TriggerRegistryEntry(Config, Instance, CompiledTrigger, ArgsType, ContextType);
            }
        }

        private sealed class RegisteredTriggerPlanConfig : ITriggerPlanConfig
        {
            private readonly ITriggerPlanConfig _inner;

            public RegisteredTriggerPlanConfig(int triggerId, ITriggerPlanConfig inner)
            {
                if (triggerId <= 0) throw new ArgumentOutOfRangeException(nameof(triggerId));
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                TriggerId = triggerId;
            }

            public int TriggerId { get; }
            public int EventId => _inner.EventId;
            public string EventName => _inner.EventName;
            public int Phase => _inner.Phase;
            public int Priority => _inner.Priority;
            public int InterruptPriority => _inner.InterruptPriority;
            public IPredicateConfig Predicate => _inner.Predicate;
            public IReadOnlyList<IActionCallConfig> Actions => _inner.Actions;
            public IScheduleConfig Schedule => _inner.Schedule;
            public ICueConfig Cue => _inner.Cue;
            public TriggerPlanScope Scope => _inner.Scope;
        }
    }

    public readonly struct TriggerRegistryEntry
    {
        public readonly ITriggerPlanConfig Config;
        public readonly ITriggerInstance Instance;
        public readonly object CompiledTrigger;
        public readonly Type ArgsType;
        public readonly Type ContextType;

        public TriggerRegistryEntry(
            ITriggerPlanConfig config,
            ITriggerInstance instance,
            object compiledTrigger,
            Type argsType,
            Type contextType)
        {
            Config = config;
            Instance = instance;
            CompiledTrigger = compiledTrigger;
            ArgsType = argsType;
            ContextType = contextType;
        }

        public int TriggerId => Config?.TriggerId ?? 0;
        public bool HasCompiledTrigger => CompiledTrigger != null;
    }
}

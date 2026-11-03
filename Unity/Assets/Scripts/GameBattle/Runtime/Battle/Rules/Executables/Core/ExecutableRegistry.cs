using System;
using System.Collections.Generic;
using System.Reflection;
using AbilityKit.Core.Markers;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime.Config;
using AbilityKit.Triggering.Runtime.Context;
using AbilityKit.Triggering.Variables.Numeric;

namespace AbilityKit.Triggering.Runtime.Executable
{
    /// <summary>
    /// 行为类型描述符
    /// </summary>
    public sealed class ExecutableDescriptor
    {
        public int TypeId;
        public string TypeName;
        public ExecutableMetadata Metadata;
        public bool IsScheduled => Metadata.IsScheduled;
        public bool IsPeriodic => Metadata.IsScheduled && Metadata.DefaultPeriodMs.HasValue;
        public Func<IExecutable> Factory;
    }

    /// <summary>
    /// 条件类型描述符
    /// </summary>
    public sealed class ConditionDescriptor
    {
        public int TypeId;
        public string TypeName;
        public Func<ICondition> Factory;
    }

    /// <summary>
    /// 行为类型注册表。
    /// 旧 Runtime/Executable 兼容路径默认只注册内建类型，外部扩展需要显式注册或显式触发 Attribute 扫描。
    /// </summary>
    public sealed class ExecutableRegistry : IMarkerRegistry
    {
        private static readonly Lazy<ExecutableRegistry> _instance = new(() => new ExecutableRegistry());
        public static ExecutableRegistry Instance => _instance.Value;

        private readonly Dictionary<int, ExecutableDescriptor> _executables = new();
        private readonly Dictionary<string, int> _nameToId = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, ConditionDescriptor> _conditions = new();
        private readonly Dictionary<string, int> _conditionNameToId = new(StringComparer.OrdinalIgnoreCase);

        private ExecutableRegistry()
        {
            RegisterBuiltin();
        }

        /// <summary>
        /// 注册行为类型（通过显式 TypeId）
        /// </summary>
        public void Register<TExecutable>(int typeId, string typeName, ExecutableMetadata metadata = default)
            where TExecutable : IExecutable, new()
        {
            _executables[typeId] = new ExecutableDescriptor
            {
                TypeId = typeId,
                TypeName = typeName,
                Metadata = metadata,
                Factory = () => new TExecutable()
            };
            _nameToId[typeName] = typeId;
        }

        public IExecutable CreateExecutable(int typeId)
        {
            if (_executables.TryGetValue(typeId, out var descriptor))
                return descriptor.Factory();
            throw new KeyNotFoundException($"Executable type {typeId} not found");
        }

        public TExecutable CreateExecutable<TExecutable>(int typeId) where TExecutable : IExecutable
            => (TExecutable)CreateExecutable(typeId);

        public bool TryGetDescriptor(int typeId, out ExecutableDescriptor descriptor)
            => _executables.TryGetValue(typeId, out descriptor);

        public ExecutableDescriptor GetDescriptor(int typeId)
        {
            if (_executables.TryGetValue(typeId, out var descriptor))
                return descriptor;
            throw new KeyNotFoundException($"Executable type {typeId} not found");
        }

        public bool TryGetTypeIdByName(string typeName, out int typeId)
            => _nameToId.TryGetValue(typeName, out typeId);

        public void RegisterCondition<TCondition>(int typeId, string typeName)
            where TCondition : ICondition, new()
        {
            _conditions[typeId] = new ConditionDescriptor
            {
                TypeId = typeId,
                TypeName = typeName,
                Factory = () => new TCondition()
            };
            _conditionNameToId[typeName] = typeId;
        }

        public ICondition CreateCondition(int typeId)
        {
            if (_conditions.TryGetValue(typeId, out var descriptor))
                return descriptor.Factory();
            throw new KeyNotFoundException($"Condition type {typeId} not found");
        }

        public ICondition CreateCondition(string typeName)
        {
            if (TryGetConditionTypeIdByName(typeName, out var typeId))
                return CreateCondition(typeId);
            throw new KeyNotFoundException($"Condition type '{typeName}' not found");
        }

        public bool TryGetConditionDescriptor(int typeId, out ConditionDescriptor descriptor)
            => _conditions.TryGetValue(typeId, out descriptor);

        public bool TryGetConditionTypeIdByName(string typeName, out int typeId)
            => _conditionNameToId.TryGetValue(typeName, out typeId);

        public int Count => _executables.Count + _conditions.Count;

        public void Register(Type implType)
        {
            if (implType == null)
            {
                throw new ArgumentNullException(nameof(implType));
            }

            RegisterExecutableAttribute(implType);
            RegisterConditionAttribute(implType);
        }

        public void RegisterMarker(Type markerType, Type implType)
        {
            Register(implType);
        }

        private void RegisterExecutableAttribute(Type implType)
        {
            if (!typeof(IExecutable).IsAssignableFrom(implType))
            {
                return;
            }

            var attribute = implType.GetCustomAttribute<ExecutableTypeIdAttribute>();
            if (attribute == null)
            {
                return;
            }

            _executables[attribute.TypeId] = new ExecutableDescriptor
            {
                TypeId = attribute.TypeId,
                TypeName = attribute.TypeName,
                Metadata = new ExecutableMetadata(
                    attribute.TypeId,
                    attribute.TypeName,
                    attribute.IsComposite,
                    attribute.IsScheduled,
                    attribute.DefaultDurationMs > 0f ? attribute.DefaultDurationMs : null,
                    attribute.DefaultPeriodMs > 0f ? attribute.DefaultPeriodMs : null),
                Factory = () => (IExecutable)Activator.CreateInstance(implType)
            };
            _nameToId[attribute.TypeName] = attribute.TypeId;
        }

        private void RegisterConditionAttribute(Type implType)
        {
            if (!typeof(ICondition).IsAssignableFrom(implType))
            {
                return;
            }

            var attribute = implType.GetCustomAttribute<ConditionTypeIdAttribute>();
            if (attribute == null)
            {
                return;
            }

            _conditions[attribute.TypeId] = new ConditionDescriptor
            {
                TypeId = attribute.TypeId,
                TypeName = attribute.TypeName,
                Factory = () => (ICondition)Activator.CreateInstance(implType)
            };
            _conditionNameToId[attribute.TypeName] = attribute.TypeId;
        }

        private void RegisterBuiltin()
        {
            Register<SequenceExecutable>(TypeIdRegistry.Executable.Sequence, "Sequence", new ExecutableMetadata(TypeIdRegistry.Executable.Sequence, "Sequence", isComposite: true));
            Register<SelectorExecutable>(TypeIdRegistry.Executable.Selector, "Selector", new ExecutableMetadata(TypeIdRegistry.Executable.Selector, "Selector", isComposite: true));
            Register<ParallelExecutable>(TypeIdRegistry.Executable.Parallel, "Parallel", new ExecutableMetadata(TypeIdRegistry.Executable.Parallel, "Parallel", isComposite: true));
            Register<IfExecutable>(TypeIdRegistry.Executable.If, "If", new ExecutableMetadata(TypeIdRegistry.Executable.If, "If", isComposite: true));
            Register<IfElseExecutable>(TypeIdRegistry.Executable.IfElse, "IfElse", new ExecutableMetadata(TypeIdRegistry.Executable.IfElse, "IfElse", isComposite: true));
            Register<SwitchExecutable>(TypeIdRegistry.Executable.Switch, "Switch", new ExecutableMetadata(TypeIdRegistry.Executable.Switch, "Switch", isComposite: true));
            Register<RepeatExecutable>(TypeIdRegistry.Executable.Repeat, "Repeat", new ExecutableMetadata(TypeIdRegistry.Executable.Repeat, "Repeat", isComposite: true));
            Register<UntilExecutable>(TypeIdRegistry.Executable.Until, "Until", new ExecutableMetadata(TypeIdRegistry.Executable.Until, "Until", isComposite: true));

            RegisterCondition<MultiCondition>(TypeIdRegistry.Condition.Multi, "Multi");
            RegisterCondition<NotCondition>(TypeIdRegistry.Condition.Not, "Not");
            RegisterCondition<AndCondition>(TypeIdRegistry.Condition.And, "And");
            RegisterCondition<OrCondition>(TypeIdRegistry.Condition.Or, "Or");
            RegisterCondition<NumericCompareCondition>(TypeIdRegistry.Condition.NumericCompare, "NumericCompare");
            RegisterCondition<PayloadCompareCondition>(TypeIdRegistry.Condition.PayloadCompare, "PayloadCompare");
            RegisterCondition<ConstCondition>(TypeIdRegistry.Condition.Const, "Const");
        }
    }
}

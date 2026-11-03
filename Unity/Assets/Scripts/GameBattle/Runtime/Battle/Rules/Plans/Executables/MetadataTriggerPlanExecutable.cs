using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public enum ETriggerPlanMetadataKind : byte
    {
        Generic = 0,
        Tags = 1,
        Modifiers = 2,
        Stack = 3,
        Hierarchy = 4,
        Capability = 5,
        Duration = 6,
        Continuous = 7,
    }

    public sealed class MetadataTriggerPlanExecutable : TriggerPlanExecutableBase
    {
        private readonly ITriggerPlanExecutable _child;
        private readonly Dictionary<string, string> _values;

        public MetadataTriggerPlanExecutable(
            ITriggerPlanExecutable child,
            ETriggerPlanMetadataKind metadataKind,
            IReadOnlyDictionary<string, string> values = null,
            ITriggerPlanCondition condition = null,
            float weight = 1f)
            : base(condition, weight)
        {
            _child = child;
            MetadataKind = metadataKind;
            _values = values != null
                ? new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public override string Name => $"Metadata({MetadataKind}, {_child?.Name ?? "null"})";
        public override ETriggerPlanExecutableKind Kind => ETriggerPlanExecutableKind.Metadata;
        public ETriggerPlanMetadataKind MetadataKind { get; }
        public IReadOnlyDictionary<string, string> Values => _values;
        public ITriggerPlanExecutable Child => _child;

        protected override TriggerPlanExecutionResult ExecuteCore<TCtx>(object args, in ExecCtx<TCtx> ctx)
        {
            if (_child == null)
                return TriggerPlanExecutionResult.Skipped("Metadata child is empty");

            return _child.Execute(args, in ctx);
        }
    }
}

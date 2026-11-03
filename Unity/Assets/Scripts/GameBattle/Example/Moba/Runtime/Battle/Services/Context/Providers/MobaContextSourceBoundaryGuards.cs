namespace AbilityKit.Demo.Moba.Services
{
    /// <summary>
    /// 上下文来源模型的边界辅助方法。
    /// 这些方法用于让调用点明确自己消费的是执行期模型、诊断/查询视图，还是可持久化的安全快照。
    /// </summary>
    public static class MobaContextSourceBoundaryGuards
    {
        public static bool IsExecutionBoundary(this in MobaContextSourceView source)
        {
            return source.Boundary == MobaContextSourceBoundary.Execution;
        }

        public static bool IsSnapshotBoundary(this in MobaContextSourceView source)
        {
            return source.Boundary == MobaContextSourceBoundary.Snapshot;
        }

        public static bool IsLiveRuntimeBoundary(this in MobaContextSourceView source)
        {
            return source.Boundary == MobaContextSourceBoundary.LiveRuntime;
        }

        public static bool CanCreateDownstreamExecution(this in MobaContextSourceView source)
        {
            return source.IsFormalSource && source.HasExecutionSource && source.ParentContextId != 0;
        }

        public static bool CanUseForBusinessQuery(this in MobaContextSourceView source)
        {
            return source.IsFormalSource && source.IsValid;
        }

        public static bool CanUseForDiagnostics(this in MobaContextSourceView source)
        {
            return source.IsValid && (source.IsDiagnosticSource || source.HasRuntimeDiagnostics);
        }

        public static MobaPersistentContextSourceSnapshot ToPersistentSnapshot(this in MobaContextSourceView source)
        {
            return MobaPersistentContextSourceSnapshotFactory.FromContextSource(in source);
        }

        public static bool TryToPersistentSnapshot(this in MobaContextSourceView source, out MobaPersistentContextSourceSnapshot snapshot)
        {
            snapshot = source.IsValid
                ? MobaPersistentContextSourceSnapshotFactory.FromContextSource(in source)
                : default;
            return snapshot.IsValid;
        }

        public static bool TryAsExecutionSource(this in MobaContextSourceView source, out MobaCombatExecutionContext context)
        {
            context = default;
            if (!source.CanCreateDownstreamExecution()) return false;

            var snapshot = MobaPersistentContextSourceSnapshotFactory.FromContextSource(in source);
            return snapshot.TryGetCombatContextSource(out var combatSource)
                   && MobaCombatContextBuilder.TryFromSource(snapshot, in combatSource, out context)
                   && context.HasExecutionSource;
        }
    }
}

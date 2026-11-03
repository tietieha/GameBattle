using System;
using AbilityKit.Ability.World.DI;
using AbilityKit.Ability.World.Services;
using AbilityKit.Ability.World.Services.Attributes;

namespace AbilityKit.Demo.Moba.Services
{
    public enum MobaTemporaryEntityKind
    {
        Projectile = 0,
        Area = 1,
        Summon = 2,
    }

    public readonly struct MobaTemporaryEntityLifecycleHealth
    {
        public readonly MobaTemporaryEntityKind Kind;
        public readonly int ActiveCount;
        public readonly long SpawnedCount;
        public readonly long DespawnedCount;
        public readonly long RejectedCount;
        public readonly long ReplacedCount;
        public readonly long TickEventCount;
        public readonly long HitEventCount;
        public readonly long EnterEventCount;
        public readonly long ExitEventCount;
        public readonly long ExpireEventCount;
        public readonly int LastFrame;

        public MobaTemporaryEntityLifecycleHealth(
            MobaTemporaryEntityKind kind,
            int activeCount,
            long spawnedCount,
            long despawnedCount,
            long rejectedCount,
            long replacedCount,
            long tickEventCount,
            long hitEventCount,
            long enterEventCount,
            long exitEventCount,
            long expireEventCount,
            int lastFrame)
        {
            Kind = kind;
            ActiveCount = activeCount;
            SpawnedCount = spawnedCount;
            DespawnedCount = despawnedCount;
            RejectedCount = rejectedCount;
            ReplacedCount = replacedCount;
            TickEventCount = tickEventCount;
            HitEventCount = hitEventCount;
            EnterEventCount = enterEventCount;
            ExitEventCount = exitEventCount;
            ExpireEventCount = expireEventCount;
            LastFrame = lastFrame;
        }

        public override string ToString()
        {
            return $"kind={Kind}, active={ActiveCount}, spawned={SpawnedCount}, despawned={DespawnedCount}, rejected={RejectedCount}, replaced={ReplacedCount}, ticks={TickEventCount}, hits={HitEventCount}, enters={EnterEventCount}, exits={ExitEventCount}, expires={ExpireEventCount}, lastFrame={LastFrame}";
        }
    }

    public interface IMobaTemporaryEntityLifecycleHealthProvider
    {
        MobaTemporaryEntityLifecycleHealth GetHealth(MobaTemporaryEntityKind kind);
        void GetAllHealth(MobaTemporaryEntityLifecycleHealth[] results);
    }

    public interface IMobaTemporaryEntityLifecycleService : IMobaTemporaryEntityLifecycleHealthProvider
    {
        void RecordSpawn(MobaTemporaryEntityKind kind, int activeCount, int frame = 0, long count = 1L);
        void RecordDespawn(MobaTemporaryEntityKind kind, int activeCount, int frame = 0, long count = 1L);
        void RecordRejected(MobaTemporaryEntityKind kind, int activeCount, int frame = 0, long count = 1L);
        void RecordReplaced(MobaTemporaryEntityKind kind, int activeCount, int frame = 0, long count = 1L);
        void RecordTickEvents(MobaTemporaryEntityKind kind, long count, int frame = 0);
        void RecordHitEvents(MobaTemporaryEntityKind kind, long count, int frame = 0);
        void RecordEnterEvents(MobaTemporaryEntityKind kind, long count, int frame = 0);
        void RecordExitEvents(MobaTemporaryEntityKind kind, long count, int frame = 0);
        void RecordExpireEvents(MobaTemporaryEntityKind kind, long count, int frame = 0);
        void SetActive(MobaTemporaryEntityKind kind, int activeCount, int frame = 0);
    }

    [WorldService(typeof(IMobaTemporaryEntityLifecycleService), WorldLifetime.Scoped)]
    [WorldService(typeof(IMobaTemporaryEntityLifecycleHealthProvider), WorldLifetime.Scoped)]
    [WorldService(typeof(MobaTemporaryEntityLifecycleService), WorldLifetime.Scoped)]
    public sealed class MobaTemporaryEntityLifecycleService : IMobaTemporaryEntityLifecycleService, IService
    {
        [WorldInject(required: false)] private IMobaBattleDiagnosticsService _diagnostics = null;

        private CounterState _projectiles;
        private CounterState _areas;
        private CounterState _summons;

        public void RecordSpawn(MobaTemporaryEntityKind kind, int activeCount, int frame = 0, long count = 1L)
        {
            if (count <= 0L) return;
            ref var state = ref GetState(kind);
            state.SpawnedCount += count;
            state.ActiveCount = ClampActive(activeCount);
            state.LastFrame = frame;
            Counter(kind, MobaBattleDiagnosticMetric.TempEntitySpawnedSuffix, count);
            GaugeActive(kind, state.ActiveCount);
            RecordHealth(kind);
        }

        public void RecordDespawn(MobaTemporaryEntityKind kind, int activeCount, int frame = 0, long count = 1L)
        {
            if (count <= 0L) return;
            ref var state = ref GetState(kind);
            state.DespawnedCount += count;
            state.ActiveCount = ClampActive(activeCount);
            state.LastFrame = frame;
            Counter(kind, MobaBattleDiagnosticMetric.TempEntityDespawnedSuffix, count);
            GaugeActive(kind, state.ActiveCount);
            RecordHealth(kind);
        }

        public void RecordRejected(MobaTemporaryEntityKind kind, int activeCount, int frame = 0, long count = 1L)
        {
            if (count <= 0L) return;
            ref var state = ref GetState(kind);
            state.RejectedCount += count;
            state.ActiveCount = ClampActive(activeCount);
            state.LastFrame = frame;
            Counter(kind, MobaBattleDiagnosticMetric.TempEntityRejectedSuffix, count);
            GaugeActive(kind, state.ActiveCount);
            RecordHealth(kind);
        }

        public void RecordReplaced(MobaTemporaryEntityKind kind, int activeCount, int frame = 0, long count = 1L)
        {
            if (count <= 0L) return;
            ref var state = ref GetState(kind);
            state.ReplacedCount += count;
            state.ActiveCount = ClampActive(activeCount);
            state.LastFrame = frame;
            Counter(kind, MobaBattleDiagnosticMetric.TempEntityReplacedSuffix, count);
            GaugeActive(kind, state.ActiveCount);
            RecordHealth(kind);
        }

        public void RecordTickEvents(MobaTemporaryEntityKind kind, long count, int frame = 0)
        {
            if (count <= 0L) return;
            ref var state = ref GetState(kind);
            state.TickEventCount += count;
            state.LastFrame = frame;
            Counter(kind, MobaBattleDiagnosticMetric.TempEntityTicksSuffix, count);
            RecordHealth(kind);
        }

        public void RecordHitEvents(MobaTemporaryEntityKind kind, long count, int frame = 0)
        {
            if (count <= 0L) return;
            ref var state = ref GetState(kind);
            state.HitEventCount += count;
            state.LastFrame = frame;
            Counter(kind, MobaBattleDiagnosticMetric.TempEntityHitsSuffix, count);
            RecordHealth(kind);
        }

        public void RecordEnterEvents(MobaTemporaryEntityKind kind, long count, int frame = 0)
        {
            if (count <= 0L) return;
            ref var state = ref GetState(kind);
            state.EnterEventCount += count;
            state.LastFrame = frame;
            Counter(kind, MobaBattleDiagnosticMetric.TempEntityEntersSuffix, count);
            RecordHealth(kind);
        }

        public void RecordExitEvents(MobaTemporaryEntityKind kind, long count, int frame = 0)
        {
            if (count <= 0L) return;
            ref var state = ref GetState(kind);
            state.ExitEventCount += count;
            state.LastFrame = frame;
            Counter(kind, MobaBattleDiagnosticMetric.TempEntityExitsSuffix, count);
            RecordHealth(kind);
        }

        public void RecordExpireEvents(MobaTemporaryEntityKind kind, long count, int frame = 0)
        {
            if (count <= 0L) return;
            ref var state = ref GetState(kind);
            state.ExpireEventCount += count;
            state.LastFrame = frame;
            Counter(kind, MobaBattleDiagnosticMetric.TempEntityExpiresSuffix, count);
            RecordHealth(kind);
        }

        public void SetActive(MobaTemporaryEntityKind kind, int activeCount, int frame = 0)
        {
            ref var state = ref GetState(kind);
            state.ActiveCount = ClampActive(activeCount);
            state.LastFrame = frame;
            GaugeActive(kind, state.ActiveCount);
            RecordHealth(kind);
        }

        public MobaTemporaryEntityLifecycleHealth GetHealth(MobaTemporaryEntityKind kind)
        {
            var state = GetState(kind);
            return new MobaTemporaryEntityLifecycleHealth(
                kind,
                state.ActiveCount,
                state.SpawnedCount,
                state.DespawnedCount,
                state.RejectedCount,
                state.ReplacedCount,
                state.TickEventCount,
                state.HitEventCount,
                state.EnterEventCount,
                state.ExitEventCount,
                state.ExpireEventCount,
                state.LastFrame);
        }

        public void GetAllHealth(MobaTemporaryEntityLifecycleHealth[] results)
        {
            if (results == null || results.Length < 3) return;
            results[0] = GetHealth(MobaTemporaryEntityKind.Projectile);
            results[1] = GetHealth(MobaTemporaryEntityKind.Area);
            results[2] = GetHealth(MobaTemporaryEntityKind.Summon);
        }

        public void Dispose()
        {
            _projectiles = default;
            _areas = default;
            _summons = default;
        }

        private ref CounterState GetState(MobaTemporaryEntityKind kind)
        {
            switch (kind)
            {
                case MobaTemporaryEntityKind.Area:
                    return ref _areas;
                case MobaTemporaryEntityKind.Summon:
                    return ref _summons;
                case MobaTemporaryEntityKind.Projectile:
                default:
                    return ref _projectiles;
            }
        }

        private void Counter(MobaTemporaryEntityKind kind, string suffix, long count)
        {
            _diagnostics?.Counter(GetMetricPrefix(kind) + suffix, count);
        }

        private void GaugeActive(MobaTemporaryEntityKind kind, int activeCount)
        {
            _diagnostics?.Gauge(GetMetricPrefix(kind) + MobaBattleDiagnosticMetric.TempEntityActiveSuffix, activeCount);
        }

        private void RecordHealth(MobaTemporaryEntityKind kind)
        {
            _diagnostics?.RecordLifecycleHealth(GetHealth(kind));
        }

        private static int ClampActive(int activeCount)
        {
            return activeCount < 0 ? 0 : activeCount;
        }

        private static string GetMetricPrefix(MobaTemporaryEntityKind kind)
        {
            switch (kind)
            {
                case MobaTemporaryEntityKind.Area:
                    return MobaBattleDiagnosticMetric.TempEntityAreaPrefix;
                case MobaTemporaryEntityKind.Summon:
                    return MobaBattleDiagnosticMetric.TempEntitySummonPrefix;
                case MobaTemporaryEntityKind.Projectile:
                default:
                    return MobaBattleDiagnosticMetric.TempEntityProjectilePrefix;
            }
        }

        private struct CounterState
        {
            public int ActiveCount;
            public long SpawnedCount;
            public long DespawnedCount;
            public long RejectedCount;
            public long ReplacedCount;
            public long TickEventCount;
            public long HitEventCount;
            public long EnterEventCount;
            public long ExitEventCount;
            public long ExpireEventCount;
            public int LastFrame;
        }
    }
}

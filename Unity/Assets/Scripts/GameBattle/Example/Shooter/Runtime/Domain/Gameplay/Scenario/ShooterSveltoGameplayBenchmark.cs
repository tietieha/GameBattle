#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public readonly struct ShooterSveltoGameplayBenchmarkProfile
    {
        public ShooterSveltoGameplayBenchmarkProfile(string id, string displayName, ShooterSveltoGameplayScenarioConfig scenario, int iterations)
            : this(id, displayName, scenario, iterations, ShooterSveltoGameplayEntityBudgetProfile.Default)
        {
        }

        public ShooterSveltoGameplayBenchmarkProfile(
            string id,
            string displayName,
            ShooterSveltoGameplayScenarioConfig scenario,
            int iterations,
            ShooterSveltoGameplayEntityBudgetProfile entityBudget)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Profile id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Profile display name is required.", nameof(displayName));

            Id = id;
            DisplayName = displayName;
            Scenario = scenario;
            Iterations = iterations < 1 ? 1 : iterations;
            EntityBudget = entityBudget.MaxEntityCount <= 0
                ? ShooterSveltoGameplayEntityBudgetProfile.Default
                : entityBudget;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public ShooterSveltoGameplayScenarioConfig Scenario { get; }
        public int Iterations { get; }
        public ShooterSveltoGameplayEntityBudgetProfile EntityBudget { get; }
    }

    public readonly struct ShooterSveltoGameplayEntityBudgetProfile
    {
        public ShooterSveltoGameplayEntityBudgetProfile(int maxEntityCount, int activeSyncBudget)
        {
            var limits = new ShooterEntityLimitOptions(maxEntityCount);
            MaxEntityCount = limits.MaxEntityCount;
            ActiveSyncBudget = activeSyncBudget < 1 ? MaxEntityCount : Math.Min(activeSyncBudget, MaxEntityCount);
        }

        public int MaxEntityCount { get; }
        public int ActiveSyncBudget { get; }
        public static ShooterSveltoGameplayEntityBudgetProfile Default => new ShooterSveltoGameplayEntityBudgetProfile(ShooterEntityLimitOptions.DefaultMaxEntityCount, ShooterEntityLimitOptions.DefaultMaxEntityCount);
    }

    public readonly struct ShooterSveltoGameplayEntityBudgetDiagnostics
    {
        public ShooterSveltoGameplayEntityBudgetDiagnostics(
            int maxEntityCount,
            int activeSyncBudget,
            int requestedInitialEntityCount,
            int clampedInitialEntityCount,
            int initialEntityBudgetHeadroom,
            bool initialEntitiesWithinBudget,
            long totalActiveSyncBudgetFrames)
            : this(
                maxEntityCount,
                activeSyncBudget,
                requestedInitialEntityCount,
                clampedInitialEntityCount,
                initialEntityBudgetHeadroom,
                initialEntitiesWithinBudget,
                totalActiveSyncBudgetFrames,
                0L,
                0L,
                0L,
                0L)
        {
        }

        public ShooterSveltoGameplayEntityBudgetDiagnostics(
            int maxEntityCount,
            int activeSyncBudget,
            int requestedInitialEntityCount,
            int clampedInitialEntityCount,
            int initialEntityBudgetHeadroom,
            bool initialEntitiesWithinBudget,
            long totalActiveSyncBudgetFrames,
            long elapsedTicks,
            long allocatedBytes,
            long averageFrameTicks,
            long averageFrameAllocatedBytes)
        {
            MaxEntityCount = maxEntityCount;
            ActiveSyncBudget = activeSyncBudget;
            RequestedInitialEntityCount = requestedInitialEntityCount;
            ClampedInitialEntityCount = clampedInitialEntityCount;
            InitialEntityBudgetHeadroom = initialEntityBudgetHeadroom;
            InitialEntitiesWithinBudget = initialEntitiesWithinBudget;
            TotalActiveSyncBudgetFrames = totalActiveSyncBudgetFrames;
            ElapsedTicks = elapsedTicks;
            AllocatedBytes = allocatedBytes;
            AverageFrameTicks = averageFrameTicks;
            AverageFrameAllocatedBytes = averageFrameAllocatedBytes;
        }

        public int MaxEntityCount { get; }
        public int ActiveSyncBudget { get; }
        public int RequestedInitialEntityCount { get; }
        public int ClampedInitialEntityCount { get; }
        public int InitialEntityBudgetHeadroom { get; }
        public bool InitialEntitiesWithinBudget { get; }
        public long TotalActiveSyncBudgetFrames { get; }
        public long ElapsedTicks { get; }
        public long AllocatedBytes { get; }
        public long AverageFrameTicks { get; }
        public long AverageFrameAllocatedBytes { get; }
    }

    public readonly struct ShooterCollisionBenchmarkResult
    {
        public ShooterCollisionBenchmarkResult(
            int projectileCount,
            int targetCount,
            int iterations,
            int linearHits,
            int spatialHits,
            long linearElapsedTicks,
            long spatialElapsedTicks,
            bool sameHitCount)
            : this(
                projectileCount,
                targetCount,
                iterations,
                linearHits,
                spatialHits,
                linearElapsedTicks,
                spatialElapsedTicks,
                sameHitCount,
                0,
                0,
                0,
                0L)
        {
        }

        public ShooterCollisionBenchmarkResult(
            int projectileCount,
            int targetCount,
            int iterations,
            int linearHits,
            int spatialHits,
            long linearElapsedTicks,
            long spatialElapsedTicks,
            bool sameHitCount,
            int spatialCellCount,
            int spatialIndexedTargetCount,
            int spatialLargestCellOccupancy,
            long spatialCandidateChecks)
        {
            ProjectileCount = projectileCount;
            TargetCount = targetCount;
            Iterations = iterations;
            LinearHits = linearHits;
            SpatialHits = spatialHits;
            LinearElapsedTicks = linearElapsedTicks;
            SpatialElapsedTicks = spatialElapsedTicks;
            SameHitCount = sameHitCount;
            SpatialCellCount = spatialCellCount;
            SpatialIndexedTargetCount = spatialIndexedTargetCount;
            SpatialLargestCellOccupancy = spatialLargestCellOccupancy;
            SpatialCandidateChecks = spatialCandidateChecks;
        }

        public int ProjectileCount { get; }
        public int TargetCount { get; }
        public int Iterations { get; }
        public int LinearHits { get; }
        public int SpatialHits { get; }
        public long LinearElapsedTicks { get; }
        public long SpatialElapsedTicks { get; }
        public bool SameHitCount { get; }
        public int SpatialCellCount { get; }
        public int SpatialIndexedTargetCount { get; }
        public int SpatialLargestCellOccupancy { get; }
        public long SpatialCandidateChecks { get; }
        public long AverageSpatialCandidateChecksPerProjectile => ProjectileCount <= 0 || Iterations <= 0 ? 0L : SpatialCandidateChecks / ((long)ProjectileCount * Iterations);
        public bool SpatialFaster => SpatialElapsedTicks < LinearElapsedTicks;
    }

    public readonly struct ShooterSveltoGameplayBenchmarkResult
    {
        public ShooterSveltoGameplayBenchmarkResult(
            string profileId,
            string scenarioId,
            int iterations,
            int framesPerIteration,
            int initialEntityCount,
            long totalFrames,
            long totalInitialEntityFrames,
            bool deterministic,
            ShooterSveltoGameplayScenarioResult firstResult,
            ShooterSveltoGameplayScenarioResult lastResult,
            ShooterSveltoGameplayEntityBudgetDiagnostics entityBudget)
        {
            ProfileId = profileId;
            ScenarioId = scenarioId;
            Iterations = iterations;
            FramesPerIteration = framesPerIteration;
            InitialEntityCount = initialEntityCount;
            TotalFrames = totalFrames;
            TotalInitialEntityFrames = totalInitialEntityFrames;
            Deterministic = deterministic;
            FirstResult = firstResult;
            LastResult = lastResult;
            EntityBudget = entityBudget;
        }

        public string ProfileId { get; }
        public string ScenarioId { get; }
        public int Iterations { get; }
        public int FramesPerIteration { get; }
        public int InitialEntityCount { get; }
        public long TotalFrames { get; }
        public long TotalInitialEntityFrames { get; }
        public bool Deterministic { get; }
        public ShooterSveltoGameplayScenarioResult FirstResult { get; }
        public ShooterSveltoGameplayScenarioResult LastResult { get; }
        public ShooterSveltoGameplayEntityBudgetDiagnostics EntityBudget { get; }
    }

    public static class ShooterSveltoGameplayBenchmarkProfiles
    {
        public static ShooterSveltoGameplayBenchmarkProfile ProjectileStormBaseline { get; } = new ShooterSveltoGameplayBenchmarkProfile(
            "svelto-projectile-storm-baseline",
            "Svelto Projectile Storm Baseline",
            ShooterSveltoGameplayScenarioCatalog.ProjectileStorm,
            iterations: 3);

        public static ShooterSveltoGameplayBenchmarkProfile WaveSurvivalBaseline { get; } = new ShooterSveltoGameplayBenchmarkProfile(
            "svelto-wave-survival-baseline",
            "Svelto Wave Survival Baseline",
            ShooterSveltoGameplayScenarioCatalog.WaveSurvival,
            iterations: 3);

        public static ShooterSveltoGameplayBenchmarkProfile SmallScaleEntityBudget { get; } = new ShooterSveltoGameplayBenchmarkProfile(
            "svelto-small-scale-entity-budget",
            "Svelto Small Scale Entity Budget",
            new ShooterSveltoGameplayScenarioConfig(
                "svelto-small-scale-entity-budget",
                "Svelto Small Scale Entity Budget",
                "小规模 Shooter 实体预算回归入口，快速验证预算诊断与确定性。",
                shooterCount: 4,
                targetCount: 64,
                tickCount: 30,
                tickDeltaTime: 1f / 30f,
                arenaRadius: 18f,
                loadout: ShooterSveltoGameplayScenarioCatalog.DefaultLoadout,
                battleFlow: new ShooterSveltoGameplayBattleFlowConfig(
                    durationFrames: 30,
                    victoryTargetDefeats: 64,
                    maxActiveEnemies: 32,
                    waves: new[]
                    {
                        new ShooterSveltoGameplayWaveConfig(1, 0, 2, 32, 2, 14f)
                    })),
            iterations: 2,
            entityBudget: new ShooterSveltoGameplayEntityBudgetProfile(
                ShooterEntityLimitOptions.DefaultMaxEntityCount,
                activeSyncBudget: 128));

        public static ShooterSveltoGameplayBenchmarkProfile MediumScaleEntityBudget { get; } = new ShooterSveltoGameplayBenchmarkProfile(
            "svelto-medium-scale-entity-budget",
            "Svelto Medium Scale Entity Budget",
            new ShooterSveltoGameplayScenarioConfig(
                "svelto-medium-scale-entity-budget",
                "Svelto Medium Scale Entity Budget",
                "中规模 Shooter 实体预算回归入口，覆盖更多玩家、敌人与同步预算。",
                shooterCount: 32,
                targetCount: 512,
                tickCount: 45,
                tickDeltaTime: 1f / 30f,
                arenaRadius: 36f,
                loadout: ShooterSveltoGameplayScenarioCatalog.DefaultLoadout,
                battleFlow: new ShooterSveltoGameplayBattleFlowConfig(
                    durationFrames: 45,
                    victoryTargetDefeats: 512,
                    maxActiveEnemies: 128,
                    waves: new[]
                    {
                        new ShooterSveltoGameplayWaveConfig(1, 0, 1, 128, 2, 24f),
                        new ShooterSveltoGameplayWaveConfig(2, 15, 2, 128, 3, 30f)
                    })),
            iterations: 2,
            entityBudget: new ShooterSveltoGameplayEntityBudgetProfile(
                ShooterEntityLimitOptions.DefaultMaxEntityCount,
                activeSyncBudget: 512));

        public static ShooterSveltoGameplayBenchmarkProfile MassScaleEntityBudget { get; } = new ShooterSveltoGameplayBenchmarkProfile(
            "svelto-mass-scale-entity-budget",
            "Svelto Mass Scale Entity Budget",
            new ShooterSveltoGameplayScenarioConfig(
                "svelto-mass-scale-entity-budget",
                "Svelto Mass Scale Entity Budget",
                "大规模 Shooter 实体预算压测入口，复用 Svelto gameplay runner 输出预算诊断。",
                shooterCount: 256,
                targetCount: 4096,
                tickCount: 60,
                tickDeltaTime: 1f / 30f,
                arenaRadius: 64f,
                loadout: ShooterSveltoGameplayScenarioCatalog.DefaultLoadout,
                battleFlow: new ShooterSveltoGameplayBattleFlowConfig(
                    durationFrames: 60,
                    victoryTargetDefeats: 4096,
                    maxActiveEnemies: 512,
                    waves: new[]
                    {
                        new ShooterSveltoGameplayWaveConfig(1, 0, 1, 512, 2, 32f),
                        new ShooterSveltoGameplayWaveConfig(2, 15, 1, 512, 3, 40f)
                    })),
            iterations: 2,
            entityBudget: new ShooterSveltoGameplayEntityBudgetProfile(
                ShooterEntityLimitOptions.DefaultMaxEntityCount,
                activeSyncBudget: 2048));

        public static ShooterSveltoGameplayBenchmarkProfile LargeScaleEntityBudget => MassScaleEntityBudget;
    }

    public static class ShooterSveltoGameplayBenchmark
    {
        public static ShooterSveltoGameplayBenchmarkResult Run(
            IShooterSveltoGameplayScenarioRunner runner,
            in ShooterSveltoGameplayBenchmarkProfile profile)
        {
            if (runner == null) throw new ArgumentNullException(nameof(runner));

            ShooterSveltoGameplayScenarioResult first = default;
            ShooterSveltoGameplayScenarioResult last = default;
            var deterministic = true;
            var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < profile.Iterations; i++)
            {
                var result = runner.Run(profile.Scenario);
                if (i == 0)
                {
                    first = result;
                }
                else if (!HasSameDeterministicOutcome(in first, in result) || !HasSameDeterministicOutcome(in last, in result))
                {
                    deterministic = false;
                }

                last = result;
            }

            stopwatch.Stop();
            var allocatedBytes = Math.Max(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore);
            var initialEntityCount = profile.Scenario.ShooterCount + profile.Scenario.TargetCount;
            var totalFrames = (long)profile.Iterations * profile.Scenario.TickCount;
            var entityBudget = CreateEntityBudgetDiagnostics(in profile, initialEntityCount, totalFrames, stopwatch.ElapsedTicks, allocatedBytes);
            return new ShooterSveltoGameplayBenchmarkResult(
                profile.Id,
                profile.Scenario.Id,
                profile.Iterations,
                profile.Scenario.TickCount,
                initialEntityCount,
                totalFrames,
                totalFrames * initialEntityCount,
                deterministic,
                first,
                last,
                entityBudget);
        }

        public static ShooterCollisionBenchmarkResult RunCollisionBroadphaseComparison(
            int projectileCount = 4096,
            int targetCount = 4096,
            int iterations = 8,
            float arenaRadius = 64f,
            float hitRadius = 0.45f)
        {
            projectileCount = Math.Max(1, projectileCount);
            targetCount = Math.Max(1, targetCount);
            iterations = Math.Max(1, iterations);
            arenaRadius = arenaRadius <= 0f ? 64f : arenaRadius;
            hitRadius = hitRadius <= 0f ? ShooterBattleTuning.HitRadius : hitRadius;

            var projectiles = new CollisionPoint[projectileCount];
            var targets = new CollisionPoint[targetCount];
            FillDeterministicCollisionPoints(projectiles, arenaRadius, phase: 17f);
            FillDeterministicCollisionPoints(targets, arenaRadius, phase: 97f);

            var linearStopwatch = Stopwatch.StartNew();
            var linearHits = 0;
            for (var iteration = 0; iteration < iterations; iteration++)
            {
                linearHits += CountLinearHits(projectiles, targets, hitRadius);
            }

            linearStopwatch.Stop();

            var spatialStopwatch = Stopwatch.StartNew();
            var spatialHits = 0;
            var spatialCandidateChecks = 0L;
            var spatialCellCount = 0;
            var spatialIndexedTargetCount = 0;
            var spatialLargestCellOccupancy = 0;
            for (var iteration = 0; iteration < iterations; iteration++)
            {
                spatialHits += CountSpatialHits(
                    projectiles,
                    targets,
                    hitRadius,
                    out var iterationCellCount,
                    out var iterationIndexedTargetCount,
                    out var iterationLargestCellOccupancy,
                    out var iterationCandidateChecks);
                spatialCandidateChecks += iterationCandidateChecks;
                spatialCellCount = iterationCellCount;
                spatialIndexedTargetCount = iterationIndexedTargetCount;
                spatialLargestCellOccupancy = Math.Max(spatialLargestCellOccupancy, iterationLargestCellOccupancy);
            }

            spatialStopwatch.Stop();

            return new ShooterCollisionBenchmarkResult(
                projectileCount,
                targetCount,
                iterations,
                linearHits,
                spatialHits,
                linearStopwatch.ElapsedTicks,
                spatialStopwatch.ElapsedTicks,
                linearHits == spatialHits,
                spatialCellCount,
                spatialIndexedTargetCount,
                spatialLargestCellOccupancy,
                spatialCandidateChecks);
        }

        private static void FillDeterministicCollisionPoints(CollisionPoint[] points, float arenaRadius, float phase)
        {
            for (var i = 0; i < points.Length; i++)
            {
                var angle = (i * 37f + phase) * MathF.PI / 180f;
                var radius = arenaRadius * (0.15f + (i % 23) / 28f);
                points[i] = new CollisionPoint(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
            }
        }

        private static int CountLinearHits(CollisionPoint[] projectiles, CollisionPoint[] targets, float hitRadius)
        {
            var hits = 0;
            var hitRadiusSq = hitRadius * hitRadius;
            for (var i = 0; i < projectiles.Length; i++)
            {
                for (var targetIndex = 0; targetIndex < targets.Length; targetIndex++)
                {
                    var dx = targets[targetIndex].X - projectiles[i].X;
                    var dy = targets[targetIndex].Y - projectiles[i].Y;
                    if (dx * dx + dy * dy <= hitRadiusSq)
                    {
                        hits++;
                        break;
                    }
                }
            }

            return hits;
        }

        private static int CountSpatialHits(
            CollisionPoint[] projectiles,
            CollisionPoint[] targets,
            float hitRadius,
            out int cellCount,
            out int indexedTargetCount,
            out int largestCellOccupancy,
            out long candidateChecks)
        {
            var index = new CollisionPointGrid(Math.Max(hitRadius * 2f, 1f));
            index.Rebuild(targets);
            var hits = 0;
            candidateChecks = 0L;
            for (var i = 0; i < projectiles.Length; i++)
            {
                if (index.HasHit(projectiles[i].X, projectiles[i].Y, hitRadius, targets, out var queryCandidateChecks))
                {
                    hits++;
                }

                candidateChecks += queryCandidateChecks;
            }

            cellCount = index.CellCount;
            indexedTargetCount = index.IndexedTargetCount;
            largestCellOccupancy = index.LargestCellOccupancy;
            return hits;
        }

        private static ShooterSveltoGameplayEntityBudgetDiagnostics CreateEntityBudgetDiagnostics(
            in ShooterSveltoGameplayBenchmarkProfile profile,
            int requestedInitialEntityCount,
            long totalFrames,
            long elapsedTicks,
            long allocatedBytes)
        {
            var limits = new ShooterEntityLimitOptions(profile.EntityBudget.MaxEntityCount);
            var clampedInitialEntityCount = limits.ClampRequestedCount(requestedInitialEntityCount);
            var safeTotalFrames = Math.Max(1L, totalFrames);
            return new ShooterSveltoGameplayEntityBudgetDiagnostics(
                limits.MaxEntityCount,
                profile.EntityBudget.ActiveSyncBudget,
                requestedInitialEntityCount,
                clampedInitialEntityCount,
                limits.MaxEntityCount - clampedInitialEntityCount,
                requestedInitialEntityCount <= limits.MaxEntityCount,
                totalFrames * profile.EntityBudget.ActiveSyncBudget,
                elapsedTicks,
                allocatedBytes,
                elapsedTicks / safeTotalFrames,
                allocatedBytes / safeTotalFrames);
        }

        private static bool HasSameDeterministicOutcome(
            in ShooterSveltoGameplayScenarioResult expected,
            in ShooterSveltoGameplayScenarioResult actual)
        {
            return string.Equals(expected.ScenarioId, actual.ScenarioId, StringComparison.Ordinal)
                && expected.Frames == actual.Frames
                && expected.Shooters == actual.Shooters
                && expected.Targets == actual.Targets
                && expected.ProjectilesSpawned == actual.ProjectilesSpawned
                && expected.ProjectilesExpired == actual.ProjectilesExpired
                && expected.Hits == actual.Hits
                && expected.DefeatedTargets == actual.DefeatedTargets
                && expected.ActiveProjectiles == actual.ActiveProjectiles
                && expected.RemainingTargetHp == actual.RemainingTargetHp
                && expected.EnemyHits == actual.EnemyHits
                && expected.StateHash == actual.StateHash;
        }

        private readonly struct CollisionPoint
        {
            public CollisionPoint(float x, float y)
            {
                X = x;
                Y = y;
            }

            public float X { get; }
            public float Y { get; }
        }

        private sealed class CollisionPointGrid
        {
            private readonly ShooterSpatialHashGrid _grid;
            private readonly List<int> _candidates = new(32);

            public CollisionPointGrid(float cellSize)
            {
                _grid = new ShooterSpatialHashGrid(cellSize);
            }

            public int CellCount => _grid.CellCount;

            public int IndexedTargetCount => _grid.TotalEntries;

            public int LargestCellOccupancy => _grid.LargestCellOccupancy;

            public void Rebuild(CollisionPoint[] targets)
            {
                _grid.Clear();
                for (var i = 0; i < targets.Length; i++)
                {
                    _grid.Add(targets[i].X, targets[i].Y, i);
                }
            }

            public bool HasHit(float x, float y, float hitRadius, CollisionPoint[] targets, out int candidateChecks)
            {
                _candidates.Clear();
                _grid.CollectAabb(x, y, hitRadius, _candidates);
                candidateChecks = _candidates.Count;

                var hitRadiusSq = hitRadius * hitRadius;
                for (var i = 0; i < _candidates.Count; i++)
                {
                    var target = targets[_candidates[i]];
                    var dx = target.X - x;
                    var dy = target.Y - y;
                    if (dx * dx + dy * dy <= hitRadiusSq)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}

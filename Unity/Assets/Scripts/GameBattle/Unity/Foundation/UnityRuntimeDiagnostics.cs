namespace AbilityKit.Diagnostics
{
    public static class UnityRuntimeDiagnostics
    {
        private static long _frameIndex;
        private static bool _metricsRegistered;

        public static void RegisterMetrics()
        {
            if (_metricsRegistered) return;
            _metricsRegistered = true;
            ProfilerHub.RegisterMetric(new MetricDefinition { Name = "unity.frame.delta_ms", Category = "unity", Kind = MetricKind.Sample, Unit = "ms", Description = "Scaled frame delta time", Tags = new[] { "unity", "frame" } });
            ProfilerHub.RegisterMetric(new MetricDefinition { Name = "unity.frame.unscaled_delta_ms", Category = "unity", Kind = MetricKind.Sample, Unit = "ms", Description = "Unscaled frame delta time", Tags = new[] { "unity", "frame" } });
            ProfilerHub.RegisterMetric(new MetricDefinition { Name = "unity.frame.index", Category = "unity", Kind = MetricKind.Gauge, Unit = "frame", Description = "Observed frame index", Tags = new[] { "unity", "frame" } });
            ProfilerHub.RegisterMetric(new MetricDefinition { Name = "unity.memory.gc_allocated_bytes", Category = "unity", Kind = MetricKind.Gauge, Unit = "bytes", Description = "Total managed memory reported by Unity profiler", Tags = new[] { "unity", "memory" } });
        }

        public static void CollectFrame()
        {
            CollectFrame(UnityEngine.Time.deltaTime, UnityEngine.Time.unscaledDeltaTime,
                UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong());
        }

        public static void CollectFrame(float deltaTime, float unscaledDeltaTime, long allocatedBytes)
        {
            if (!ProfilerHub.IsEnabled) return;
            RegisterMetrics();
            _frameIndex++;
            ProfilerHub.Sample("unity.frame.delta_ms", deltaTime * 1000d);
            ProfilerHub.Sample("unity.frame.unscaled_delta_ms", unscaledDeltaTime * 1000d);
            ProfilerHub.SetGauge("unity.frame.index", _frameIndex);
            if (allocatedBytes >= 0L) ProfilerHub.SetGauge("unity.memory.gc_allocated_bytes", allocatedBytes);
        }
    }
}

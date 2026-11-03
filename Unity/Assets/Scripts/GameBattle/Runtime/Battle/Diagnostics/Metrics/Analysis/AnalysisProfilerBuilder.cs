using System;
using System.Collections.Generic;

namespace AbilityKit.Diagnostics.Analysis
{
    /// <summary>
    /// 将内存中的 Profiler 快照转换为面向 Web 的稳定分析 Profiler 区段。
    /// </summary>
    public static class AnalysisProfilerBuilder
    {
        public static AnalysisProfilerSection FromSnapshot(ProfilerSnapshot snapshot)
        {
            var section = new AnalysisProfilerSection
            {
                SessionId = snapshot.SessionId ?? string.Empty,
                Timestamp = snapshot.Timestamp,
                DurationMs = GetDurationMilliseconds(snapshot)
            };

            AppendMetrics(section, snapshot.Metrics);
            AppendCounters(section, snapshot.Counters);
            AppendGauges(section, snapshot.Gauges);
            AppendSamples(section, snapshot.Samples);
            AppendRates(section, snapshot.Rates);
            AppendDurations(section, snapshot.Durations);
            AppendEvents(section, snapshot.Events);
            AppendFlame(section, snapshot.Root);
            return section;
        }

        private static void AppendMetrics(AnalysisProfilerSection section, Dictionary<string, MetricDefinition> metrics)
        {
            if (metrics == null) return;

            foreach (var kvp in metrics)
            {
                var record = kvp.Value;
                var item = new AnalysisMetricDefinition
                {
                    Name = string.IsNullOrEmpty(record.Name) ? kvp.Key : record.Name,
                    Category = record.Category ?? string.Empty,
                    Kind = record.Kind.ToString(),
                    Unit = record.Unit ?? string.Empty,
                    Description = record.Description ?? string.Empty
                };

                if (record.Tags != null)
                {
                    for (var i = 0; i < record.Tags.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(record.Tags[i])) item.Tags.Add(record.Tags[i]);
                    }
                }

                section.Metrics.Add(item);
            }
        }

        private static void AppendCounters(AnalysisProfilerSection section, Dictionary<string, CounterRecord> counters)
        {
            if (counters == null) return;

            foreach (var kvp in counters)
            {
                var record = kvp.Value;
                section.Counters.Add(new AnalysisCounterRecord
                {
                    Name = string.IsNullOrEmpty(record.Name) ? kvp.Key : record.Name,
                    Value = record.Value,
                    Delta = record.Delta,
                    SampleCount = record.SampleCount
                });
            }
        }

        private static void AppendGauges(AnalysisProfilerSection section, Dictionary<string, GaugeRecord> gauges)
        {
            if (gauges == null) return;

            foreach (var kvp in gauges)
            {
                var record = kvp.Value;
                section.Gauges.Add(new AnalysisGaugeRecord
                {
                    Name = string.IsNullOrEmpty(record.Name) ? kvp.Key : record.Name,
                    Value = record.Value,
                    Timestamp = record.Timestamp
                });
            }
        }

        private static void AppendSamples(AnalysisProfilerSection section, Dictionary<string, List<double>> samples)
        {
            if (samples == null) return;

            foreach (var kvp in samples)
            {
                section.Samples.Add(CreateSampleSummary(kvp.Key, kvp.Value));
            }
        }

        private static void AppendRates(AnalysisProfilerSection section, Dictionary<string, RateRecord> rates)
        {
            if (rates == null) return;

            foreach (var kvp in rates)
            {
                var record = kvp.Value;
                section.Rates.Add(new AnalysisRateRecord
                {
                    Name = string.IsNullOrEmpty(record.Name) ? kvp.Key : record.Name,
                    Total = record.TotalCount,
                    Count1Second = record.Count1Second,
                    Count5Seconds = record.Count5Seconds,
                    Count60Seconds = record.Count60Seconds,
                    PeakPerSecond = record.PeakPerSecond
                });
            }
        }

        private static void AppendDurations(AnalysisProfilerSection section, Dictionary<string, DurationSummaryRecord> durations)
        {
            if (durations == null) return;

            foreach (var kvp in durations)
            {
                var record = kvp.Value;
                section.Durations.Add(new AnalysisDurationSummary
                {
                    Name = string.IsNullOrEmpty(record.Name) ? kvp.Key : record.Name,
                    Count = record.Count,
                    SumMs = record.SumMilliseconds,
                    MeanMs = record.MeanMilliseconds,
                    MinMs = record.MinMilliseconds,
                    MaxMs = record.MaxMilliseconds
                });
            }
        }

        private static void AppendEvents(AnalysisProfilerSection section, List<DiagnosticEventRecord> events)
        {
            if (events == null) return;

            for (var i = 0; i < events.Count; i++)
            {
                var record = events[i];
                section.Events.Add(new AnalysisProfilerEvent
                {
                    Timestamp = record.Timestamp,
                    Severity = record.Severity.ToString(),
                    Category = record.Category ?? string.Empty,
                    Name = record.Name ?? string.Empty,
                    Message = record.Message ?? string.Empty,
                    Value = record.Value,
                    Threshold = record.Threshold
                });
            }
        }

        private static void AppendFlame(AnalysisProfilerSection section, FlameRoot root)
        {
            if (root == null) return;

            foreach (var flameRoot in root.Roots.Values)
            {
                section.Flame.Add(CreateFlameNode(flameRoot));
            }
        }

        private static AnalysisSampleSummary CreateSampleSummary(string name, List<double> values)
        {
            var summary = new AnalysisSampleSummary { Name = name ?? string.Empty };
            if (values == null || values.Count == 0) return summary;

            double sum = 0d;
            var min = double.MaxValue;
            var max = double.MinValue;
            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                sum += value;
                if (value < min) min = value;
                if (value > max) max = value;
            }

            summary.Count = values.Count;
            summary.Sum = sum;
            summary.Mean = sum / values.Count;
            summary.Min = min;
            summary.Max = max;
            return summary;
        }

        private static AnalysisFlameNode CreateFlameNode(FlameNode node)
        {
            var result = new AnalysisFlameNode
            {
                Name = node != null ? node.Name ?? string.Empty : string.Empty,
                Category = node != null ? node.Category ?? string.Empty : string.Empty,
                TotalMs = node != null ? node.TotalNanoseconds / 1000000.0d : 0d,
                SelfMs = node != null ? node.SelfNanoseconds / 1000000.0d : 0d,
                Hits = node != null ? node.HitCount : 0
            };

            if (node == null) return result;

            foreach (var child in node.Children.Values)
            {
                result.Children.Add(CreateFlameNode(child));
            }

            return result;
        }

        private static double GetDurationMilliseconds(ProfilerSnapshot snapshot)
        {
            if (snapshot.Root == null) return 0d;
            return Math.Max(0L, snapshot.Root.EndTimestamp - snapshot.Root.StartTimestamp);
        }
    }
}

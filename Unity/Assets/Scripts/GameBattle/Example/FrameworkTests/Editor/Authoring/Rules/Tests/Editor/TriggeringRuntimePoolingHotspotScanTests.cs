using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace AbilityKit.Triggering.Tests
{
    public sealed class TriggeringRuntimePoolingHotspotScanTests
    {
        private static readonly Regex PooledRuntimeAllocationPattern = new Regex(
            @"new\s+(ActionInstance|DefaultActionExecutor|QueuedActionExecutor|RetryActionExecutor|ScheduleToBehaviorContextAdapter)\s*\(",
            RegexOptions.Compiled);

        [Test]
        public void RuntimePooledHotspots_OnlyAllocateInPoolsOrFallbackPaths()
        {
            var packageRoot = FindPackageRoot();
            var runtimeRoot = Path.Combine(packageRoot, "Runtime");
            var allowed = new HashSet<string>(StringComparer.Ordinal)
            {
                Normalize(Path.Combine(runtimeRoot, "Pooling", "TriggeringRuntimePools.cs")),
                Normalize(Path.Combine(runtimeRoot, "Scheduling", "Actions", "ActionScheduler.cs")),
                Normalize(Path.Combine(runtimeRoot, "Scheduling", "Strategies", "SchedulableBehaviorScheduleAdapter.cs"))
            };
            var violations = new List<string>();

            foreach (var file in Directory.EnumerateFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories))
            {
                var normalized = Normalize(file);
                var text = File.ReadAllText(file);
                var matches = PooledRuntimeAllocationPattern.Matches(text);
                if (matches.Count == 0)
                    continue;

                if (!allowed.Contains(normalized))
                {
                    violations.Add(normalized);
                    continue;
                }

                if (normalized.EndsWith("/ActionScheduler.cs", StringComparison.Ordinal))
                {
                    AssertFallbackPath(text, "_pools != null", "ActionScheduler.cs");
                }
                else if (normalized.EndsWith("/SchedulableBehaviorScheduleAdapter.cs", StringComparison.Ordinal))
                {
                    AssertFallbackPath(text, "_pools != null", "SchedulableBehaviorScheduleAdapter.cs");
                }
                else if (normalized.EndsWith("/TriggeringRuntimePools.cs", StringComparison.Ordinal))
                {
                    StringAssert.Contains("Scope.GetPool", text);
                }
            }

            Assert.That(violations, Is.Empty, "Pooled runtime hotspot allocations must stay behind TriggeringRuntimePools or explicit no-pool fallback paths.");
        }

        private static void AssertFallbackPath(string text, string guard, string fileName)
        {
            StringAssert.Contains(guard, text, fileName + " must keep pooled and no-pool paths explicit.");
        }

        private static string Normalize(string path)
        {
            return Path.GetFullPath(path).Replace('\\', '/');
        }

        private static string FindPackageRoot()
        {
            var candidates = new[]
            {
                Path.Combine("Packages", "com.abilitykit.triggering"),
                Path.Combine("Unity", "Packages", "com.abilitykit.triggering"),
                Path.Combine("..", "Packages", "com.abilitykit.triggering"),
                Path.Combine("..", "Unity", "Packages", "com.abilitykit.triggering")
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                var fullPath = Path.GetFullPath(candidates[i]);
                if (Directory.Exists(fullPath))
                    return fullPath;
            }

            Assert.Fail("Unable to locate com.abilitykit.triggering package root.");
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace AbilityKit.Triggering.Tests
{
    public sealed class RuntimeCompatibilityLegacyEntryScanTests
    {
        [Test]
        public void PackageExternalSources_DoNotReferenceHistoricalTriggeringEntrypoints()
        {
            var packagesRoot = FindPackagesRoot();
            var forbidden = new[]
            {
                "AbilityKit.Triggering.Runtime.Scheduler",
                "AbilityKit.Triggering.Runtime.Executable",
                "TriggerDispatcherHub",
                "TriggerDispatcherRegistry",
                "EventBusDispatcher",
                "TimedDispatcher"
            };
            var violations = new List<string>();

            foreach (var file in Directory.EnumerateFiles(packagesRoot, "*.cs", SearchOption.AllDirectories))
            {
                var normalized = file.Replace('\\', '/');
                if (normalized.Contains("/com.abilitykit.triggering/", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (normalized.Contains("/Documentation~/", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (normalized.Contains("/Document/", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (normalized.Contains("/Docs/", StringComparison.OrdinalIgnoreCase))
                    continue;

                var text = File.ReadAllText(file);
                for (int i = 0; i < forbidden.Length; i++)
                {
                    if (text.Contains(forbidden[i], StringComparison.Ordinal))
                    {
                        violations.Add($"{normalized}: {forbidden[i]}");
                    }
                }
            }

            Assert.That(violations, Is.Empty, "Package-external code must not call historical triggering entrypoints.");
        }

        private static string FindPackagesRoot()
        {
            var candidates = new[]
            {
                Path.Combine("Packages"),
                Path.Combine("Unity", "Packages"),
                Path.Combine("..", "Packages"),
                Path.Combine("..", "Unity", "Packages")
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                var fullPath = Path.GetFullPath(candidates[i]);
                if (Directory.Exists(Path.Combine(fullPath, "com.abilitykit.triggering")))
                    return fullPath;
            }

            Assert.Fail("Unable to locate Unity/Packages root for runtime compatibility caller scan.");
            return null;
        }
    }
}

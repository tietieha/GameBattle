using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Validation;

namespace AbilityKit.Triggering.Runtime.Compatibility
{
    /// <summary>
    /// Runtime 根目录兼容入口的正式治理清单。
    /// 用于集中记录根目录占位文件、推荐替代路径、兼容状态与删除条件。
    /// </summary>
    public static class RootRuntimeCompatibilityCatalog
    {
        public const string DefaultRemovalGate = "No package references, external samples migrated, Unity meta GUID no longer referenced, and a major compatibility cleanup batch is active.";

        private static readonly CompatibilityEntry[] _entries =
        {
        };

        public static IReadOnlyList<CompatibilityEntry> Entries => _entries;

        public static bool TryGetEntry(string entryPath, out CompatibilityEntry entry)
        {
            if (!string.IsNullOrEmpty(entryPath))
            {
                for (int i = 0; i < _entries.Length; i++)
                {
                    if (string.Equals(_entries[i].EntryPath, entryPath, StringComparison.OrdinalIgnoreCase))
                    {
                        entry = _entries[i];
                        return true;
                    }
                }
            }

            entry = default;
            return false;
        }

        public static RuntimeCompatibilityScanResult ScanRootEntries(IEnumerable<string> rootRuntimeFileNames)
        {
            var issues = new List<RuntimeCompatibilityScanIssue>();
            var actual = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (rootRuntimeFileNames != null)
            {
                foreach (var fileName in rootRuntimeFileNames)
                {
                    var normalized = NormalizeRootFileName(fileName);
                    if (!string.IsNullOrEmpty(normalized) && normalized.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        actual.Add(normalized);
                    }
                }
            }

            foreach (var fileName in actual)
            {
                if (!TryGetEntry(fileName, out _))
                {
                    issues.Add(new RuntimeCompatibilityScanIssue(
                        fileName,
                        ValidationErrorCodes.RUNTIME_COMPATIBILITY_ENTRY_MISSING,
                        $"Root Runtime compatibility entry is not registered: {fileName}"));
                }
            }

            for (int i = 0; i < _entries.Length; i++)
            {
                var entry = _entries[i];
                if (!actual.Contains(entry.EntryPath))
                {
                    issues.Add(new RuntimeCompatibilityScanIssue(
                        entry.EntryPath,
                        ValidationErrorCodes.RUNTIME_COMPATIBILITY_ENTRY_STALE,
                        $"Registered root Runtime compatibility entry is not present on disk: {entry.EntryPath}"));
                }
            }

            return new RuntimeCompatibilityScanResult(issues.ToArray());
        }

        private static string NormalizeRootFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            var normalized = fileName.Replace('\\', '/');
            var index = normalized.LastIndexOf('/');
            return index >= 0 ? normalized.Substring(index + 1) : normalized;
        }

        private static CompatibilityEntry Entry(string entryPath, string replacementPath, ECompatibilityEntryStatus status)
            => new CompatibilityEntry(entryPath, replacementPath, status, DefaultRemovalGate);
    }
}

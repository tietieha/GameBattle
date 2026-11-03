using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.Compatibility;

namespace AbilityKit.Triggering.Validation
{
    public enum ECompatibilityEntryStatus : byte
    {
        Formal = 0,
        Compatibility = 1,
        Legacy = 2,
        Deprecated = 3,
    }

    public readonly struct CompatibilityEntry
    {
        public readonly string EntryPath;
        public readonly string ReplacementPath;
        public readonly ECompatibilityEntryStatus Status;
        public readonly string RemovalGate;

        public CompatibilityEntry(string entryPath, string replacementPath, ECompatibilityEntryStatus status, string removalGate)
        {
            EntryPath = entryPath;
            ReplacementPath = replacementPath;
            Status = status;
            RemovalGate = removalGate;
        }

        public bool IsRemovalCandidate => Status == ECompatibilityEntryStatus.Deprecated || Status == ECompatibilityEntryStatus.Legacy;
    }

    public readonly struct RuntimeCompatibilityScanIssue
    {
        public readonly string EntryPath;
        public readonly string Code;
        public readonly string Message;

        public RuntimeCompatibilityScanIssue(string entryPath, string code, string message)
        {
            EntryPath = entryPath;
            Code = code;
            Message = message;
        }
    }

    public readonly struct RuntimeCompatibilityScanResult
    {
        private readonly RuntimeCompatibilityScanIssue[] _issues;

        public RuntimeCompatibilityScanResult(RuntimeCompatibilityScanIssue[] issues)
        {
            _issues = issues ?? Array.Empty<RuntimeCompatibilityScanIssue>();
        }

        public IReadOnlyList<RuntimeCompatibilityScanIssue> Issues => _issues ?? Array.Empty<RuntimeCompatibilityScanIssue>();
        public bool IsValid => Issues.Count == 0;

        public ValidationResult ToValidationResult(string path = "$.runtimeCompatibility")
        {
            var result = ValidationResult.Success;
            for (int i = 0; i < Issues.Count; i++)
            {
                var issue = Issues[i];
                result.AddWarning(issue.Code, issue.Message, $"{path}.{issue.EntryPath}");
            }

            return result;
        }
    }

    public static class RuntimeCompatibilityCatalog
    {
        public const string DefaultRemovalGate = RootRuntimeCompatibilityCatalog.DefaultRemovalGate;

        public static IReadOnlyList<CompatibilityEntry> Entries => RootRuntimeCompatibilityCatalog.Entries;

        public static bool TryGetEntry(string entryPath, out CompatibilityEntry entry)
            => RootRuntimeCompatibilityCatalog.TryGetEntry(entryPath, out entry);

        public static RuntimeCompatibilityScanResult ScanRootEntries(IEnumerable<string> rootRuntimeFileNames)
            => RootRuntimeCompatibilityCatalog.ScanRootEntries(rootRuntimeFileNames);
    }
}

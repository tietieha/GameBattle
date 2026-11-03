using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.World.Diagnostics
{
    public static class WorldDebugRegistry
    {
        private static readonly Dictionary<string, WorldCompositionReport> Reports = new Dictionary<string, WorldCompositionReport>(StringComparer.Ordinal);
        private static readonly object SyncRoot = new object();

        public static void Report(WorldCompositionReport report)
        {
            if (report == null) return;
            var key = report.WorldId ?? string.Empty;
            lock (SyncRoot)
            {
                Reports[key] = report;
            }
        }

        public static bool TryGet(string worldId, out WorldCompositionReport report)
        {
            lock (SyncRoot)
            {
                return Reports.TryGetValue(worldId ?? string.Empty, out report);
            }
        }

        public static IReadOnlyCollection<WorldCompositionReport> GetAll()
        {
            lock (SyncRoot)
            {
                return new List<WorldCompositionReport>(Reports.Values);
            }
        }

        public static void Clear(string worldId)
        {
            lock (SyncRoot)
            {
                Reports.Remove(worldId ?? string.Empty);
            }
        }

        public static void ClearAll()
        {
            lock (SyncRoot)
            {
                Reports.Clear();
            }
        }
    }
}

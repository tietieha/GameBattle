using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AbilityKit.Diagnostics;

namespace AbilityKit.Diagnostics.Exporters
{
    /// <summary>
    /// Speedscope import format exporter.
    /// </summary>
    public sealed class SpeedScopeExporter : IExporter
    {
        public string Extension => ".speedscope.json";

        public void Export(ProfilerSnapshot snapshot, string filePath)
        {
            JsonExporter.WriteAllText(filePath, ExportToString(snapshot));
        }

        public string ExportToString(ProfilerSnapshot snapshot)
        {
            var frames = new List<string>();
            var frameIndex = new Dictionary<string, int>(StringComparer.Ordinal);
            var events = new List<string>();
            var currentValue = 0L;

            foreach (var root in snapshot.Root.Roots.Values)
            {
                AppendSpeedscopeEvents(root, currentValue, frames, frameIndex, events);
                currentValue += Math.Max(1L, root.TotalNanoseconds);
            }

            var sb = new StringBuilder(4096);
            sb.AppendLine("{");
            sb.AppendLine("  \"$schema\": \"https://www.speedscope.app/file-format-schema.json\",");
            sb.AppendLine("  \"shared\": {");
            sb.AppendLine("    \"frames\": [");
            for (var i = 0; i < frames.Count; i++)
            {
                sb.AppendLine($"      {{\"name\": \"{JsonExporter.EscapeJson(frames[i])}\"}}{(i < frames.Count - 1 ? "," : string.Empty)}");
            }
            sb.AppendLine("    ]");
            sb.AppendLine("  },");
            sb.AppendLine("  \"profiles\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"type\": \"evented\",");
            sb.AppendLine("      \"name\": \"AbilityKit Diagnostics\",");
            sb.AppendLine("      \"unit\": \"nanoseconds\",");
            sb.AppendLine("      \"startValue\": 0,");
            sb.AppendLine($"      \"endValue\": {Math.Max(1L, currentValue)},");
            sb.AppendLine("      \"events\": [");
            for (var i = 0; i < events.Count; i++)
            {
                sb.AppendLine($"        {events[i]}{(i < events.Count - 1 ? "," : string.Empty)}");
            }
            sb.AppendLine("      ]");
            sb.AppendLine("    }");
            sb.AppendLine("  ],");
            sb.AppendLine($"  \"name\": \"AbilityKit Diagnostics {JsonExporter.EscapeJson(snapshot.SessionId)}\",");
            sb.AppendLine("  \"exporter\": \"AbilityKit.Diagnostics\"");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static long AppendSpeedscopeEvents(FlameNode node, long startValue, List<string> frames, Dictionary<string, int> frameIndex, List<string> events)
        {
            var index = GetFrameIndex(node.Name, frames, frameIndex);
            var endValue = startValue + Math.Max(1L, node.TotalNanoseconds);
            events.Add($"{{\"type\": \"O\", \"frame\": {index}, \"at\": {startValue}}}");

            var childStart = startValue;
            foreach (var child in node.Children.Values)
            {
                childStart = AppendSpeedscopeEvents(child, childStart, frames, frameIndex, events);
            }

            events.Add($"{{\"type\": \"C\", \"frame\": {index}, \"at\": {endValue}}}");
            return endValue;
        }

        private static int GetFrameIndex(string name, List<string> frames, Dictionary<string, int> frameIndex)
        {
            name = string.IsNullOrEmpty(name) ? "unnamed" : name;
            if (frameIndex.TryGetValue(name, out var index))
            {
                return index;
            }

            index = frames.Count;
            frames.Add(name);
            frameIndex[name] = index;
            return index;
        }
    }

    /// <summary>
    /// Chrome trace event exporter.
    /// </summary>
    public sealed class ChromePerfExporter : IExporter
    {
        public string Extension => ".chrome-perf.json";

        public void Export(ProfilerSnapshot snapshot, string filePath)
        {
            JsonExporter.WriteAllText(filePath, ExportToString(snapshot));
        }

        public string ExportToString(ProfilerSnapshot snapshot)
        {
            var events = new List<string>();
            var startMicroseconds = 0d;
            foreach (var root in snapshot.Root.Roots.Values)
            {
                AppendTraceEvents(root, startMicroseconds, 0, events);
                startMicroseconds += Math.Max(1d, root.TotalNanoseconds / 1000.0d);
            }

            var sb = new StringBuilder(4096);
            sb.AppendLine("{");
            sb.AppendLine("  \"traceEvents\": [");
            for (var i = 0; i < events.Count; i++)
            {
                sb.AppendLine($"    {events[i]}{(i < events.Count - 1 ? "," : string.Empty)}");
            }
            sb.AppendLine("  ],");
            sb.AppendLine("  \"metadata\": {");
            sb.AppendLine($"    \"sessionId\": \"{JsonExporter.EscapeJson(snapshot.SessionId)}\",");
            sb.AppendLine($"    \"exportedAt\": \"{DateTimeOffset.UtcNow:O}\"");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void AppendTraceEvents(FlameNode node, double startMicroseconds, int depth, List<string> events)
        {
            var category = string.IsNullOrEmpty(node.Category) ? "default" : node.Category;
            var durationMicroseconds = Math.Max(1d, node.TotalNanoseconds / 1000.0d);
            events.Add("{"
                + $"\"name\":\"{JsonExporter.EscapeJson(node.Name)}\","
                + $"\"cat\":\"{JsonExporter.EscapeJson(category)}\","
                + "\"ph\":\"X\","
                + $"\"ts\":{startMicroseconds.ToString("F3", CultureInfo.InvariantCulture)},"
                + $"\"dur\":{durationMicroseconds.ToString("F3", CultureInfo.InvariantCulture)},"
                + "\"pid\":1,"
                + $"\"tid\":{depth + 1},"
                + $"\"args\":{{\"hits\":{node.HitCount},\"selfMs\":{JsonExporter.Format(node.SelfNanoseconds / 1000000.0d)}}}"
                + "}");

            var childStart = startMicroseconds;
            foreach (var child in node.Children.Values)
            {
                AppendTraceEvents(child, childStart, depth + 1, events);
                childStart += Math.Max(1d, child.TotalNanoseconds / 1000.0d);
            }
        }
    }

    /// <summary>
    /// Brendan Gregg folded stack exporter.
    /// </summary>
    public sealed class FlameGraphExporter : IExporter
    {
        public string Extension => ".folded.txt";

        public void Export(ProfilerSnapshot snapshot, string filePath)
        {
            JsonExporter.WriteAllText(filePath, ExportToString(snapshot));
        }

        public string ExportToString(ProfilerSnapshot snapshot)
        {
            var sb = new StringBuilder(2048);
            foreach (var root in snapshot.Root.Roots.Values)
            {
                FoldedExporter.AppendFolded(sb, root, string.Empty);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Simple tree JSON exporter for flamegraph visualizers.
    /// </summary>
    public sealed class ZeroxExporter : IExporter
    {
        public string Extension => ".0x.json";

        public void Export(ProfilerSnapshot snapshot, string filePath)
        {
            JsonExporter.WriteAllText(filePath, ExportToString(snapshot));
        }

        public string ExportToString(ProfilerSnapshot snapshot)
        {
            var roots = new List<FlameNode>(snapshot.Root.Roots.Values);
            var sb = new StringBuilder(4096);
            sb.AppendLine("{");
            sb.AppendLine("  \"name\": \"AbilityKit Diagnostics\",");
            sb.AppendLine("  \"children\": [");
            for (var i = 0; i < roots.Count; i++)
            {
                AppendTreeNode(sb, roots[i], 2, i < roots.Count - 1);
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void AppendTreeNode(StringBuilder sb, FlameNode node, int indentLevel, bool comma)
        {
            var indent = new string(' ', indentLevel * 2);
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}  \"name\": \"{JsonExporter.EscapeJson(node.Name)}\",");
            sb.AppendLine($"{indent}  \"value\": {Math.Max(1L, node.SelfNanoseconds > 0 ? node.SelfNanoseconds : node.TotalNanoseconds)},");
            sb.AppendLine($"{indent}  \"children\": [");

            var children = new List<FlameNode>(node.Children.Values);
            for (var i = 0; i < children.Count; i++)
            {
                AppendTreeNode(sb, children[i], indentLevel + 2, i < children.Count - 1);
            }

            sb.AppendLine($"{indent}  ]");
            sb.AppendLine($"{indent}}}{(comma ? "," : string.Empty)}");
        }
    }
}

using System;
using System.IO;
using System.Text;
using AbilityKit.Diagnostics.Analysis;
using Newtonsoft.Json;

namespace AbilityKit.Diagnostics.Exporters
{
    /// <summary>
    /// Exports the stable Web-facing AbilityKit analysis artifact as JSON.
    /// </summary>
    public interface IAnalysisArtifactExporter
    {
        string Extension { get; }
        void Export(AbilityKitAnalysisArtifact artifact, string filePath);
        string ExportToString(AbilityKitAnalysisArtifact artifact);
    }

    public sealed class AnalysisArtifactJsonExporter : IAnalysisArtifactExporter
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include
        };

        public string Extension => ".analysis.json";

        public void Export(AbilityKitAnalysisArtifact artifact, string filePath)
        {
            WriteAllText(filePath, ExportToString(artifact));
        }

        public string ExportToString(AbilityKitAnalysisArtifact artifact)
        {
            if (artifact == null) throw new ArgumentNullException(nameof(artifact));
            if (string.IsNullOrEmpty(artifact.SchemaVersion))
            {
                artifact.SchemaVersion = AbilityKitAnalysisSchema.Version;
            }

            return JsonConvert.SerializeObject(artifact, Settings);
        }

        private static void WriteAllText(string filePath, string content)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content, Encoding.UTF8);
        }
    }
}

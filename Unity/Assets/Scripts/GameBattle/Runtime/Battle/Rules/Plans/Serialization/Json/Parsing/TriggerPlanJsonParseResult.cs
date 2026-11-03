using System;
using System.Collections.Generic;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    public enum TriggerPlanJsonFormat
    {
        Unknown = 0,
        Runtime = 1,
        Source = 2
    }

    public enum TriggerPlanJsonDiagnosticSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    public readonly struct TriggerPlanJsonDiagnostic
    {
        public readonly TriggerPlanJsonDiagnosticSeverity Severity;
        public readonly string SourceName;
        public readonly string Path;
        public readonly string Message;
        public readonly Exception Exception;

        public TriggerPlanJsonDiagnostic(
            TriggerPlanJsonDiagnosticSeverity severity,
            string message,
            string sourceName = null,
            string path = null,
            Exception exception = null)
        {
            Severity = severity;
            SourceName = sourceName;
            Path = path;
            Message = message;
            Exception = exception;
        }

        public override string ToString()
        {
            var location = string.IsNullOrEmpty(SourceName) ? string.Empty : SourceName;
            if (!string.IsNullOrEmpty(Path))
            {
                location = string.IsNullOrEmpty(location) ? Path : location + ":" + Path;
            }

            return string.IsNullOrEmpty(location)
                ? $"[{Severity}] {Message}"
                : $"[{Severity}] {location}: {Message}";
        }
    }

    public sealed class TriggerPlanJsonParseOptions
    {
        public bool RequireExplicitSourceFormat { get; set; }
        public bool TreatWarningsAsErrors { get; set; }

        public static TriggerPlanJsonParseOptions Default { get; } = new TriggerPlanJsonParseOptions();
    }

    internal sealed class TriggerPlanJsonParseResult
    {
        private readonly List<TriggerPlanJsonDiagnostic> _diagnostics;

        public TriggerPlanJsonParseResult(
            bool success,
            TriggerPlanJsonFormat format,
            TriggerPlanJsonDatabase.TriggerPlanDatabaseDto dto,
            List<TriggerPlanJsonDiagnostic> diagnostics)
        {
            Success = success;
            Format = format;
            Dto = dto;
            _diagnostics = diagnostics ?? new List<TriggerPlanJsonDiagnostic>();
        }

        public bool Success { get; }

        public TriggerPlanJsonFormat Format { get; }

        public TriggerPlanJsonDatabase.TriggerPlanDatabaseDto Dto { get; }

        public IReadOnlyList<TriggerPlanJsonDiagnostic> Diagnostics => _diagnostics;

        public TriggerPlanJsonDiagnostic FirstError
        {
            get
            {
                for (int i = 0; i < _diagnostics.Count; i++)
                {
                    if (_diagnostics[i].Severity == TriggerPlanJsonDiagnosticSeverity.Error)
                    {
                        return _diagnostics[i];
                    }
                }

                return default;
            }
        }
    }
}

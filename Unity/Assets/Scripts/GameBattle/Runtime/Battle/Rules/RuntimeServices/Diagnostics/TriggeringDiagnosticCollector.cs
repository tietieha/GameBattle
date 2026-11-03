using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.Plan.Json;
using AbilityKit.Triggering.Validation;

namespace AbilityKit.Triggering.Runtime.Diagnostics
{
    public enum TriggeringDiagnosticEventKind
    {
        Validation = 0,
        JsonLoad = 1,
        Execution = 2,
        Schedule = 3,
        LegacyHit = 4
    }

    public readonly struct TriggeringDiagnosticRecord
    {
        public readonly TriggeringDiagnosticEventKind Kind;
        public readonly string Source;
        public readonly string Path;
        public readonly string Code;
        public readonly string Message;
        public readonly int ErrorCount;
        public readonly int WarningCount;
        public readonly int InfoCount;
        public readonly int TriggerId;
        public readonly int ActionId;
        public readonly string ScheduleHandle;

        public TriggeringDiagnosticRecord(
            TriggeringDiagnosticEventKind kind,
            string source = null,
            string path = null,
            string code = null,
            string message = null,
            int errorCount = 0,
            int warningCount = 0,
            int infoCount = 0,
            int triggerId = 0,
            int actionId = 0,
            string scheduleHandle = null)
        {
            Kind = kind;
            Source = source;
            Path = path;
            Code = code;
            Message = message;
            ErrorCount = errorCount;
            WarningCount = warningCount;
            InfoCount = infoCount;
            TriggerId = triggerId;
            ActionId = actionId;
            ScheduleHandle = scheduleHandle;
        }
    }

    public readonly struct TriggeringDiagnosticSnapshot
    {
        public readonly int ValidationErrors;
        public readonly int ValidationWarnings;
        public readonly int ValidationInfos;
        public readonly int JsonErrors;
        public readonly int JsonWarnings;
        public readonly int JsonInfos;
        public readonly int ExecutionFailures;
        public readonly int ScheduleEvents;
        public readonly int LegacyHits;
        public readonly int TotalRecords;

        public TriggeringDiagnosticSnapshot(
            int validationErrors,
            int validationWarnings,
            int validationInfos,
            int jsonErrors,
            int jsonWarnings,
            int jsonInfos,
            int executionFailures,
            int scheduleEvents,
            int legacyHits,
            int totalRecords)
        {
            ValidationErrors = validationErrors;
            ValidationWarnings = validationWarnings;
            ValidationInfos = validationInfos;
            JsonErrors = jsonErrors;
            JsonWarnings = jsonWarnings;
            JsonInfos = jsonInfos;
            ExecutionFailures = executionFailures;
            ScheduleEvents = scheduleEvents;
            LegacyHits = legacyHits;
            TotalRecords = totalRecords;
        }
    }

    public sealed class TriggeringDiagnosticCollector
    {
        private readonly List<TriggeringDiagnosticRecord> _records = new List<TriggeringDiagnosticRecord>();
        private int _validationErrors;
        private int _validationWarnings;
        private int _validationInfos;
        private int _jsonErrors;
        private int _jsonWarnings;
        private int _jsonInfos;
        private int _executionFailures;
        private int _scheduleEvents;
        private int _legacyHits;

        public IReadOnlyList<TriggeringDiagnosticRecord> Records => _records;

        public TriggeringDiagnosticSnapshot Snapshot => new TriggeringDiagnosticSnapshot(
            _validationErrors,
            _validationWarnings,
            _validationInfos,
            _jsonErrors,
            _jsonWarnings,
            _jsonInfos,
            _executionFailures,
            _scheduleEvents,
            _legacyHits,
            _records.Count);

        public void Clear()
        {
            _records.Clear();
            _validationErrors = 0;
            _validationWarnings = 0;
            _validationInfos = 0;
            _jsonErrors = 0;
            _jsonWarnings = 0;
            _jsonInfos = 0;
            _executionFailures = 0;
            _scheduleEvents = 0;
            _legacyHits = 0;
        }

        public void RecordValidation(in ValidationResult result, string source = null)
        {
            _validationErrors += result.Errors.Count;
            _validationWarnings += result.Warnings.Count;
            _validationInfos += result.Infos.Count;
            _records.Add(new TriggeringDiagnosticRecord(
                TriggeringDiagnosticEventKind.Validation,
                source,
                errorCount: result.Errors.Count,
                warningCount: result.Warnings.Count,
                infoCount: result.Infos.Count));
        }

        public void RecordJsonDiagnostics(IEnumerable<TriggerPlanJsonDiagnostic> diagnostics)
        {
            if (diagnostics == null)
                return;

            foreach (var diagnostic in diagnostics)
            {
                switch (diagnostic.Severity)
                {
                    case TriggerPlanJsonDiagnosticSeverity.Error:
                        _jsonErrors++;
                        break;
                    case TriggerPlanJsonDiagnosticSeverity.Warning:
                        _jsonWarnings++;
                        break;
                    default:
                        _jsonInfos++;
                        break;
                }

                _records.Add(new TriggeringDiagnosticRecord(
                    TriggeringDiagnosticEventKind.JsonLoad,
                    diagnostic.SourceName,
                    diagnostic.Path,
                    diagnostic.Severity.ToString(),
                    diagnostic.Message));
            }
        }

        public void RecordExecutionFailure(int triggerId, int actionId, string message, string path = null)
        {
            _executionFailures++;
            _records.Add(new TriggeringDiagnosticRecord(
                TriggeringDiagnosticEventKind.Execution,
                path: path,
                message: message,
                triggerId: triggerId,
                actionId: actionId,
                errorCount: 1));
        }

        public void RecordScheduleEvent(string scheduleHandle, string message = null, int triggerId = 0, int actionId = 0)
        {
            _scheduleEvents++;
            _records.Add(new TriggeringDiagnosticRecord(
                TriggeringDiagnosticEventKind.Schedule,
                message: message,
                triggerId: triggerId,
                actionId: actionId,
                scheduleHandle: scheduleHandle));
        }

        public void RecordLegacyHit(string entry, string replacement, string caller = null)
        {
            _legacyHits++;
            _records.Add(new TriggeringDiagnosticRecord(
                TriggeringDiagnosticEventKind.LegacyHit,
                source: caller,
                path: entry,
                message: replacement,
                warningCount: 1));
        }
    }
}

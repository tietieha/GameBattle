using System;
using System.Collections.Generic;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    public sealed class TriggerPlanDirectoryLoadOptions
    {
        public bool ThrowOnFileParseError { get; set; }

        public bool RequireExplicitSourceFormat { get; set; }

        public bool TreatWarningsAsErrors { get; set; }

        public TriggerPlanJsonDatabase.ICueFactory CueFactory { get; set; }

        public IList<TriggerPlanJsonDiagnostic> Diagnostics { get; set; }

        public static TriggerPlanDirectoryLoadOptions Default { get; } = new TriggerPlanDirectoryLoadOptions();

        internal TriggerPlanJsonParseOptions ToParseOptions()
        {
            return new TriggerPlanJsonParseOptions
            {
                RequireExplicitSourceFormat = RequireExplicitSourceFormat,
                TreatWarningsAsErrors = TreatWarningsAsErrors
            };
        }

        internal void AddDiagnostic(TriggerPlanJsonDiagnostic diagnostic)
        {
            Diagnostics?.Add(diagnostic);
        }

        internal void AddDiagnostics(IEnumerable<TriggerPlanJsonDiagnostic> diagnostics)
        {
            if (Diagnostics == null || diagnostics == null)
            {
                return;
            }

            foreach (var diagnostic in diagnostics)
            {
                Diagnostics.Add(diagnostic);
            }
        }
    }
}

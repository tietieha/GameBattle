using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Services
{
    public sealed class MobaContextIntegrityRuntimeValidator : IMobaRuntimeValidator
    {
        public const string SourceName = "runtime.context.integrity";

        public string Name => SourceName;

        public void Validate(in MobaRuntimeValidationContext context, MobaRuntimeValidationReport report)
        {
            if (report == null) return;

            if (!context.TryResolve<IMobaContinuousRuntimeQueryService>(out var queryService))
            {
                report.Warning(Name, "continuous.runtime.query", "IMobaContinuousRuntimeQueryService is missing; continuous runtime context integrity cannot be inspected.", nameof(IMobaContinuousRuntimeQueryService));
                return;
            }

            var runtimes = queryService.GetAllContinuous(includeTerminated: false);
            if (runtimes == null || runtimes.Count == 0)
            {
                report.Info(Name, "continuous.runtime", "No active continuous runtime requires context integrity inspection.");
                return;
            }

            for (int i = 0; i < runtimes.Count; i++)
            {
                ValidateRuntime(runtimes[i], report);
            }
        }

        private void ValidateRuntime(MobaContinuousRuntimeView runtime, MobaRuntimeValidationReport report)
        {
            if (runtime == null || !runtime.IsActive || runtime.IsTerminated) return;

            var path = BuildPath(runtime);
            var businessId = string.IsNullOrEmpty(runtime.Id) ? runtime.ConfigId.ToString() : runtime.Id;
            var source = runtime.ContextSource;

            if (!source.IsValid)
            {
                report.Warning(Name, path + ".contextSource", "Active continuous runtime has no resolvable context source; trace, replay and diagnostics lineage may be incomplete.", businessId);
                return;
            }

            if (!source.HasExecutionSource)
            {
                report.Warning(Name, path + ".executionSource", "Active continuous runtime context source does not contain both source actor and source context id.", businessId);
            }

            if (runtime.SourceActorId <= 0 && source.SourceActorId <= 0)
            {
                report.Warning(Name, path + ".sourceActor", "Active continuous runtime is missing source actor identity.", businessId);
            }

            if (runtime.SourceContextId == 0 && source.SourceContextId == 0)
            {
                report.Warning(Name, path + ".sourceContext", "Active continuous runtime is missing source context id.", businessId);
            }

            if (runtime.RootContextId == 0 && source.RootContextId == 0 && runtime.SourceContextId == 0 && source.SourceContextId == 0)
            {
                report.Warning(Name, path + ".rootContext", "Active continuous runtime cannot resolve root context id or fallback source context id.", businessId);
            }

            if (runtime.OwnerActorId <= 0 && runtime.OwnerId == 0 && source.OwnerContextId == 0)
            {
                report.Warning(Name, path + ".owner", "Active continuous runtime is missing owner actor or owner context identity.", businessId);
            }

            if (runtime.ContextSourceBoundary == MobaContextSourceBoundary.LiveRuntime && !runtime.HasLiveRuntimeSource)
            {
                report.Warning(Name, path + ".liveRuntime", "Active continuous runtime declares live runtime boundary but source is not linked to a live runtime.", businessId);
            }
        }

        private static string BuildPath(MobaContinuousRuntimeView runtime)
        {
            var kind = string.IsNullOrEmpty(runtime.Kind) ? "unknown" : runtime.Kind;
            return "continuous.runtime." + kind + "." + runtime.ConfigId;
        }
    }
}

using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host.Extensions.Moba.Runtime;

namespace AbilityKit.Demo.Moba.Services
{
    public sealed class MobaBattleRuntimeReadinessValidator : IMobaRuntimeValidator
    {
        private const string Source = "runtime.readiness";
        private const string MetricRuntimeReady = "moba.runtime.ready";
        private const string MetricSnapshotReady = "moba.snapshot.ready";
        private const string MetricSnapshotEmitters = "moba.snapshot.emitters";
        private const string MetricAuthorityFrameReady = "moba.frames.authority.ready";

        public string Name => Source;

        public void Validate(in MobaRuntimeValidationContext context, MobaRuntimeValidationReport report)
        {
            if (report == null) return;

            context.TryResolve<IMobaBattleDiagnosticsService>(out var diagnostics);
            ValidateRuntimePort(in context, report, diagnostics);
            ValidateSnapshotHealth(in context, report, diagnostics);
            ValidateAuthorityFrames(in context, report, diagnostics);
        }

        private static void ValidateRuntimePort(in MobaRuntimeValidationContext context, MobaRuntimeValidationReport report, IMobaBattleDiagnosticsService diagnostics)
        {
            if (!context.TryResolve<IMobaBattleRuntimePort>(out var runtime) || runtime == null)
            {
                report.Error(Source, "runtime.port", "IMobaBattleRuntimePort is required as the single formal battle runtime entry.", nameof(IMobaBattleRuntimePort), blocksStartup: true);
                diagnostics?.Gauge(MetricRuntimeReady, 0);
                return;
            }

            var status = runtime.Status;
            if (!status.IsReadyForBattleLoop)
            {
                report.Error(Source, "runtime.port.capabilities", "Battle runtime port is not ready for the battle loop. " + status, nameof(IMobaBattleRuntimePort), blocksStartup: true);
                diagnostics?.Gauge(MetricRuntimeReady, 0);
                return;
            }

            if (!status.IsReadyForGameStart)
            {
                report.Warning(Source, "runtime.port.game_start", "Battle runtime port is missing game start capability. " + status, nameof(IMobaBattleRuntimePort));
            }

            diagnostics?.Gauge(MetricRuntimeReady, 1);
        }

        private static void ValidateSnapshotHealth(in MobaRuntimeValidationContext context, MobaRuntimeValidationReport report, IMobaBattleDiagnosticsService diagnostics)
        {
            if (!context.TryResolve<IMobaSnapshotHealthProvider>(out var healthProvider) || healthProvider == null)
            {
                report.Error(Source, "snapshot.health", "IMobaSnapshotHealthProvider is required for snapshot readiness diagnostics.", nameof(IMobaSnapshotHealthProvider), blocksStartup: true);
                diagnostics?.Gauge(MetricSnapshotReady, 0);
                return;
            }

            var health = healthProvider.GetHealth();
            diagnostics?.Gauge(MetricSnapshotEmitters, health.EmitterCount);

            if (!health.HasEmitters)
            {
                report.Error(Source, "snapshot.emitters", "Snapshot router has no emitters; battle state output cannot be produced. " + health, nameof(IMobaSnapshotHealthProvider), blocksStartup: true);
                diagnostics?.Gauge(MetricSnapshotReady, 0);
                return;
            }

            diagnostics?.Gauge(MetricSnapshotReady, 1);
        }

        private static void ValidateAuthorityFrames(in MobaRuntimeValidationContext context, MobaRuntimeValidationReport report, IMobaBattleDiagnosticsService diagnostics)
        {
            if (!context.TryResolve<MobaAuthorityFrameService>(out var frames) || frames == null)
            {
                report.Warning(Source, "frames.authority", "MobaAuthorityFrameService is not resolved; frame diagnostics cannot report confirmed/predicted frame readiness.", nameof(MobaAuthorityFrameService));
                diagnostics?.Gauge(MetricAuthorityFrameReady, 0);
                return;
            }

            if (!frames.TryGetFrames(out FrameIndex confirmed, out FrameIndex predicted))
            {
                report.Warning(Source, "frames.authority.current", "Authority frame service cannot provide current confirmed/predicted frames during readiness validation.", nameof(MobaAuthorityFrameService));
                diagnostics?.Gauge(MetricAuthorityFrameReady, 0);
                return;
            }

            if (predicted.Value < confirmed.Value)
            {
                report.Warning(Source, "frames.authority.order", $"Predicted frame is behind confirmed frame. confirmed={confirmed.Value}, predicted={predicted.Value}", nameof(MobaAuthorityFrameService));
            }

            diagnostics?.Gauge(MetricAuthorityFrameReady, 1);
        }
    }
}

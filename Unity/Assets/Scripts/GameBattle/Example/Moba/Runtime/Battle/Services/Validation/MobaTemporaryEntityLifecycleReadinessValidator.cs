using AbilityKit.Demo.Moba.Services.Area;
using AbilityKit.Demo.Moba.Services.Projectile;

namespace AbilityKit.Demo.Moba.Services
{
    public sealed class MobaTemporaryEntityLifecycleReadinessValidator : IMobaRuntimeValidator
    {
        private const string Source = "temp_entity.lifecycle";
        private const string MetricReady = "moba.temp_entity.lifecycle.ready";
        private const string MetricActiveMismatch = "moba.temp_entity.lifecycle.active_mismatch";

        public string Name => Source;

        public void Validate(in MobaRuntimeValidationContext context, MobaRuntimeValidationReport report)
        {
            if (report == null) return;

            context.TryResolve<IMobaBattleDiagnosticsService>(out var diagnostics);
            if (!context.TryResolve<IMobaTemporaryEntityLifecycleHealthProvider>(out var healthProvider) || healthProvider == null)
            {
                report.Error(Source, "health.provider", "IMobaTemporaryEntityLifecycleHealthProvider is required for projectile/area/summon lifecycle governance.", nameof(IMobaTemporaryEntityLifecycleHealthProvider), blocksStartup: true);
                diagnostics?.Gauge(MetricReady, 0);
                return;
            }

            var mismatchCount = 0;
            mismatchCount += ValidateProjectile(context, healthProvider, report);
            mismatchCount += ValidateArea(context, healthProvider, report);
            mismatchCount += ValidateSummon(context, healthProvider, report);

            diagnostics?.Gauge(MetricActiveMismatch, mismatchCount);
            diagnostics?.Gauge(MetricReady, 1);
        }

        private static int ValidateProjectile(in MobaRuntimeValidationContext context, IMobaTemporaryEntityLifecycleHealthProvider healthProvider, MobaRuntimeValidationReport report)
        {
            if (!context.TryResolve<MobaProjectileLinkService>(out var links) || links == null)
            {
                report.Error(Source, "projectile.links", "MobaProjectileLinkService is required to verify active projectile lifecycle state.", nameof(MobaProjectileLinkService), blocksStartup: true);
                return 1;
            }

            var health = healthProvider.GetHealth(MobaTemporaryEntityKind.Projectile);
            if (health.ActiveCount != links.ActiveCount)
            {
                report.Warning(Source, "projectile.active", $"Projectile lifecycle active count differs from link service. lifecycle={health.ActiveCount}, links={links.ActiveCount}", nameof(MobaProjectileLinkService));
                return 1;
            }

            return 0;
        }

        private static int ValidateArea(in MobaRuntimeValidationContext context, IMobaTemporaryEntityLifecycleHealthProvider healthProvider, MobaRuntimeValidationReport report)
        {
            if (!context.TryResolve<MobaAreaRuntimeService>(out var areas) || areas == null)
            {
                report.Error(Source, "area.runtime", "MobaAreaRuntimeService is required to verify active area lifecycle state.", nameof(MobaAreaRuntimeService), blocksStartup: true);
                return 1;
            }

            var health = healthProvider.GetHealth(MobaTemporaryEntityKind.Area);
            if (health.ActiveCount != areas.ActiveCount)
            {
                report.Warning(Source, "area.active", $"Area lifecycle active count differs from area runtime. lifecycle={health.ActiveCount}, runtime={areas.ActiveCount}", nameof(MobaAreaRuntimeService));
                return 1;
            }

            return 0;
        }

        private static int ValidateSummon(in MobaRuntimeValidationContext context, IMobaTemporaryEntityLifecycleHealthProvider healthProvider, MobaRuntimeValidationReport report)
        {
            if (!context.TryResolve<MobaSummonService>(out var summons) || summons == null)
            {
                report.Error(Source, "summon.service", "MobaSummonService is required to verify active summon lifecycle state.", nameof(MobaSummonService), blocksStartup: true);
                return 1;
            }

            var health = healthProvider.GetHealth(MobaTemporaryEntityKind.Summon);
            if (health.ActiveCount != summons.ActiveCount)
            {
                report.Warning(Source, "summon.active", $"Summon lifecycle active count differs from summon service. lifecycle={health.ActiveCount}, service={summons.ActiveCount}", nameof(MobaSummonService));
                return 1;
            }

            return 0;
        }
    }
}

#nullable enable

using System;

namespace AbilityKit.Demo.Shooter.View
{
    public sealed class ShooterReconciliationDiagnosticsStream
    {
        public event Action<ShooterClientReconciliationResult>? ReconciliationApplied;

        public void Publish(in ShooterClientReconciliationResult result)
        {
            if (result.ApplyResult == ShooterSnapshotApplyResult.Ignored)
            {
                return;
            }

            ReconciliationApplied?.Invoke(result);
        }
    }
}

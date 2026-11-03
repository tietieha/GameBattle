#nullable enable

using AbilityKit.Network.Runtime.Sync;

namespace AbilityKit.Demo.Shooter.View
{
    /// <summary>
    /// 将 Shooter 示例特定的同步诊断映射到玩法无关的框架契约
    /// （<see cref="SyncTickResult"/> / <see cref="SyncReconciliationReport"/>），让 Shooter 控制器无需改变
    /// 现有面向示例的表面接口，就能满足 <see cref="IClientSyncStrategy{TInput, TSample}"/>。
    ///
    /// 这是迁移步骤 3 的轻量适配接缝：示例保留更丰富的
    /// <see cref="ShooterClientFrameTickResult"/> / <see cref="ShooterClientReconciliationResult"/>
    /// 类型，而这些投影只暴露框架需要的子集。
    /// </summary>
    internal static class ShooterClientSyncStrategyMapping
    {
        public static SyncTickResult ToSyncTickResult(in ShooterClientFrameTickResult tick)
        {
            return new SyncTickResult(tick.Ticks, tick.Frame, tick.StateHash);
        }

        public static SyncReconciliationReason ToReason(ShooterClientResyncReason reason)
        {
            // 框架枚举直接按 Shooter 的原因建模，因此数值当前是一一对应的；这里仍显式映射，
            // 以便未来两侧发生差异时保持稳健。
            switch (reason)
            {
                case ShooterClientResyncReason.ImportFailed: return SyncReconciliationReason.ImportFailed;
                case ShooterClientResyncReason.AuthoritativeHashMismatch: return SyncReconciliationReason.AuthoritativeHashMismatch;
                case ShooterClientResyncReason.ClientHashRejectedByServer: return SyncReconciliationReason.ClientHashRejected;
                case ShooterClientResyncReason.FrameTooFarBehind: return SyncReconciliationReason.FrameTooFarBehind;
                case ShooterClientResyncReason.FrameTooFarAhead: return SyncReconciliationReason.FrameTooFarAhead;
                case ShooterClientResyncReason.SnapshotTimeout: return SyncReconciliationReason.SnapshotTimeout;
                case ShooterClientResyncReason.WorldMismatch: return SyncReconciliationReason.WorldMismatch;
                case ShooterClientResyncReason.None:
                default:
                    return SyncReconciliationReason.None;
            }
        }

        public static SyncRecoveryState ToRecoveryState(ShooterClientRecoveryState state)
        {
            switch (state)
            {
                case ShooterClientRecoveryState.CatchUp: return SyncRecoveryState.CatchUp;
                case ShooterClientRecoveryState.AwaitingFullSnapshot: return SyncRecoveryState.AwaitingFullSnapshot;
                case ShooterClientRecoveryState.ApplyingFullSnapshot: return SyncRecoveryState.ApplyingFullSnapshot;
                case ShooterClientRecoveryState.Recovered: return SyncRecoveryState.Recovered;
                case ShooterClientRecoveryState.Normal:
                default:
                    return SyncRecoveryState.Normal;
            }
        }

        /// <summary>
        /// 基于控制器当前诊断状态构建框架校正报告。
        /// 分歧帧/哈希来自控制器最近一次重同步元数据，重放 tick 数来自最近一次校正结果。
        /// </summary>
        public static SyncReconciliationReport ToReconciliationReport(IShooterClientSyncController controller)
        {
            return new SyncReconciliationReport(
                ToReason(controller.LastResyncReason),
                ToRecoveryState(controller.RecoveryState),
                controller.NeedsFullSnapshotResync,
                controller.LastResyncClientFrame,
                controller.LastResyncAuthoritativeFrame,
                controller.LastResyncClientStateHash,
                controller.LastResyncAuthoritativeStateHash,
                controller.LastReconciliationResult.ReplayTicks);
        }
    }
}

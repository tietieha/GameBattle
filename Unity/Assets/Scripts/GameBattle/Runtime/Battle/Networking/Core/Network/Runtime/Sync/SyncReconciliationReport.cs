#nullable enable

namespace AbilityKit.Network.Runtime.Sync
{
    /// <summary>
    /// 客户端同步策略产出的玩法无关校正/追赶诊断。
    /// 它替代示例过去暴露的零散 Last* 字段（例如 Shooter 的 LastResyncReason /
    /// LastResyncClientFrame / LastResyncAuthoritativeStateHash ...），为演示框架提供单一统一方式，
    /// 跨所有同步模型展示“客户端为何分歧，以及如何恢复”。
    ///
    /// 从不进行校正的模型（例如远端 actor 的纯权威插值）只需报告 <see cref="None"/>。
    /// </summary>
    public readonly struct SyncReconciliationReport
    {
        /// <summary>表示未发生校正的空报告。</summary>
        public static readonly SyncReconciliationReport None = new SyncReconciliationReport(
            SyncReconciliationReason.None,
            SyncRecoveryState.Normal,
            needsFullSnapshot: false,
            clientFrame: 0,
            authoritativeFrame: 0,
            clientStateHash: 0u,
            authoritativeStateHash: 0u,
            replayTicks: 0);

        public SyncReconciliationReport(
            SyncReconciliationReason reason,
            SyncRecoveryState recoveryState,
            bool needsFullSnapshot,
            int clientFrame,
            int authoritativeFrame,
            uint clientStateHash,
            uint authoritativeStateHash,
            int replayTicks)
        {
            Reason = reason;
            RecoveryState = recoveryState;
            NeedsFullSnapshot = needsFullSnapshot;
            ClientFrame = clientFrame;
            AuthoritativeFrame = authoritativeFrame;
            ClientStateHash = clientStateHash;
            AuthoritativeStateHash = authoritativeStateHash;
            ReplayTicks = replayTicks;
        }

        /// <summary>最近一次校正被触发的原因（或 <see cref="SyncReconciliationReason.None"/>）。</summary>
        public SyncReconciliationReason Reason { get; }

        /// <summary>策略当前所处的恢复阶段。</summary>
        public SyncRecoveryState RecoveryState { get; }

        /// <summary>策略是否正在等待完整权威快照以恢复。</summary>
        public bool NeedsFullSnapshot { get; }

        /// <summary>检测到分歧时的本地模拟帧。</summary>
        public int ClientFrame { get; }

        /// <summary>分歧涉及的权威服务器帧。</summary>
        public int AuthoritativeFrame { get; }

        /// <summary>分歧点处的本地预测状态哈希。</summary>
        public uint ClientStateHash { get; }

        /// <summary>分歧点处的权威状态哈希。</summary>
        public uint AuthoritativeStateHash { get; }

        /// <summary>最近一次回滚/校正期间重放的 tick 数（没有时为 0）。</summary>
        public int ReplayTicks { get; }

        /// <summary>最近一次校正确实修复了本地状态时为 true。</summary>
        public bool DidReconcile => Reason != SyncReconciliationReason.None;
    }
}

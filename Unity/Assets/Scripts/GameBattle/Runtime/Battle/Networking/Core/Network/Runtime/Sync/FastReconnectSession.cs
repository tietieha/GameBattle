#nullable enable

using System;
using System.Collections.Generic;

namespace AbilityKit.Network.Runtime.Sync
{
    /// <summary>
    /// <see cref="FastReconnectSession"/> 当前所处的生命周期阶段。会话从 <see cref="Connected"/> 开始，
    /// 连接中断后进入 <see cref="Disconnected"/>，重连时根据客户端落后程度选择两条恢复路径之一：
    /// <see cref="Resuming"/>（小 gap，从缓冲增量追帧）或 <see cref="AwaitingFullSnapshot"/>（大 gap，需要新的权威快照）。
    /// 任一路径最终都会进入 <see cref="Recovered"/>。
    /// </summary>
    public enum FastReconnectPhase
    {
        Connected = 0,
        Disconnected = 1,
        Resuming = 2,
        AwaitingFullSnapshot = 3,
        Recovered = 4
    }

    /// <summary>
    /// 单次 <see cref="FastReconnectSession"/> 阶段迁移的结果：会话当前阶段、该迁移的玩法无关校正诊断，
    /// 以及迁移过程中发出的健康事件。Carrier 会把 <see cref="Reconciliation"/> 与 <see cref="HealthEvents"/>
    /// 直接转发到 <see cref="DemoHarness"/> 单步遥测中，让 harness 使用与其他同步模型相同的管线聚合重连视图。
    /// </summary>
    public readonly struct FastReconnectStepReport
    {
        private static readonly SyncHealthEvent[] EmptyEvents = Array.Empty<SyncHealthEvent>();

        private readonly SyncHealthEvent[]? _healthEvents;

        public FastReconnectStepReport(
            FastReconnectPhase phase,
            SyncReconciliationReport reconciliation,
            params SyncHealthEvent[]? healthEvents)
        {
            Phase = phase;
            Reconciliation = reconciliation;
            _healthEvents = healthEvents != null && healthEvents.Length > 0 ? healthEvents : null;
        }

        public FastReconnectPhase Phase { get; }

        public SyncReconciliationReport Reconciliation { get; }

        public IReadOnlyList<SyncHealthEvent> HealthEvents => _healthEvents ?? EmptyEvents;
    }

    /// <summary>
    /// <see cref="NetworkSyncModel.FastReconnect"/> 能力的玩法无关实现（审计迁移步骤 7）。
    /// 该会话是 <see cref="NetworkSyncProfiles.FastReconnect"/> 档案背后的可复用运行时
    /// （Recovery = <see cref="RecoveryPolicy.ReconnectResume"/> | <see cref="RecoveryPolicy.RequestFullSnapshot"/>）：
    /// 它跟踪最后确认的权威帧，并在重连时根据帧 gap 相对可配置恢复窗口的大小，在增量恢复与完整快照重建之间决策。
    /// 每次迁移都会暴露统一的 <see cref="SyncHealthEvent"/>（<see cref="SyncHealthEventKind.SnapshotGap"/>、
    /// <see cref="SyncHealthEventKind.FullSnapshotRequested"/>、<see cref="SyncHealthEventKind.FullSnapshotApplied"/>、
    /// <see cref="SyncHealthEventKind.InterpolationRecovered"/>）以及 <see cref="SyncReconciliationReport"/>，
    /// 让示例完全通过框架驱动重连，而不是重新实现各自的恢复状态。
    /// </summary>
    public sealed class FastReconnectSession
    {
        private readonly int _resumeWindowFrames;

        private FastReconnectPhase _phase = FastReconnectPhase.Connected;
        private int _lastAckedServerFrame;
        private int _currentServerFrame;
        private int _pendingGapFrames;

        /// <param name="resumeWindowFrames">
        /// 可通过增量恢复处理的最大权威帧 gap。重连 gap 小于等于该窗口时从缓冲增量恢复；更大的 gap 会强制完整快照重建。
        /// </param>
        public FastReconnectSession(int resumeWindowFrames = 32)
        {
            if (resumeWindowFrames <= 0) throw new ArgumentOutOfRangeException(nameof(resumeWindowFrames));

            _resumeWindowFrames = resumeWindowFrames;
        }

        public FastReconnectPhase Phase => _phase;

        public int ResumeWindowFrames => _resumeWindowFrames;

        /// <summary>客户端已完整确认的最新权威帧。</summary>
        public int LastAckedServerFrame => _lastAckedServerFrame;

        /// <summary>最近一次重连时观察到的权威帧（连接中为 0）。</summary>
        public int CurrentServerFrame => _currentServerFrame;

        /// <summary>
        /// 当前进行中的恢复正在闭合的帧 gap。仅当 <see cref="Phase"/> 为 <see cref="FastReconnectPhase.Resuming"/>
        /// 或 <see cref="FastReconnectPhase.AwaitingFullSnapshot"/> 时有效。
        /// </summary>
        public int PendingGapFrames => _pendingGapFrames;

        /// <summary>
        /// 在连接状态下记录一次常规权威心跳，并确认 <paramref name="serverFrame"/>。
        /// 发出信息级 <see cref="SyncHealthEventKind.SnapshotReceived"/> 事件。
        /// </summary>
        public FastReconnectStepReport ObserveServerFrame(int serverFrame)
        {
            if (serverFrame < 0) throw new ArgumentOutOfRangeException(nameof(serverFrame));
            if (_phase != FastReconnectPhase.Connected && _phase != FastReconnectPhase.Recovered)
            {
                throw new InvalidOperationException($"Cannot observe a server frame while {_phase}; reconnect must complete first.");
            }
            if (serverFrame < _lastAckedServerFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(serverFrame), serverFrame, "Authoritative frame cannot move backwards.");
            }

            _phase = FastReconnectPhase.Connected;
            _lastAckedServerFrame = serverFrame;
            _currentServerFrame = serverFrame;
            _pendingGapFrames = 0;

            return new FastReconnectStepReport(
                _phase,
                SyncReconciliationReport.None,
                SyncHealthEvent.Info(SyncHealthEventKind.SnapshotReceived, serverFrame));
        }

        /// <summary>
        /// 标记连接已丢失。本地播放会保持饥饿状态，直到调用 <see cref="Reconnect"/>。
        /// 在最后确认帧发出 <see cref="SyncHealthEventKind.InterpolationStarved"/> 警告。
        /// </summary>
        public FastReconnectStepReport Disconnect()
        {
            if (_phase == FastReconnectPhase.Disconnected)
            {
                throw new InvalidOperationException("Session is already disconnected.");
            }

            _phase = FastReconnectPhase.Disconnected;

            var report = new SyncReconciliationReport(
                SyncReconciliationReason.SnapshotTimeout,
                SyncRecoveryState.Normal,
                needsFullSnapshot: false,
                clientFrame: _lastAckedServerFrame,
                authoritativeFrame: _lastAckedServerFrame,
                clientStateHash: 0u,
                authoritativeStateHash: 0u,
                replayTicks: 0);

            return new FastReconnectStepReport(
                _phase,
                report,
                SyncHealthEvent.Warning(SyncHealthEventKind.InterpolationStarved, _lastAckedServerFrame));
        }

        /// <summary>
        /// 在 <paramref name="currentServerFrame"/> 处重新建立连接并选择恢复路径。
        /// 始终发出携带 gap 的 <see cref="SyncHealthEventKind.SnapshotGap"/> 警告；恢复窗口内的 gap 进入
        /// <see cref="FastReconnectPhase.Resuming"/>，否则请求完整快照（<see cref="SyncHealthEventKind.FullSnapshotRequested"/>）
        /// 并进入 <see cref="FastReconnectPhase.AwaitingFullSnapshot"/>。调用 <see cref="CompleteRecovery"/> 完成恢复。
        /// </summary>
        public FastReconnectStepReport Reconnect(int currentServerFrame)
        {
            if (_phase != FastReconnectPhase.Disconnected)
            {
                throw new InvalidOperationException($"Cannot reconnect while {_phase}; the session must be disconnected first.");
            }
            if (currentServerFrame < _lastAckedServerFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(currentServerFrame), currentServerFrame, "Authoritative frame cannot move backwards.");
            }

            _currentServerFrame = currentServerFrame;
            var gap = currentServerFrame - _lastAckedServerFrame;
            _pendingGapFrames = gap;

            var gapEvent = SyncHealthEvent.Warning(SyncHealthEventKind.SnapshotGap, currentServerFrame, gap);

            if (gap <= _resumeWindowFrames)
            {
                _phase = FastReconnectPhase.Resuming;
                var resumeReport = new SyncReconciliationReport(
                    SyncReconciliationReason.FrameTooFarBehind,
                    SyncRecoveryState.CatchUp,
                    needsFullSnapshot: false,
                    clientFrame: _lastAckedServerFrame,
                    authoritativeFrame: currentServerFrame,
                    clientStateHash: 0u,
                    authoritativeStateHash: 0u,
                    replayTicks: gap);

                return new FastReconnectStepReport(_phase, resumeReport, gapEvent);
            }

            _phase = FastReconnectPhase.AwaitingFullSnapshot;
            var snapshotReport = new SyncReconciliationReport(
                SyncReconciliationReason.FrameTooFarBehind,
                SyncRecoveryState.AwaitingFullSnapshot,
                needsFullSnapshot: true,
                clientFrame: _lastAckedServerFrame,
                authoritativeFrame: currentServerFrame,
                clientStateHash: 0u,
                authoritativeStateHash: 0u,
                replayTicks: 0);

            return new FastReconnectStepReport(
                _phase,
                snapshotReport,
                gapEvent,
                SyncHealthEvent.Info(SyncHealthEventKind.FullSnapshotRequested, currentServerFrame, gap));
        }

        /// <summary>
        /// 完成进行中的恢复：增量恢复会将客户端追到权威帧，完整快照路径会先应用快照
        /// （<see cref="SyncHealthEventKind.FullSnapshotApplied"/>）。两条路径都会以
        /// <see cref="SyncHealthEventKind.InterpolationRecovered"/> 结束，并让会话进入已闭合 gap 的
        /// <see cref="FastReconnectPhase.Recovered"/>。
        /// </summary>
        public FastReconnectStepReport CompleteRecovery()
        {
            if (_phase != FastReconnectPhase.Resuming && _phase != FastReconnectPhase.AwaitingFullSnapshot)
            {
                throw new InvalidOperationException($"Cannot complete recovery while {_phase}; reconnect must be in progress.");
            }

            var gap = _pendingGapFrames;
            var recoveredFrame = _currentServerFrame;
            var wasFullSnapshot = _phase == FastReconnectPhase.AwaitingFullSnapshot;

            _lastAckedServerFrame = recoveredFrame;
            _pendingGapFrames = 0;
            _phase = FastReconnectPhase.Recovered;

            var report = new SyncReconciliationReport(
                SyncReconciliationReason.FrameTooFarBehind,
                SyncRecoveryState.Recovered,
                needsFullSnapshot: false,
                clientFrame: recoveredFrame,
                authoritativeFrame: recoveredFrame,
                clientStateHash: 0u,
                authoritativeStateHash: 0u,
                replayTicks: wasFullSnapshot ? 0 : gap);

            var recovered = SyncHealthEvent.Info(SyncHealthEventKind.InterpolationRecovered, recoveredFrame, gap);

            return wasFullSnapshot
                ? new FastReconnectStepReport(
                    _phase,
                    report,
                    SyncHealthEvent.Info(SyncHealthEventKind.FullSnapshotApplied, recoveredFrame, gap),
                    recovered)
                : new FastReconnectStepReport(_phase, report, recovered);
        }
    }
}

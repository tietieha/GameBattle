#nullable enable

using System;
using System.Collections.Generic;
using AbilityKit.Demo.Shooter.Runtime;
using AbilityKit.Network.Runtime.Conditioning;
using AbilityKit.Network.Runtime.Sync;
using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.View.Hosting
{
    /// <summary>
    /// Shooter 演示外壳使用的宿主无关诊断快照。Editor 窗口、PlayMode host 与 attach 模式观察器
    /// 都可以投影同一份运行时/会话状态，而不需要各自持有采集逻辑。
    /// </summary>
    public readonly struct ShooterHostDiagnosticsSnapshot
    {
        public ShooterHostDiagnosticsSnapshot(
            int frame,
            int playerCount,
            int bulletCount,
            int enemyCount,
            IReadOnlyList<ShooterEventSnapshot> recentEvents,
            int totalEvents,
            double maxDivergence,
            IReadOnlyList<ShooterWorldDivergence> divergences,
            NetworkConditioningStats? carrierNetworkStats,
            ShooterSnapshotApplyResult? lastCarrierSnapshotApplyResult,
            SyncTimeAnchor lastCarrierTimeAnchor,
            SyncTimeAnchor localTimeAnchor,
            ShooterLagCompensationTelemetry? lagCompensationTelemetry,
            ShooterLagCompensationEvaluation? lagCompensationEvaluation,
            ShooterRemoteLatencyCompensationDiagnostics remoteLatencyCompensationDiagnostics,
            ShooterCrossLayerDiagnostics crossLayerDiagnostics,
            ShooterPureStateSyncDiagnostics pureStateSyncDiagnostics,
            bool needsPureStateBaselineResync,
            ShooterPureStateResyncReason lastPureStateResyncReason,
            int lastPureStateAppliedFrame,
            uint lastPureStateAppliedStateHash,
            int lastPureStateResyncFrame,
            uint lastPureStateResyncStateHash)
        {
            Frame = frame;
            PlayerCount = playerCount;
            BulletCount = bulletCount;
            EnemyCount = enemyCount;
            RecentEvents = recentEvents ?? Array.Empty<ShooterEventSnapshot>();
            TotalEvents = totalEvents;
            MaxDivergence = maxDivergence;
            Divergences = divergences ?? Array.Empty<ShooterWorldDivergence>();
            CarrierNetworkStats = carrierNetworkStats;
            LastCarrierSnapshotApplyResult = lastCarrierSnapshotApplyResult;
            LastCarrierTimeAnchor = lastCarrierTimeAnchor;
            LocalTimeAnchor = localTimeAnchor;
            LagCompensationTelemetry = lagCompensationTelemetry;
            LagCompensationEvaluation = lagCompensationEvaluation;
            RemoteLatencyCompensationDiagnostics = remoteLatencyCompensationDiagnostics;
            CrossLayerDiagnostics = crossLayerDiagnostics;
            PureStateSyncDiagnostics = pureStateSyncDiagnostics;
            NeedsPureStateBaselineResync = needsPureStateBaselineResync;
            LastPureStateResyncReason = lastPureStateResyncReason;
            LastPureStateAppliedFrame = lastPureStateAppliedFrame;
            LastPureStateAppliedStateHash = lastPureStateAppliedStateHash;
            LastPureStateResyncFrame = lastPureStateResyncFrame;
            LastPureStateResyncStateHash = lastPureStateResyncStateHash;
        }

        public int Frame { get; }

        public int PlayerCount { get; }

        public int BulletCount { get; }

        public int EnemyCount { get; }

        public IReadOnlyList<ShooterEventSnapshot> RecentEvents { get; }

        public int TotalEvents { get; }

        public double MaxDivergence { get; }

        public IReadOnlyList<ShooterWorldDivergence> Divergences { get; }

        public NetworkConditioningStats? CarrierNetworkStats { get; }

        public ShooterSnapshotApplyResult? LastCarrierSnapshotApplyResult { get; }

        public SyncTimeAnchor LastCarrierTimeAnchor { get; }

        public SyncTimeAnchor LocalTimeAnchor { get; }

        public ShooterLagCompensationTelemetry? LagCompensationTelemetry { get; }

        public ShooterLagCompensationEvaluation? LagCompensationEvaluation { get; }

        public ShooterRemoteLatencyCompensationDiagnostics RemoteLatencyCompensationDiagnostics { get; }
 
        public ShooterCrossLayerDiagnostics CrossLayerDiagnostics { get; }
 
        public ShooterPureStateSyncDiagnostics PureStateSyncDiagnostics { get; }

        public bool NeedsPureStateBaselineResync { get; }

        public ShooterPureStateResyncReason LastPureStateResyncReason { get; }

        public int LastPureStateAppliedFrame { get; }

        public uint LastPureStateAppliedStateHash { get; }

        public int LastPureStateResyncFrame { get; }

        public uint LastPureStateResyncStateHash { get; }
    }

    public static class ShooterHostDiagnosticsProjector
    {
        public static ShooterHostDiagnosticsSnapshot ProjectFromSession(
            ShooterAcceptanceSession session,
            in ShooterStateSnapshotPayload snapshot,
            int previousTotalEvents)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var recentEvents = snapshot.Events ?? Array.Empty<ShooterEventSnapshot>();
            var comparison = session.HasAuthoritativeWorld
                ? session.CompareWorlds()
                : new ShooterWorldComparison(snapshot.Frame, 0, Array.Empty<ShooterWorldDivergence>());

            return new ShooterHostDiagnosticsSnapshot(
                snapshot.Frame,
                snapshot.Players?.Length ?? 0,
                snapshot.Bullets?.Length ?? 0,
                CountEntities(session.Presentation.ViewModel.Current.EntityChanges, ShooterViewEntityKind.Enemy),
                recentEvents,
                previousTotalEvents + recentEvents.Length,
                comparison.MaxDistance,
                comparison.Divergences,
                session.CarrierNetworkStats,
                session.LastCarrierSnapshotApplyResult,
                session.LastCarrierTimeAnchor,
                default,
                session.LagCompensationTelemetry,
                session.LastLagCompensationEvaluation,
                default,
                ShooterCrossLayerDiagnostics.From(
                    session.FrameworkSnapshotPipelineDiagnostics,
                    session.LastCarrierSnapshotApplyResult,
                    default,
                    session.Presentation.NeedsPureStateFullBaselineResync,
                    session.Presentation.LastPureStateAppliedFrame,
                    session.Presentation.LastPureStateResyncFrame),
                session.Presentation.LastPureStateSyncDiagnostics,
                session.Presentation.NeedsPureStateFullBaselineResync,
                session.Presentation.LastPureStateResyncReason,
                session.Presentation.LastPureStateAppliedFrame,
                session.Presentation.LastPureStateAppliedStateHash,
                session.Presentation.LastPureStateResyncFrame,
                session.Presentation.LastPureStateResyncStateHash);
        }

        public static ShooterHostDiagnosticsSnapshot ProjectFromFrame(
            in ShooterHostPresentationFrame frame,
            int previousTotalEvents)
        {
            var batch = frame.ClientBatch;
            var recentEvents = batch.Events ?? Array.Empty<ShooterEventSnapshot>();

            return new ShooterHostDiagnosticsSnapshot(
                batch.Frame,
                CountEntities(batch.EntityChanges, ShooterViewEntityKind.Player),
                CountEntities(batch.EntityChanges, ShooterViewEntityKind.Bullet),
                CountEntities(batch.EntityChanges, ShooterViewEntityKind.Enemy),
                recentEvents,
                previousTotalEvents + recentEvents.Count,
                0d,
                Array.Empty<ShooterWorldDivergence>(),
                frame.CarrierNetworkStats,
                frame.LastCarrierSnapshotApplyResult,
                frame.LastCarrierTimeAnchor,
                frame.LocalTimeAnchor,
                frame.LagCompensationTelemetry,
                frame.LagCompensationEvaluation,
                frame.RemoteLatencyCompensationDiagnostics,
                frame.CrossLayerDiagnostics,
                frame.PureStateSyncDiagnostics,
                frame.NeedsPureStateBaselineResync,
                frame.LastPureStateResyncReason,
                frame.LastPureStateAppliedFrame,
                frame.LastPureStateAppliedStateHash,
                frame.LastPureStateResyncFrame,
                frame.LastPureStateResyncStateHash);
        }

        private static int CountEntities(IReadOnlyList<ShooterViewEntityChange> changes, ShooterViewEntityKind kind)
        {
            var count = 0;
            for (var i = 0; i < changes.Count; i++)
            {
                if (changes[i].Kind == kind && changes[i].Alive)
                {
                    count++;
                }
            }

            return count;
        }
    }
}

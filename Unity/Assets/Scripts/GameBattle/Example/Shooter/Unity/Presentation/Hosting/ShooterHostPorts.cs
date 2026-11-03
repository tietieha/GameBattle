#nullable enable

using AbilityKit.Demo.Shooter.Runtime;
using AbilityKit.Protocol.Shooter;
using AbilityKit.Network.Runtime.Conditioning;
using AbilityKit.Network.Runtime.Sync;

namespace AbilityKit.Demo.Shooter.View.Hosting
{
    public readonly struct ShooterHostFrameInput
    {
        public ShooterHostFrameInput(float moveX, float moveY, float aimX, float aimY, bool fire)
            : this(moveX, moveY, aimX, aimY, fire, ShooterPlayerAttackSlots.Primary)
        {
        }

        public ShooterHostFrameInput(float moveX, float moveY, float aimX, float aimY, bool fire, int attackSlot)
        {
            MoveX = moveX;
            MoveY = moveY;
            AimX = aimX;
            AimY = aimY;
            Fire = fire;
            AttackSlot = ShooterPlayerAttackSlots.Normalize(attackSlot);
        }

        public float MoveX { get; }
        public float MoveY { get; }
        public float AimX { get; }
        public float AimY { get; }
        public bool Fire { get; }
        public int AttackSlot { get; }
    }

    public readonly struct ShooterHostPresentationFrame
    {
        public ShooterHostPresentationFrame(
            ShooterSnapshotViewBatch clientBatch,
            ShooterSnapshotViewBatch authorityBatch,
            bool hasAuthorityBatch,
            int controlledPlayerId,
            float worldScale,
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
            ClientBatch = clientBatch;
            AuthorityBatch = authorityBatch;
            HasAuthorityBatch = hasAuthorityBatch;
            ControlledPlayerId = controlledPlayerId;
            WorldScale = worldScale;
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

        public ShooterSnapshotViewBatch ClientBatch { get; }
        public ShooterSnapshotViewBatch AuthorityBatch { get; }
        public bool HasAuthorityBatch { get; }
        public int ControlledPlayerId { get; }
        public float WorldScale { get; }
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

    public interface IShooterHostInputSource
    {
        ShooterHostFrameInput ReadInput(int controlledPlayerId);
    }

    public interface IShooterHostViewSink
    {
        void Render(in ShooterHostPresentationFrame frame);
        void Clear();
    }
}

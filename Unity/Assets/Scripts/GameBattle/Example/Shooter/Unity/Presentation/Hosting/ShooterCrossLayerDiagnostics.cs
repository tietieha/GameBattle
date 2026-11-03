#nullable enable

using AbilityKit.Demo.Shooter.Runtime;
using AbilityKit.Demo.Shooter.View;

namespace AbilityKit.Demo.Shooter.View.Hosting
{
    public readonly struct ShooterCrossLayerDiagnostics
    {
        public ShooterCrossLayerDiagnostics(
            int frameworkPacketCount,
            int frameworkDispatchedSnapshotCount,
            int frameworkPackedSnapshotCount,
            int frameworkPureStateSnapshotCount,
            int lastFrameworkFrame,
            int lastFrameworkPayloadOpCode,
            string lastFrameworkWorldId,
            bool hasSnapshotApplyResult,
            ShooterSnapshotApplyResult snapshotApplyResult,
            bool hasRemoteLatencyResult,
            int remoteInputDelayFrames,
            int remoteAuthoritativeFrameGap,
            bool needsPureStateBaselineResync,
            int lastPureStateAppliedFrame,
            int lastPureStateResyncFrame)
        {
            FrameworkPacketCount = frameworkPacketCount;
            FrameworkDispatchedSnapshotCount = frameworkDispatchedSnapshotCount;
            FrameworkPackedSnapshotCount = frameworkPackedSnapshotCount;
            FrameworkPureStateSnapshotCount = frameworkPureStateSnapshotCount;
            LastFrameworkFrame = lastFrameworkFrame;
            LastFrameworkPayloadOpCode = lastFrameworkPayloadOpCode;
            LastFrameworkWorldId = lastFrameworkWorldId ?? string.Empty;
            HasSnapshotApplyResult = hasSnapshotApplyResult;
            SnapshotApplyResult = snapshotApplyResult;
            HasRemoteLatencyResult = hasRemoteLatencyResult;
            RemoteInputDelayFrames = remoteInputDelayFrames;
            RemoteAuthoritativeFrameGap = remoteAuthoritativeFrameGap;
            NeedsPureStateBaselineResync = needsPureStateBaselineResync;
            LastPureStateAppliedFrame = lastPureStateAppliedFrame;
            LastPureStateResyncFrame = lastPureStateResyncFrame;
        }

        public int FrameworkPacketCount { get; }
        public int FrameworkDispatchedSnapshotCount { get; }
        public int FrameworkPackedSnapshotCount { get; }
        public int FrameworkPureStateSnapshotCount { get; }
        public int LastFrameworkFrame { get; }
        public int LastFrameworkPayloadOpCode { get; }
        public string LastFrameworkWorldId { get; }
        public bool HasSnapshotApplyResult { get; }
        public ShooterSnapshotApplyResult SnapshotApplyResult { get; }
        public bool HasRemoteLatencyResult { get; }
        public int RemoteInputDelayFrames { get; }
        public int RemoteAuthoritativeFrameGap { get; }
        public bool NeedsPureStateBaselineResync { get; }
        public int LastPureStateAppliedFrame { get; }
        public int LastPureStateResyncFrame { get; }
        public bool HasFrameworkSnapshot => FrameworkPacketCount > 0 || FrameworkDispatchedSnapshotCount > 0;

        public static ShooterCrossLayerDiagnostics From(
            in ShooterFrameworkSnapshotPipelineDiagnostics framework,
            ShooterSnapshotApplyResult? snapshotApplyResult,
            in ShooterRemoteLatencyCompensationDiagnostics remoteLatency,
            bool needsPureStateBaselineResync,
            int lastPureStateAppliedFrame,
            int lastPureStateResyncFrame)
        {
            return new ShooterCrossLayerDiagnostics(
                framework.PacketCount,
                framework.DispatchedSnapshotCount,
                framework.PackedSnapshotCount,
                framework.PureStateSnapshotCount,
                framework.LastFrame,
                framework.LastPayloadOpCode,
                framework.LastWorldId,
                snapshotApplyResult.HasValue,
                snapshotApplyResult.GetValueOrDefault(),
                remoteLatency.HasResult,
                remoteLatency.InputDelayFrames,
                remoteLatency.AuthoritativeFrameGap,
                needsPureStateBaselineResync,
                lastPureStateAppliedFrame,
                lastPureStateResyncFrame);
        }
    }
}

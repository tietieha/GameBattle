using System;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.FrameSync.Rollback;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public static class ShooterStateRecoveryScenarios
    {
        public const string Rollback = "rollback";
        public const string Replay = "replay";
        public const string Rewind = "rewind";
    }

    public enum ShooterStateRecoveryOperation
    {
        Capture = 1,
        Restore = 2
    }

    public readonly struct ShooterStateRecoveryResult
    {
        public ShooterStateRecoveryResult(
            ShooterStateRecoveryOperation operation,
            string scenario,
            int providerKey,
            int frame,
            bool success,
            int payloadBytes,
            uint stateHashBefore,
            uint stateHashAfter)
        {
            Operation = operation;
            Scenario = scenario ?? string.Empty;
            ProviderKey = providerKey;
            Frame = frame;
            Success = success;
            PayloadBytes = payloadBytes;
            StateHashBefore = stateHashBefore;
            StateHashAfter = stateHashAfter;
        }

        public ShooterStateRecoveryOperation Operation { get; }

        public string Scenario { get; }

        public int ProviderKey { get; }

        public int Frame { get; }

        public bool Success { get; }

        public int PayloadBytes { get; }

        public uint StateHashBefore { get; }

        public uint StateHashAfter { get; }

        public bool StateHashChanged => StateHashBefore != StateHashAfter;
    }

    public readonly struct ShooterStateRecoveryCapture
    {
        public ShooterStateRecoveryCapture(byte[] payload, ShooterStateRecoveryResult diagnostics)
        {
            Payload = payload ?? Array.Empty<byte>();
            Diagnostics = diagnostics;
        }

        public byte[] Payload { get; }

        public ShooterStateRecoveryResult Diagnostics { get; }
    }

    public sealed class ShooterStateRecoveryExample
    {
        private readonly IRollbackStateProvider _provider;
        private readonly IShooterStateHashProvider _hashProvider;

        public ShooterStateRecoveryExample(IShooterBattleRuntimePort runtime, ulong worldId)
            : this(new ShooterPackedSnapshotRollbackProvider(runtime, worldId), runtime)
        {
        }

        public ShooterStateRecoveryExample(IRollbackStateProvider provider, IShooterStateHashProvider hashProvider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _hashProvider = hashProvider ?? throw new ArgumentNullException(nameof(hashProvider));
        }

        public int ProviderKey => _provider.Key;

        public ShooterStateRecoveryResult LastDiagnostics { get; private set; }

        public ShooterStateRecoveryCapture CaptureRollbackCheckpoint(int frame)
        {
            return Capture(new FrameIndex(frame), ShooterStateRecoveryScenarios.Rollback);
        }

        public ShooterStateRecoveryCapture CaptureReplayCheckpoint(int frame)
        {
            return Capture(new FrameIndex(frame), ShooterStateRecoveryScenarios.Replay);
        }

        public ShooterStateRecoveryCapture CaptureRewindCheckpoint(int frame)
        {
            return Capture(new FrameIndex(frame), ShooterStateRecoveryScenarios.Rewind);
        }

        public ShooterStateRecoveryCapture Capture(FrameIndex frame, string scenario)
        {
            var before = _hashProvider.ComputeStateHash();
            var payload = _provider.Export(frame) ?? Array.Empty<byte>();
            var after = _hashProvider.ComputeStateHash();
            var result = new ShooterStateRecoveryResult(
                ShooterStateRecoveryOperation.Capture,
                scenario,
                _provider.Key,
                frame.Value,
                payload.Length > 0,
                payload.Length,
                before,
                after);
            LastDiagnostics = result;
            return new ShooterStateRecoveryCapture(payload, result);
        }

        public ShooterStateRecoveryResult RestoreRollbackCheckpoint(int frame, byte[] payload)
        {
            return Restore(new FrameIndex(frame), payload, ShooterStateRecoveryScenarios.Rollback);
        }

        public ShooterStateRecoveryResult RestoreReplayCheckpoint(int frame, byte[] payload)
        {
            return Restore(new FrameIndex(frame), payload, ShooterStateRecoveryScenarios.Replay);
        }

        public ShooterStateRecoveryResult RestoreRewindCheckpoint(int frame, byte[] payload)
        {
            return Restore(new FrameIndex(frame), payload, ShooterStateRecoveryScenarios.Rewind);
        }

        public ShooterStateRecoveryResult Restore(FrameIndex frame, byte[] payload, string scenario)
        {
            var before = _hashProvider.ComputeStateHash();
            var success = payload != null && payload.Length > 0;
            if (success)
            {
                _provider.Import(frame, payload);
            }

            var after = _hashProvider.ComputeStateHash();
            var result = new ShooterStateRecoveryResult(
                ShooterStateRecoveryOperation.Restore,
                scenario,
                _provider.Key,
                frame.Value,
                success,
                payload == null ? 0 : payload.Length,
                before,
                after);
            LastDiagnostics = result;
            return result;
        }
    }
}

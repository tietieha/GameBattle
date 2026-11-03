using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public readonly struct ShooterDeterminismSpec
    {
        public ShooterDeterminismSpec(ShooterStartGamePayload start, ShooterDeterminismFrameInput[] frames, float fixedDeltaSeconds)
        {
            Start = start;
            Frames = frames ?? System.Array.Empty<ShooterDeterminismFrameInput>();
            FixedDeltaSeconds = fixedDeltaSeconds > 0f ? fixedDeltaSeconds : 1f / 30f;
        }

        public ShooterStartGamePayload Start { get; }

        public ShooterDeterminismFrameInput[] Frames { get; }

        public float FixedDeltaSeconds { get; }
    }

    public readonly struct ShooterDeterminismFrameInput
    {
        public ShooterDeterminismFrameInput(int frame, ShooterPlayerCommand[] commands)
        {
            Frame = frame;
            Commands = commands ?? System.Array.Empty<ShooterPlayerCommand>();
        }

        public int Frame { get; }

        public ShooterPlayerCommand[] Commands { get; }
    }

    public readonly struct ShooterDeterminismResult
    {
        public ShooterDeterminismResult(
            int frame,
            uint stateHash,
            uint roundTripStateHash,
            bool roundTripMatched,
            ShooterStateSnapshotPayload snapshot,
            ShooterPackedSnapshotPayload packedSnapshot)
        {
            Frame = frame;
            StateHash = stateHash;
            RoundTripStateHash = roundTripStateHash;
            RoundTripMatched = roundTripMatched;
            Snapshot = snapshot;
            PackedSnapshot = packedSnapshot;
        }

        public int Frame { get; }

        public uint StateHash { get; }

        public uint RoundTripStateHash { get; }

        public bool RoundTripMatched { get; }

        public ShooterStateSnapshotPayload Snapshot { get; }

        public ShooterPackedSnapshotPayload PackedSnapshot { get; }
    }

    public sealed class ShooterDeterminismSpecRunner
    {
        public const ulong DefaultSnapshotWorldId = 1ul;

        public ShooterDeterminismResult Run(in ShooterDeterminismSpec spec)
        {
            return Run(in spec, DefaultSnapshotWorldId);
        }

        public ShooterDeterminismResult Run(in ShooterDeterminismSpec spec, ulong snapshotWorldId)
        {
            var runtime = new ShooterBattleRuntimePort();
            var start = spec.Start;
            if (!runtime.StartGame(in start))
            {
                throw new System.InvalidOperationException("Shooter determinism spec failed to start.");
            }

            var frames = spec.Frames;
            for (var i = 0; i < frames.Length; i++)
            {
                var frameInput = frames[i];
                runtime.SubmitInput(frameInput.Frame, frameInput.Commands);
                if (!runtime.Tick(spec.FixedDeltaSeconds))
                {
                    throw new System.InvalidOperationException($"Shooter determinism spec failed to tick frame {frameInput.Frame}.");
                }
            }

            var stateHash = runtime.ComputeStateHash();
            var snapshot = runtime.GetSnapshot();
            var packed = runtime.ExportPackedSnapshot(snapshotWorldId, isFullSnapshot: true, authorityOverride: true);

            var roundTripRuntime = new ShooterBattleRuntimePort();
            roundTripRuntime.ImportPackedSnapshot(in packed);
            var roundTripHash = roundTripRuntime.ComputeStateHash();

            return new ShooterDeterminismResult(
                runtime.CurrentFrame,
                stateHash,
                roundTripHash,
                stateHash == roundTripHash,
                snapshot,
                packed);
        }
    }
}

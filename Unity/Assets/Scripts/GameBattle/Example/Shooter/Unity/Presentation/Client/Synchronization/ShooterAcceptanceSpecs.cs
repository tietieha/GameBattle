#nullable enable

using System;
using System.Collections.Generic;
using AbilityKit.Demo.Shooter.Runtime;
using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.View
{
    /// <summary>
    /// Shooter 纯 C# 验收规格中的一帧输入。Unity 外壳和无头测试都应使用同一份规格，
    /// 避免“测试脚本”和“可视化脚本”产生分叉。
    /// </summary>
    public readonly struct ShooterAcceptanceSpecFrame
    {
        public ShooterAcceptanceSpecFrame(int frame, IReadOnlyList<ShooterPlayerCommand> commands)
        {
            if (frame < 0) throw new ArgumentOutOfRangeException(nameof(frame));

            Frame = frame;
            Commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public int Frame { get; }

        public IReadOnlyList<ShooterPlayerCommand> Commands { get; }
    }

    /// <summary>
    /// 可重复执行的 Shooter 验收规格。它描述初始玩家、固定输入脚本和总帧数，
    /// 是 AI 先行实现、无头测试验证、Unity 后续可视化三者共享的行为契约。
    /// </summary>
    public sealed class ShooterAcceptanceSpec
    {
        public ShooterAcceptanceSpec(
            string id,
            ShooterStartGamePayload start,
            IReadOnlyList<ShooterAcceptanceSpecFrame> frames,
            int totalFrames,
            float deltaSeconds)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Spec id is required.", nameof(id));
            if (totalFrames <= 0) throw new ArgumentOutOfRangeException(nameof(totalFrames));
            if (deltaSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));

            Id = id;
            Start = start;
            Frames = frames ?? throw new ArgumentNullException(nameof(frames));
            TotalFrames = totalFrames;
            DeltaSeconds = deltaSeconds;
        }

        public string Id { get; }

        public ShooterStartGamePayload Start { get; }

        public IReadOnlyList<ShooterAcceptanceSpecFrame> Frames { get; }

        public int TotalFrames { get; }

        public float DeltaSeconds { get; }
    }

    /// <summary>
    /// Shooter 规格执行结果。测试可锁定这些值；Unity 外壳也可以读取最终快照和事件进行可视化。
    /// </summary>
    public readonly struct ShooterAcceptanceSpecResult
    {
        public ShooterAcceptanceSpecResult(
            string specId,
            int frame,
            uint stateHash,
            ShooterStateSnapshotPayload snapshot,
            ShooterPackedSnapshotPayload packedSnapshot,
            IReadOnlyList<ShooterEventSnapshot> events)
        {
            SpecId = specId;
            Frame = frame;
            StateHash = stateHash;
            Snapshot = snapshot;
            PackedSnapshot = packedSnapshot;
            Events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public string SpecId { get; }

        public int Frame { get; }

        public uint StateHash { get; }

        public ShooterStateSnapshotPayload Snapshot { get; }

        public ShooterPackedSnapshotPayload PackedSnapshot { get; }

        public IReadOnlyList<ShooterEventSnapshot> Events { get; }
    }

    /// <summary>
    /// Shooter 规格目录。这里的场景应保持少而稳定，作为“代码实现必须满足”的产品级回归基线。
    /// </summary>
    public static class ShooterAcceptanceSpecs
    {
        public static ShooterAcceptanceSpec BasicCombat { get; } = new ShooterAcceptanceSpec(
            "basic-combat",
            new ShooterStartGamePayload(
                "spec-basic-combat",
                30,
                1401,
                new[]
                {
                    new ShooterStartPlayer(1, "P1", 0f, 0f),
                    new ShooterStartPlayer(2, "P2", 0.6f, 0f)
                }),
            new[]
            {
                new ShooterAcceptanceSpecFrame(0, new[] { new ShooterPlayerCommand(1, 0f, 0f, 1f, 0f, true) }),
                new ShooterAcceptanceSpecFrame(1, new[] { new ShooterPlayerCommand(1, 1f, 0f, 1f, 0f, false) }),
                new ShooterAcceptanceSpecFrame(2, new[] { new ShooterPlayerCommand(2, -1f, 0f, -1f, 0f, false) })
            },
            totalFrames: 6,
            deltaSeconds: 1f / 30f);
    }

    /// <summary>
    /// 执行 Shooter 纯 C# 验收规格。该 Runner 不依赖 Unity 对象，适合 CI、AI 自动实现后的回归，
    /// 也适合 Unity 外壳在需要时直接读取同一份规格进行可视化。
    /// </summary>
    public sealed class ShooterAcceptanceSpecRunner
    {
        private const ulong SnapshotWorldId = 0x51007ul;

        public ShooterAcceptanceSpecResult Run(ShooterAcceptanceSpec spec)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            var runtime = new ShooterBattleRuntimePort();
            var start = spec.Start;
            if (!runtime.StartGame(in start))
            {
                throw new InvalidOperationException($"Shooter acceptance spec '{spec.Id}' failed to start.");
            }

            var events = new List<ShooterEventSnapshot>();
            for (var frame = 0; frame < spec.TotalFrames; frame++)
            {
                SubmitFrame(runtime, spec.Frames, frame);
                if (!runtime.Tick(spec.DeltaSeconds))
                {
                    throw new InvalidOperationException($"Shooter acceptance spec '{spec.Id}' failed to tick frame {frame}.");
                }

                var frameSnapshot = runtime.GetSnapshot();
                if (frameSnapshot.Events.Length > 0)
                {
                    events.AddRange(frameSnapshot.Events);
                }
            }

            var snapshot = runtime.GetSnapshot();
            var packed = runtime.ExportPackedSnapshot(SnapshotWorldId, isFullSnapshot: true, authorityOverride: true);
            return new ShooterAcceptanceSpecResult(
                spec.Id,
                runtime.CurrentFrame,
                runtime.ComputeStateHash(),
                snapshot,
                packed,
                events.ToArray());
        }

        private static void SubmitFrame(ShooterBattleRuntimePort runtime, IReadOnlyList<ShooterAcceptanceSpecFrame> frames, int frame)
        {
            for (var i = 0; i < frames.Count; i++)
            {
                var specFrame = frames[i];
                if (specFrame.Frame != frame)
                {
                    continue;
                }

                var commands = CopyCommands(specFrame.Commands);
                runtime.SubmitInput(frame, commands);
            }
        }

        private static ShooterPlayerCommand[] CopyCommands(IReadOnlyList<ShooterPlayerCommand> commands)
        {
            var copy = new ShooterPlayerCommand[commands.Count];
            for (var i = 0; i < commands.Count; i++)
            {
                copy[i] = commands[i];
            }

            return copy;
        }
    }
}

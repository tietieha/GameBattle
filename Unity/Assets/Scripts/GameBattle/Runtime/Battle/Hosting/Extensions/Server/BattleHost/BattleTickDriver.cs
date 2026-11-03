using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.Host.Extensions.Server.BattleHost
{
    public delegate int BattleInputSubmitter<in TInput>(int frame, IReadOnlyList<TInput> inputs);

    public delegate bool BattleWorldTicker(int frame, int tickRate, float deltaTime);

    public readonly struct BattleTickResult
    {
        public readonly int Frame;
        public readonly int InputCount;
        public readonly int CommandCount;
        public readonly bool WorldTicked;
        public readonly bool InputSubmitted;

        public BattleTickResult(
            int frame,
            int inputCount,
            int commandCount,
            bool worldTicked,
            bool inputSubmitted)
        {
            Frame = frame;
            InputCount = inputCount;
            CommandCount = commandCount;
            WorldTicked = worldTicked;
            InputSubmitted = inputSubmitted;
        }
    }

    public interface IBattleTickDriver<TInput>
    {
        BattleTickResult Tick(BattleHostState state, IBattleInputBuffer<TInput> inputBuffer);
    }

    public sealed class BattleTickDriver<TInput> : IBattleTickDriver<TInput>
    {
        private readonly BattleInputSubmitter<TInput> _submitter;
        private readonly BattleWorldTicker _ticker;

        public BattleTickDriver(BattleInputSubmitter<TInput> submitter, BattleWorldTicker ticker)
        {
            _submitter = submitter ?? throw new ArgumentNullException(nameof(submitter));
            _ticker = ticker ?? throw new ArgumentNullException(nameof(ticker));
        }

        public BattleTickResult Tick(BattleHostState state, IBattleInputBuffer<TInput> inputBuffer)
        {
            if (state == null || !state.Initialized)
            {
                return new BattleTickResult(0, 0, 0, false, false);
            }

            var frame = state.Frame;
            var drained = inputBuffer != null
                ? inputBuffer.Drain(frame)
                : new BattleInputDrainResult<TInput>(frame, Array.Empty<TInput>());
            var inputs = drained.Inputs ?? Array.Empty<TInput>();
            var commandCount = inputs.Count > 0 ? _submitter(frame, inputs) : 0;
            var deltaTime = 1.0f / state.TickRate;
            var worldTicked = _ticker(frame, state.TickRate, deltaTime);

            state.AdvanceFrame();
            return new BattleTickResult(frame, inputs.Count, commandCount, worldTicked, inputs.Count == 0 || commandCount > 0);
        }
    }
}

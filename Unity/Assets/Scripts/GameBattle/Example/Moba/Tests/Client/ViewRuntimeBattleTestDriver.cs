using System;
using System.Collections.Generic;
using AbilityKit.Demo.Moba.Testing;
using AbilityKit.Demo.Moba.View.Abstractions.Battle.View;

namespace AbilityKit.Game.Flow
{
    /// <summary>
    /// Headless driver that adapts shared battle test scripts to the moba.view runtime battle context.
    /// It is intended for EditMode tests, CI smoke tests, and future view-runtime regression scenarios.
    /// </summary>
    public sealed class ViewRuntimeBattleTestDriver : IBattleTestScriptDriver, IBattleTestScriptDriverLifecycle
    {
        public const int DefaultTickRate = 30;

        private readonly BattleContext _ctx;
        private readonly BattleViewInterpolationClock _clock = new();
        private readonly int _tickRate;
        private readonly float _backTimeTicks;
        private readonly float _maxLagTicks;
        private readonly List<int> _releasedSkillSlots = new();

        public ViewRuntimeBattleTestDriver(
            BattleContext ctx,
            int tickRate = DefaultTickRate,
            float backTimeTicks = 1f,
            float maxLagTicks = 2f)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            if (tickRate <= 0) throw new ArgumentOutOfRangeException(nameof(tickRate), tickRate, "Tick rate must be positive.");

            _tickRate = tickRate;
            _backTimeTicks = backTimeTicks;
            _maxLagTicks = maxLagTicks;
        }

        public int TickRate => _tickRate;
        public int TickCount { get; private set; }
        public int MoveApplyCount { get; private set; }
        public int SkillApplyCount { get; private set; }
        public int IdleApplyCount { get; private set; }
        public int WaitApplyCount { get; private set; }
        public int LastReleasedSkillSlot { get; private set; }
        public IReadOnlyList<int> ReleasedSkillSlots => _releasedSkillSlots;
        public double SampleTime { get; private set; }
        public BattleTestScriptRunResult LastResult { get; private set; }

        public void BeginScript(BattleTestScript script)
        {
            ResetRuntimeState();

            _clock.Reset();
            _ctx.LastFrame = 0;
            _ctx.LogicTimeSeconds = 0d;
            _ctx.SetHudMove(0f, 0f);
            _ctx.EndHudMove();
        }

        public void Apply(BattleTestStep step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));

            switch (step.Kind)
            {
                case BattleTestStepKind.Move:
                    MoveApplyCount++;
                    _ctx.BeginHudMove();
                    _ctx.SetHudMove(step.Dx, step.Dz);
                    break;
                case BattleTestStepKind.Skill:
                    SkillApplyCount++;
                    LastReleasedSkillSlot = step.Slot;
                    _releasedSkillSlots.Add(step.Slot);
                    _ctx.SubmitHudSkillClick(step.Slot);
                    break;
                case BattleTestStepKind.Wait:
                    WaitApplyCount++;
                    break;
                case BattleTestStepKind.Idle:
                    IdleApplyCount++;
                    _ctx.SetHudMove(0f, 0f);
                    _ctx.EndHudMove();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(step), step.Kind, "Unsupported battle test step kind.");
            }
        }

        public void Tick()
        {
            _ctx.LastFrame++;
            _ctx.LogicTimeSeconds = _ctx.LastFrame / (double)_tickRate;

            _clock.Advance(
                _ctx,
                deltaTime: 1f / _tickRate,
                backTimeTicks: _backTimeTicks,
                maxLagTicks: _maxLagTicks,
                out _);

            SampleTime = BattleViewSampleTimeResolver.Resolve(_ctx.LastFrame, _tickRate);
            TickCount++;
        }

        public void EndScript(BattleTestScript script, BattleTestScriptRunResult result)
        {
            LastResult = result;
            _ctx.SetHudMove(0f, 0f);
            _ctx.EndHudMove();
        }

        private void ResetRuntimeState()
        {
            TickCount = 0;
            MoveApplyCount = 0;
            SkillApplyCount = 0;
            IdleApplyCount = 0;
            WaitApplyCount = 0;
            LastReleasedSkillSlot = 0;
            _releasedSkillSlots.Clear();
            SampleTime = 0d;
            LastResult = null;
        }
    }
}

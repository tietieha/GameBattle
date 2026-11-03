using AbilityKit.Demo.Moba.View.Abstractions.Battle.View;
using AbilityKit.Game.Flow;
using NUnit.Framework;

namespace AbilityKit.Game.Test.UnitTest
{
    public sealed class ViewRuntimeHeadlessHarnessTests
    {
        [Test]
        public void SampleTimeResolver_UsesLastFrameAndTickRate()
        {
            var ctx = BattleContext.Rent();
            try
            {
                ctx.LastFrame = 90;
                ctx.Plan = BattleStartPlanBuilder
                    .ForWorld("world", "type", "client", "player", tickRate: 30, inputDelayFrames: 0)
                    .Build();

                var sampleTime = BattleViewSampleTimeResolver.Resolve(ctx.LastFrame, ctx.Plan.World.TickRate);

                Assert.AreEqual(3d, sampleTime, 0.0001d);
            }
            finally
            {
                BattleContext.Return(ctx);
            }
        }

        [Test]
        public void SampleTimeResolver_FallsBackToThirtyTickRate_WhenPlanTickRateIsNotPositive()
        {
            var ctx = BattleContext.Rent();
            try
            {
                ctx.LastFrame = 15;
                ctx.Plan = BattleStartPlanBuilder
                    .ForWorld("world", "type", "client", "player", tickRate: 0, inputDelayFrames: 0)
                    .Build();

                var sampleTime = BattleViewSampleTimeResolver.Resolve(ctx.LastFrame, ctx.Plan.World.TickRate);

                Assert.AreEqual(0.5d, sampleTime, 0.0001d);
            }
            finally
            {
                BattleContext.Return(ctx);
            }
        }

        [Test]
        public void InterpolationClock_AdvancesAndClampsRenderTime_WithFrameAlignedSeekWindow()
        {
            var ctx = BattleContext.Rent();
            try
            {
                ctx.Plan = BattleStartPlanBuilder
                    .ForWorld("world", "type", "client", "player", tickRate: 30, inputDelayFrames: 0)
                    .Build();
                ctx.LastFrame = 60;
                ctx.LogicTimeSeconds = 2d;

                var clock = new BattleViewInterpolationClock();

                var frameAdvanced = clock.Advance(ctx, deltaTime: 0.1f, backTimeTicks: 1f, maxLagTicks: 2f, out var sampleTime);

                Assert.IsTrue(frameAdvanced);
                Assert.AreEqual(2d, sampleTime, 0.0001d);
                Assert.LessOrEqual(clock.RenderTime, 2d);
                Assert.GreaterOrEqual(clock.RenderTime, 1.866d);
            }
            finally
            {
                BattleContext.Return(ctx);
            }
        }

        [Test]
        public void InterpolationClock_ResetClearsRenderState()
        {
            var ctx = BattleContext.Rent();
            try
            {
                ctx.Plan = BattleStartPlanBuilder
                    .ForWorld("world", "type", "client", "player", tickRate: 30, inputDelayFrames: 0)
                    .Build();
                ctx.LastFrame = 10;
                ctx.LogicTimeSeconds = 0.333d;

                var clock = new BattleViewInterpolationClock();
                clock.Advance(ctx, deltaTime: 0.05f, backTimeTicks: 1f, maxLagTicks: 1f, out _);

                clock.Reset();

                Assert.AreEqual(0d, clock.RenderTime, 0.0001d);
            }
            finally
            {
                BattleContext.Return(ctx);
            }
        }
    }
}

using System;
using AbilityKit.Demo.Moba.Testing;

namespace AbilityKit.Game.Flow
{
    /// <summary>
    /// Reusable headless harness for running shared battle test scripts against the moba.view runtime.
    /// The harness owns a rented battle context and releases it when disposed.
    /// </summary>
    public sealed class ViewRuntimeBattleTestHarness : IDisposable
    {
        private readonly BattleTestScriptRunner _runner;
        private bool _disposed;

        public ViewRuntimeBattleTestHarness(BattleContext ctx = null, BattleTestScriptRunner runner = null)
        {
            Context = ctx ?? BattleContext.Rent();
            OwnsContext = ctx == null;
            _runner = runner ?? new BattleTestScriptRunner();
        }

        public BattleContext Context { get; }
        public bool OwnsContext { get; }
        public ViewRuntimeBattleTestDriver LastDriver { get; private set; }
        public BattleTestScriptRunResult LastResult { get; private set; }

        public BattleTestScriptRunResult Run(BattleTestScript script, int tickRate = ViewRuntimeBattleTestDriver.DefaultTickRate)
        {
            ThrowIfDisposed();
            if (script == null) throw new ArgumentNullException(nameof(script));

            LastDriver = new ViewRuntimeBattleTestDriver(Context, tickRate);
            LastResult = _runner.Run(script, LastDriver);
            return LastResult;
        }

        public BattleTestScriptRunResult Run(IBattleTestScenario scenario, int tickRate = ViewRuntimeBattleTestDriver.DefaultTickRate)
        {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));
            return Run(scenario.CreateScript(), tickRate);
        }

        public void ConfigureDefaultPlan(
            string worldId = "world",
            string worldType = "type",
            string clientId = "client",
            string playerId = "player",
            int tickRate = ViewRuntimeBattleTestDriver.DefaultTickRate,
            int inputDelayFrames = 0)
        {
            ThrowIfDisposed();

            Context.Plan = BattleStartPlanBuilder
                .ForWorld(worldId, worldType, clientId, playerId, tickRate, inputDelayFrames)
                .Build();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (OwnsContext)
            {
                BattleContext.Return(Context);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ViewRuntimeBattleTestHarness));
        }
    }
}

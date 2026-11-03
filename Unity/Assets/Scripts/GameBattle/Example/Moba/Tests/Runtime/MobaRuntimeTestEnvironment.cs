using System;
using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Triggering.Testing;

namespace AbilityKit.Demo.Moba.Testing
{
    /// <summary>
    /// Aggregated MOBA test environment for fast configuration and trigger-based validation.
    /// </summary>
    public sealed class MobaRuntimeTestEnvironment<TCtx> : IDisposable
    {
        private bool _disposed;

        public MobaRuntimeTestEnvironment(
            TCtx context = default(TCtx),
            MobaConfigDatabase configDatabase = null,
            TriggeringTestHarness<TCtx> triggering = null,
            BattleTestScriptRunner battleRunner = null)
        {
            ConfigBuilder = new MobaTestConfigBuilder();
            ConfigDatabase = configDatabase ?? new MobaConfigDatabase();
            Triggering = triggering ?? new TriggeringTestHarness<TCtx>(context);
            BattleRunner = battleRunner ?? new BattleTestScriptRunner();
        }

        public MobaTestConfigBuilder ConfigBuilder { get; }

        public MobaConfigDatabase ConfigDatabase { get; private set; }

        public TriggeringTestHarness<TCtx> Triggering { get; }

        public BattleTestScriptRunner BattleRunner { get; }

        public TCtx Context
        {
            get => Triggering.Context;
            set => Triggering.SetContext(value);
        }

        public MobaRuntimeTestEnvironment<TCtx> SetContext(TCtx context)
        {
            ThrowIfDisposed();
            Triggering.SetContext(context);
            return this;
        }

        public MobaRuntimeTestEnvironment<TCtx> LoadConfig(bool strict = false)
        {
            ThrowIfDisposed();
            ConfigDatabase = ConfigBuilder.BuildDatabase(strict);
            return this;
        }

        public BattleTestScriptRunResult RunBattleScript(BattleTestScript script, IBattleTestScriptDriver driver)
        {
            ThrowIfDisposed();
            if (script == null) throw new ArgumentNullException(nameof(script));
            if (driver == null) throw new ArgumentNullException(nameof(driver));

            return BattleRunner.Run(script, driver);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Triggering.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MobaRuntimeTestEnvironment<TCtx>));
            }
        }
    }
}

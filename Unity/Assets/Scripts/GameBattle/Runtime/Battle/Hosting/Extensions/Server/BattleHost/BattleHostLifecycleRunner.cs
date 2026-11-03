using System;

namespace AbilityKit.Ability.Host.Extensions.Server.BattleHost
{
    public delegate BattleHostLifecycleResult BattleHostLifecycleStep(BattleHostStartContext context);

    public delegate BattleHostLifecycleResult BattleHostTimerStarter(BattleHostStartContext context, TimeSpan tickInterval);

    public delegate void BattleHostLifecycleCleanup(BattleHostLifecycleErrorCode reason);

    public sealed class BattleHostLifecycleRunner
    {
        private readonly BattleHostState _state;
        private readonly BattleHostLifecycleStep _createHost;
        private readonly BattleHostLifecycleStep _resolveRuntime;
        private readonly BattleHostLifecycleStep _validateRuntimeStart;
        private readonly BattleHostLifecycleStep _startRuntime;
        private readonly BattleHostLifecycleStep _resolveSnapshotProvider;
        private readonly BattleHostLifecycleStep _publishInitialSnapshot;
        private readonly BattleHostTimerStarter _startTimer;
        private readonly BattleHostLifecycleCleanup _cleanup;

        public BattleHostLifecycleRunner(
            BattleHostState state,
            BattleHostLifecycleStep createHost,
            BattleHostLifecycleStep resolveRuntime,
            BattleHostLifecycleStep validateRuntimeStart,
            BattleHostLifecycleStep startRuntime,
            BattleHostLifecycleStep resolveSnapshotProvider,
            BattleHostLifecycleStep publishInitialSnapshot,
            BattleHostTimerStarter startTimer,
            BattleHostLifecycleCleanup cleanup)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _createHost = createHost ?? throw new ArgumentNullException(nameof(createHost));
            _resolveRuntime = resolveRuntime ?? throw new ArgumentNullException(nameof(resolveRuntime));
            _validateRuntimeStart = validateRuntimeStart ?? throw new ArgumentNullException(nameof(validateRuntimeStart));
            _startRuntime = startRuntime ?? throw new ArgumentNullException(nameof(startRuntime));
            _resolveSnapshotProvider = resolveSnapshotProvider ?? throw new ArgumentNullException(nameof(resolveSnapshotProvider));
            _publishInitialSnapshot = publishInitialSnapshot ?? throw new ArgumentNullException(nameof(publishInitialSnapshot));
            _startTimer = startTimer ?? throw new ArgumentNullException(nameof(startTimer));
            _cleanup = cleanup ?? throw new ArgumentNullException(nameof(cleanup));
        }

        public BattleHostLifecycleContext Context => new BattleHostLifecycleContext(_state);

        public BattleHostLifecycleResult Start(BattleHostStartContext context)
        {
            if (context == null)
            {
                return BattleHostLifecycleResult.Fail(BattleHostLifecycleErrorCode.InvalidContext, "Battle host start context is null.");
            }

            if (_state.Initialized)
            {
                return BattleHostLifecycleResult.Fail(BattleHostLifecycleErrorCode.AlreadyStarted, "Battle host is already started.");
            }

            var result = RunStep(context, _createHost, BattleHostLifecycleErrorCode.CreateHostFailed);
            if (!result.Succeeded)
            {
                return FailAndCleanup(result);
            }

            result = RunStep(context, _resolveRuntime, BattleHostLifecycleErrorCode.RuntimeNotResolved);
            if (!result.Succeeded)
            {
                return FailAndCleanup(result);
            }

            result = RunStep(context, _validateRuntimeStart, BattleHostLifecycleErrorCode.RuntimeNotReadyForStart);
            if (!result.Succeeded)
            {
                return FailAndCleanup(result);
            }

            result = RunStep(context, _startRuntime, BattleHostLifecycleErrorCode.StartRuntimeRejected);
            if (!result.Succeeded)
            {
                return FailAndCleanup(result);
            }

            result = RunStep(context, _resolveSnapshotProvider, BattleHostLifecycleErrorCode.SnapshotProviderNotResolved);
            if (!result.Succeeded)
            {
                return FailAndCleanup(result);
            }

            _state.Initialize(context.WorldId, context.BattleId, context.TickRate);

            result = RunStep(context, _publishInitialSnapshot, BattleHostLifecycleErrorCode.None);
            if (!result.Succeeded)
            {
                return FailAndCleanup(result);
            }

            result = RunTimerStep(context);
            if (!result.Succeeded)
            {
                _state.Reset();
                return FailAndCleanup(result);
            }

            return BattleHostLifecycleResult.Success();
        }

        public BattleHostLifecycleResult Stop()
        {
            try
            {
                _cleanup(BattleHostLifecycleErrorCode.None);
                _state.Reset();
                return BattleHostLifecycleResult.Success();
            }
            catch (Exception ex)
            {
                return BattleHostLifecycleResult.Fail(BattleHostLifecycleErrorCode.StopFailed, ex.Message);
            }
        }

        private BattleHostLifecycleResult RunStep(
            BattleHostStartContext context,
            BattleHostLifecycleStep step,
            BattleHostLifecycleErrorCode fallbackErrorCode)
        {
            try
            {
                var result = step(context);
                if (result.Succeeded || result.ErrorCode != BattleHostLifecycleErrorCode.None || fallbackErrorCode == BattleHostLifecycleErrorCode.None)
                {
                    return result;
                }

                return BattleHostLifecycleResult.Fail(fallbackErrorCode, result.Message);
            }
            catch (Exception ex)
            {
                return BattleHostLifecycleResult.Fail(fallbackErrorCode, ex.Message);
            }
        }

        private BattleHostLifecycleResult RunTimerStep(BattleHostStartContext context)
        {
            try
            {
                var result = _startTimer(context, context.TickInterval);
                if (result.Succeeded || result.ErrorCode != BattleHostLifecycleErrorCode.None)
                {
                    return result;
                }

                return BattleHostLifecycleResult.Fail(BattleHostLifecycleErrorCode.TimerStartFailed, result.Message);
            }
            catch (Exception ex)
            {
                return BattleHostLifecycleResult.Fail(BattleHostLifecycleErrorCode.TimerStartFailed, ex.Message);
            }
        }

        private BattleHostLifecycleResult FailAndCleanup(BattleHostLifecycleResult result)
        {
            _cleanup(result.ErrorCode);
            _state.Reset();
            return result;
        }
    }
}

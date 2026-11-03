using System;
using System.Collections.Generic;
using AbilityKit.Continuous;

namespace AbilityKit.Demo.Moba.Services
{
    internal sealed class MobaContinuousTickProcessor : IMobaContinuousTickProcessor
    {
        internal const int MaxIntervalExecutionsPerTick = FixedIntervalTicker.DefaultExecutionBudget;

        private readonly IReadOnlyList<IMobaContinuousIntervalHandler> _intervalHandlers;

        public MobaContinuousTickProcessor(IReadOnlyList<IMobaContinuousIntervalHandler> intervalHandlers)
        {
            _intervalHandlers = intervalHandlers;
        }

        public void Tick(IContinuous continuous, float deltaTimeSeconds)
        {
            if (continuous == null || !IsFinitePositive(deltaTimeSeconds)) return;
            if (continuous.IsTerminated || !continuous.IsActive || continuous.IsPaused) return;
            if (!(continuous.Config is IMobaContinuousPeriodicConfig periodicConfig)) return;
            if (!(continuous is IMobaContinuousIntervalState intervalState)) return;
            if (periodicConfig.IntervalEffectIds == null || periodicConfig.IntervalEffectIds.Count == 0) return;

            var intervalSeconds = periodicConfig.IntervalSeconds;
            if (!IsFinitePositive(intervalSeconds))
            {
                // Invalid intervals are disabled and normalized instead of ticking every frame.
                intervalState.IntervalRemainingSeconds = 0f;
                return;
            }

            if (!HasIntervalHandler(continuous)) return;

            // 定点推进：全部 raw 整数加减（Q32.32），无 float 累计。
            if (!(continuous is MobaContinuousRuntimeBase runtimeBase))
            {
                TickLegacyFloat(continuous, intervalState, periodicConfig, deltaTimeSeconds, intervalSeconds);
                return;
            }

            var dtRaw = Core.Mathematics.DeterministicMathBridge.ToFixed(deltaTimeSeconds).RawValue;
            var intervalRaw = Core.Mathematics.DeterministicMathBridge.ToFixed(intervalSeconds).RawValue;

            var remainingRaw = runtimeBase.IntervalRemainingRaw - dtRaw;
            if (remainingRaw > 0L)
            {
                runtimeBase.IntervalRemainingRaw = remainingRaw;
                return;
            }

            if (!(continuous is IMobaContinuousExecutionContextProvider contextProvider)
                || !contextProvider.TryGetCombatExecutionContext(out var executionContext))
            {
                runtimeBase.IntervalRemainingRaw = FixedIntervalTicker.NormalizeRemainder(remainingRaw, intervalRaw);
                return;
            }

            var port = new IntervalPort(this, continuous, runtimeBase, periodicConfig, executionContext);
            FixedIntervalTicker.Tick(ref port, dtRaw, intervalRaw, MaxIntervalExecutionsPerTick);
        }

        private readonly struct IntervalPort : IFixedIntervalTickPort
        {
            private readonly MobaContinuousTickProcessor _owner;
            private readonly IContinuous _continuous;
            private readonly MobaContinuousRuntimeBase _runtime;
            private readonly IMobaContinuousPeriodicConfig _config;
            private readonly MobaCombatExecutionContext _context;
            public IntervalPort(MobaContinuousTickProcessor owner, IContinuous continuous,
                MobaContinuousRuntimeBase runtime, IMobaContinuousPeriodicConfig config, MobaCombatExecutionContext context)
            {
                _owner = owner; _continuous = continuous; _runtime = runtime; _config = config; _context = context;
            }
            public long RemainingRaw { get => _runtime.IntervalRemainingRaw; set => _runtime.IntervalRemainingRaw = value; }
            public bool CanExecute => !_continuous.IsTerminated && _continuous.IsActive && !_continuous.IsPaused;
            public void OnInterval() => _owner.DispatchInterval(_continuous, _config, in _context);
        }

        private static void TickLegacyFloat(
            IContinuous continuous,
            IMobaContinuousIntervalState intervalState,
            IMobaContinuousPeriodicConfig periodicConfig,
            float deltaTimeSeconds,
            float intervalSeconds)
        {
            var remainingSeconds = intervalState.IntervalRemainingSeconds;
            if (float.IsNaN(remainingSeconds) || float.IsInfinity(remainingSeconds))
                remainingSeconds = intervalSeconds;

            remainingSeconds -= deltaTimeSeconds;
            if (remainingSeconds > 0f)
            {
                intervalState.IntervalRemainingSeconds = remainingSeconds;
                return;
            }

            intervalState.IntervalRemainingSeconds = NormalizeRemainder(remainingSeconds, intervalSeconds);
        }

        private bool HasIntervalHandler(IContinuous continuous)
        {
            if (_intervalHandlers == null) return false;

            for (var i = 0; i < _intervalHandlers.Count; i++)
            {
                var handler = _intervalHandlers[i];
                if (handler != null && handler.CanHandle(continuous))
                    return true;
            }

            return false;
        }

        private void DispatchInterval(
            IContinuous continuous,
            IMobaContinuousPeriodicConfig periodicConfig,
            in MobaCombatExecutionContext executionContext)
        {
            for (var i = 0; i < _intervalHandlers.Count; i++)
            {
                var handler = _intervalHandlers[i];
                if (handler == null || !handler.CanHandle(continuous)) continue;

                handler.OnInterval(continuous, periodicConfig, in executionContext);
                if (continuous.IsTerminated || !continuous.IsActive || continuous.IsPaused)
                    return;
            }
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float NormalizeRemainder(float remainingSeconds, float intervalSeconds)
        {
            var skippedIntervals = Math.Floor(-(double)remainingSeconds / intervalSeconds) + 1d;
            var normalized = remainingSeconds + skippedIntervals * intervalSeconds;
            if (normalized <= 0d || normalized > intervalSeconds || double.IsNaN(normalized) || double.IsInfinity(normalized))
                return intervalSeconds;

            return (float)normalized;
        }

    }
}

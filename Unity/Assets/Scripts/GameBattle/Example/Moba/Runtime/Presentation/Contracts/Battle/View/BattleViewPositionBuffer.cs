using System;
using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public sealed class BattleViewPositionBuffer
    {
        private const double TimeEpsilon = 1e-6;

        private readonly BattleViewPositionSample[] _samples;
        private int _count;

        public BattleViewPositionBuffer(int capacity)
        {
            capacity = Math.Max(1, capacity);
            _samples = new BattleViewPositionSample[capacity];
            _count = 0;
        }

        public int Count => _count;

        public int Capacity => _samples?.Length ?? 0;

        public void Add(double time, in MobaFloat3 pos)
        {
            EnsureCapacity();

            var sample = new BattleViewPositionSample(time, in pos);
            for (var i = 0; i < _count; i++)
            {
                var existing = _samples[i];
                if (Math.Abs(existing.Time - time) <= TimeEpsilon)
                {
                    _samples[i] = sample;
                    return;
                }
            }

            if (_count == 0)
            {
                _samples[0] = sample;
                _count = 1;
                return;
            }

            var insertAt = FindInsertIndex(time);
            if (_count < _samples.Length)
            {
                InsertAt(insertAt, in sample);
                _count++;
                return;
            }

            if (insertAt <= 0) return;

            ShiftLeftAndAppend(in sample);
        }

        public bool TryEvaluate(double time, out MobaFloat3 pos)
        {
            if (_count <= 0)
            {
                pos = default;
                return false;
            }

            if (_count == 1)
            {
                pos = _samples[0].Position;
                return true;
            }

            var first = _samples[0];
            if (time <= first.Time)
            {
                pos = first.Position;
                return true;
            }

            var last = _samples[_count - 1];
            if (time >= last.Time)
            {
                pos = last.Position;
                return true;
            }

            return TryEvaluateBetweenSamples(time, in last, out pos);
        }

        private int FindInsertIndex(double time)
        {
            for (var i = 0; i < _count; i++)
            {
                if (time < _samples[i].Time)
                {
                    return i;
                }
            }

            return _count;
        }

        private bool TryEvaluateBetweenSamples(double time, in BattleViewPositionSample fallback, out MobaFloat3 pos)
        {
            for (var i = 0; i < _count - 1; i++)
            {
                var a = _samples[i];
                var b = _samples[i + 1];
                if (time < a.Time) continue;
                if (time > b.Time) continue;

                var from = a.Position;
                var to = b.Position;
                pos = BattleViewPositionInterpolator.LerpUnclamped(in from, in to, (float)((time - a.Time) / (b.Time - a.Time)));
                return true;
            }

            pos = fallback.Position;
            return true;
        }

        private void EnsureCapacity()
        {
            if (_samples != null) return;
            throw new InvalidOperationException("Position buffer is not initialized.");
        }

        private void InsertAt(int index, in BattleViewPositionSample sample)
        {
            for (var i = _count; i > index; i--)
            {
                _samples[i] = _samples[i - 1];
            }

            _samples[index] = sample;
        }

        private void ShiftLeftAndAppend(in BattleViewPositionSample sample)
        {
            for (var i = 1; i < _samples.Length; i++)
            {
                _samples[i - 1] = _samples[i];
            }

            _samples[_samples.Length - 1] = sample;
        }
    }
}

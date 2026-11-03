using System;
using System.Collections.Generic;

namespace AbilityKit.Buffs
{
    /// <summary>Single-threaded, budgeted queue; immediate and deferred commands share one execution path.</summary>
    public sealed class BuffCommandQueue<TCommand>
    {
        private readonly struct Entry
        {
            public Entry(long sequence, TCommand command) { Sequence = sequence; Command = command; }
            public long Sequence { get; }
            public TCommand Command { get; }
        }

        private readonly List<Entry> _pending = new List<Entry>(32);
        private readonly HashSet<long> _awaited = new HashSet<long>();
        private readonly Dictionary<long, bool> _results = new Dictionary<long, bool>();
        private readonly Func<TCommand, bool> _execute;
        private readonly Action<TCommand, Exception> _onFailure;
        public long LastSequence { get; private set; }
        public int PendingCount => _pending.Count;
        public bool IsDraining { get; private set; }

        public BuffCommandQueue(Func<TCommand, bool> execute, Action<TCommand, Exception> onFailure = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _onFailure = onFailure;
        }

        public long Enqueue(TCommand command)
        {
            var sequence = checked(LastSequence + 1);
            _pending.Add(new Entry(sequence, command));
            LastSequence = sequence;
            return sequence;
        }

        public int Drain(int maxCommands)
        {
            if (IsDraining || maxCommands <= 0) return 0;
            IsDraining = true;
            var cursor = 0;
            try
            {
                while (cursor < _pending.Count && cursor < maxCommands)
                {
                    var entry = _pending[cursor++];
                    var succeeded = false;
                    try { succeeded = _execute(entry.Command); }
                    catch (Exception failure)
                    {
                        if (_onFailure == null) throw;
                        _onFailure(entry.Command, failure);
                    }
                    finally
                    {
                        if (_awaited.Remove(entry.Sequence)) _results[entry.Sequence] = succeeded;
                    }
                }
                return cursor;
            }
            finally
            {
                // Even a throwing exception policy must not leave executed commands available for replay.
                if (cursor > 0) _pending.RemoveRange(0, cursor);
                IsDraining = false;
            }
        }

        public bool DrainImmediate(long sequence, int maxCommands, Action<int> drain = null) =>
            DrainImmediate(new[] { sequence }, maxCommands, drain) == 1;

        public int DrainImmediate(IReadOnlyList<long> sequences, int maxCommands, Action<int> drain = null)
        {
            if (IsDraining || sequences == null || sequences.Count == 0) return 0;
            var unique = new HashSet<long>(sequences);
            foreach (var sequence in unique) _awaited.Add(sequence);
            var count = 0;
            try
            {
                if (drain == null) Drain(maxCommands);
                else drain(maxCommands);
                foreach (var sequence in unique)
                    if (_results.TryGetValue(sequence, out var succeeded) && succeeded) count++;
                return count;
            }
            finally
            {
                foreach (var sequence in unique)
                {
                    if (!_results.ContainsKey(sequence)) RemovePending(sequence);
                    _awaited.Remove(sequence);
                    _results.Remove(sequence);
                }
            }
        }

        private void RemovePending(long sequence)
        {
            for (var i = _pending.Count - 1; i >= 0; i--)
                if (_pending[i].Sequence == sequence) { _pending.RemoveAt(i); return; }
        }

        public void Clear()
        {
            if (IsDraining) throw new InvalidOperationException("Cannot clear Buff commands during execution.");
            _pending.Clear();
            _awaited.Clear();
            _results.Clear();
        }
    }
}

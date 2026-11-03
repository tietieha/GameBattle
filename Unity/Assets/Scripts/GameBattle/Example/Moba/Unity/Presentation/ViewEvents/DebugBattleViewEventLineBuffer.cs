using System;

namespace AbilityKit.Game.Flow.Battle.ViewEvents
{
    internal sealed class DebugBattleViewEventLineBuffer
    {
        private readonly DebugBattleViewEventLineBufferIndex _index;
        private readonly string[] _lines;
        private int _next;
        private int _count;

        public DebugBattleViewEventLineBuffer(
            int capacity,
            DebugBattleViewEventLineBufferIndex index = null)
        {
            _index = index ?? new DebugBattleViewEventLineBufferIndex();
            _lines = new string[_index.NormalizeCapacity(capacity)];
        }

        public int Total { get; private set; }

        public void Push(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            _lines[_next] = line;
            _next = _index.Next(_next, _lines.Length);
            if (_count < _lines.Length) _count++;
            Total++;
        }

        public string[] GetRecentLines()
        {
            if (_count <= 0) return Array.Empty<string>();

            var count = Math.Min(_count, _lines.Length);
            var result = new string[count];
            var start = _index.Start(_next, count, _lines.Length);
            for (var i = 0; i < count; i++)
            {
                result[i] = _lines[(start + i) % _lines.Length];
            }

            return result;
        }
    }

    internal sealed class DebugBattleViewEventLineBufferIndex
    {
        public int NormalizeCapacity(int capacity)
        {
            return capacity > 0 ? capacity : 16;
        }

        public int Next(int current, int capacity)
        {
            return (current + 1) % capacity;
        }

        public int Start(int next, int count, int capacity)
        {
            return (next - count + capacity) % capacity;
        }
    }
}

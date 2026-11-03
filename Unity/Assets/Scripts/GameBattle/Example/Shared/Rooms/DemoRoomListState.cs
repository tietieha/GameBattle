#nullable enable

using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Common.Rooms
{
    public sealed class DemoRoomListState<TRoom>
    {
        private readonly List<TRoom> _rooms = new List<TRoom>();

        public IReadOnlyList<TRoom> Rooms => _rooms;

        public int Count => _rooms.Count;

        public int SelectedIndex { get; private set; } = -1;

        public int NextOffset { get; private set; }

        public void ReplaceRooms(IEnumerable<TRoom>? rooms, int nextOffset)
        {
            _rooms.Clear();
            if (rooms != null)
            {
                _rooms.AddRange(rooms);
            }

            NextOffset = Math.Max(0, nextOffset);
            if (_rooms.Count == 0)
            {
                SelectedIndex = -1;
            }
            else if (SelectedIndex >= _rooms.Count)
            {
                SelectedIndex = _rooms.Count - 1;
            }
        }

        public bool TrySelect(int index, out TRoom room)
        {
            if (index < 0 || index >= _rooms.Count)
            {
                SelectedIndex = -1;
                room = default!;
                return false;
            }

            SelectedIndex = index;
            room = _rooms[index];
            return true;
        }
    }
}

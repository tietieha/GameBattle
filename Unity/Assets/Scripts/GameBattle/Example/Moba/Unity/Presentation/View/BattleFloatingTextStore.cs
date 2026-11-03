using System;
using System.Collections.Generic;

namespace AbilityKit.Game.Flow.Battle.View
{
    internal sealed class BattleFloatingTextStore
    {
        private readonly List<BattleWorldFloatingText> _items = new List<BattleWorldFloatingText>(64);
        private readonly Action<BattleWorldFloatingText> _release;

        public BattleFloatingTextStore(Action<BattleWorldFloatingText> release = null)
        {
            _release = release;
        }

        public void Add(BattleWorldFloatingText floatingText)
        {
            if (floatingText == null) return;
            _items.Add(floatingText);
        }

        public void Tick(float deltaTime)
        {
            if (_items.Count == 0) return;

            for (var i = _items.Count - 1; i >= 0; i--)
            {
                var floatingText = _items[i];
                if (floatingText == null)
                {
                    _items.RemoveAt(i);
                    continue;
                }

                if (floatingText.Tick(deltaTime)) continue;

                Release(floatingText);
                _items.RemoveAt(i);
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _items.Count; i++)
            {
                Release(_items[i]);
            }

            _items.Clear();
        }

        private void Release(BattleWorldFloatingText floatingText)
        {
            if (floatingText == null) return;
            if (_release != null)
            {
                _release(floatingText);
                return;
            }

            floatingText.Destroy();
        }
    }
}

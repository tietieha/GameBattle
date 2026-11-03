using UnityEngine;

namespace AbilityKit.Game.Flow.Battle.ViewEvents
{
    internal readonly struct BattleFloatingTextSpec
    {
        public BattleFloatingTextSpec(string text, Color color)
        {
            Text = text;
            Color = color;
        }

        public string Text { get; }
        public Color Color { get; }
    }
}

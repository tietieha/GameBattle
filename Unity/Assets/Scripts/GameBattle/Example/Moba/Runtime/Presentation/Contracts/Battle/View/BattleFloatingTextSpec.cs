using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public readonly struct BattleFloatingTextSpec
    {
        public BattleFloatingTextSpec(string text, MobaColor32 color)
        {
            Text = text;
            Color = color;
        }

        public string Text { get; }
        public MobaColor32 Color { get; }
    }
}

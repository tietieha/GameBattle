namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public readonly struct BattleDamageTextSpec
    {
        public BattleDamageTextSpec(string text, bool isHeal)
        {
            Text = text;
            IsHeal = isHeal;
        }

        public string Text { get; }
        public bool IsHeal { get; }
    }
}

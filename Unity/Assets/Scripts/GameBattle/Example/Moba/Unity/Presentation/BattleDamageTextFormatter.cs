using AbstractBattleDamageTextFormatter = AbilityKit.Demo.Moba.View.Abstractions.Battle.View.BattleDamageTextFormatter;

namespace AbilityKit.Game.Flow
{
    internal readonly struct BattleDamageTextSpec
    {
        public BattleDamageTextSpec(string text, bool isHeal)
        {
            Text = text;
            IsHeal = isHeal;
        }

        public string Text { get; }
        public bool IsHeal { get; }
    }

    internal sealed class BattleDamageTextFormatter
    {
        private readonly AbstractBattleDamageTextFormatter _formatter;

        public BattleDamageTextFormatter(AbstractBattleDamageTextFormatter formatter = null)
        {
            _formatter = formatter ?? new AbstractBattleDamageTextFormatter();
        }

        internal AbstractBattleDamageTextFormatter InnerFormatter => _formatter;

        public bool TryFormat(float value, bool isHeal, out BattleDamageTextSpec spec)
        {
            spec = default;
            if (!_formatter.TryFormat(value, isHeal, out AbilityKit.Demo.Moba.View.Abstractions.Battle.View.BattleDamageTextSpec abstractSpec)) return false;

            spec = new BattleDamageTextSpec(abstractSpec.Text, abstractSpec.IsHeal);
            return true;
        }
    }
}

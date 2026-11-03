using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public sealed class BattleDamageFloatingTextFormatter
    {
        private static readonly MobaColor32 DamageColor = new MobaColor32(255, 51, 51, 255);
        private static readonly MobaColor32 HealColor = new MobaColor32(51, 255, 51, 255);

        private readonly BattleDamageTextFormatter _formatter;

        public BattleDamageFloatingTextFormatter(BattleDamageTextFormatter formatter = null)
        {
            _formatter = formatter ?? new BattleDamageTextFormatter();
        }

        public bool TryFormat(float value, bool isHeal, out BattleFloatingTextSpec spec)
        {
            spec = default;
            if (!_formatter.TryFormat(value, isHeal, out BattleDamageTextSpec textSpec)) return false;

            spec = new BattleFloatingTextSpec(textSpec.Text, textSpec.IsHeal ? HealColor : DamageColor);
            return true;
        }
    }
}

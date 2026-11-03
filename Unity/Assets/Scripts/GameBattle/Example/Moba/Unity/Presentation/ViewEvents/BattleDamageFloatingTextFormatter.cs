using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;
using AbilityKit.Game.Flow;
using UnityEngine;
using AbstractBattleDamageFloatingTextFormatter = AbilityKit.Demo.Moba.View.Abstractions.Battle.View.BattleDamageFloatingTextFormatter;

namespace AbilityKit.Game.Flow.Battle.ViewEvents
{
    internal sealed class BattleDamageFloatingTextFormatter
    {
        private readonly AbstractBattleDamageFloatingTextFormatter _formatter;

        public BattleDamageFloatingTextFormatter(BattleDamageTextFormatter formatter = null)
        {
            _formatter = new AbstractBattleDamageFloatingTextFormatter(formatter?.InnerFormatter);
        }

        public bool TryFormat(float value, bool isHeal, out BattleFloatingTextSpec spec)
        {
            spec = default;
            if (!_formatter.TryFormat(value, isHeal, out var abstractSpec)) return false;

            spec = new BattleFloatingTextSpec(abstractSpec.Text, ToUnityColor(abstractSpec.Color));
            return true;
        }

        private static Color ToUnityColor(MobaColor32 color)
        {
            return new Color32(color.R, color.G, color.B, color.A);
        }
    }
}

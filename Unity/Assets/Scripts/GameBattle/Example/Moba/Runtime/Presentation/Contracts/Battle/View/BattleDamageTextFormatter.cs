using System;
using System.Globalization;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public sealed class BattleDamageTextFormatter
    {
        public bool TryFormat(float value, bool isHeal, out string text)
        {
            text = null;

            if (!TryFormat(value, isHeal, out BattleDamageTextSpec spec)) return false;

            text = spec.Text;
            return true;
        }

        public bool TryFormat(float value, bool isHeal, out BattleDamageTextSpec spec)
        {
            spec = default;

            var absValue = Math.Abs(value);
            if (absValue <= 0.0001f) return false;

            var text = FormatAmount(absValue);
            text = (isHeal ? "+" : "-") + text;

            spec = new BattleDamageTextSpec(text, isHeal);
            return true;
        }

        private static string FormatAmount(float amount)
        {
            if (amount >= 1f)
            {
                return ((int)Math.Round(amount, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
            }

            return amount.ToString("0.0", CultureInfo.InvariantCulture);
        }
    }
}

using System;
using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public static class BattleViewPositionInterpolator
    {
        public static MobaFloat3 LerpUnclamped(in MobaFloat3 from, in MobaFloat3 to, float t)
        {
            return new MobaFloat3(
                from.X + (to.X - from.X) * t,
                from.Y + (to.Y - from.Y) * t,
                from.Z + (to.Z - from.Z) * t);
        }
    }
}

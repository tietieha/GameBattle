using AbilityKit.Demo.Moba;

namespace AbilityKit.Demo.Moba.Services.EntityConstruction
{
    public static class MobaEntityKindResolver
    {
        public static MobaEntityKind Resolve(EntityMainType mainType, UnitSubType unitSubType)
        {
            if (mainType != EntityMainType.Unit)
            {
                return MobaEntityKind.Hero;
            }

            switch (unitSubType)
            {
                case UnitSubType.Minion:
                    return MobaEntityKind.Minion;
                case UnitSubType.Neutral:
                case UnitSubType.Boss:
                    return MobaEntityKind.Monster;
                default:
                    return MobaEntityKind.Hero;
            }
        }
    }
}

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public static class BattleEntityKindResolver
    {
        public static BattleEntityKind Resolve(int mainType, int unitSubType)
        {
            if (mainType != 1) return BattleEntityKind.Hero;

            switch (unitSubType)
            {
                case 1:
                    return BattleEntityKind.Minion;
                case 2:
                case 3:
                    return BattleEntityKind.Monster;
                default:
                    return BattleEntityKind.Hero;
            }
        }
    }
}

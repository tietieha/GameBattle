namespace AbilityKit.Demo.Moba
{
    public enum ProjectileEmitterType
    {
        None = 0,
        Linear = 1,
    }

    public enum ProjectileTargetMode
    {
        SkillAim = 0,
        ActorId = 1,
        Search = 2,
    }

    public enum ProjectileFaceMode
    {
        SkillAimDir = 0,
        ToTarget = 1,
        CasterForward = 2,
    }

    public enum ProjectileSpawnMode
    {
        LegacyAimPos = 0,
        FromCaster = 1,
        FromTargetPoint = 2,
    }

    public enum SearchQueryCenterMode
    {
        Caster = 0,
        AimPos = 1,
        ExplicitTarget = 2,
    }

    public enum EffectExecuteMode
    {
        InternalOnly = 0,
    }

    public enum HealFormulaKind
    {
        None = 0,
        Standard = 1,
    }

    public enum DamageCalcStage
    {
        None = 0,
        AttackCreated = 1,
        BeforeCalc = 2,
        CalcBegin = 3,
        AfterBase = 4,
        AfterMitigate = 5,
        AfterShield = 6,
        Final = 7,
        BeforeApply = 8,
        AfterApply = 9,
    }
}

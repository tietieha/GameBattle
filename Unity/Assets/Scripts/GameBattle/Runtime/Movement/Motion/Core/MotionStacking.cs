namespace AbilityKit.Combat.MotionSystem.Core
{
    public enum MotionStacking
    {
        Additive = 0,

        // 同组内只允许最高优先级来源执行并产生贡献。
        ExclusiveHighestPriority = 1,

        // 当前与 ExclusiveHighestPriority 相同，但语义上用于“压制低优先级”的场景。
        OverrideLowerPriority = 2,
    }
}

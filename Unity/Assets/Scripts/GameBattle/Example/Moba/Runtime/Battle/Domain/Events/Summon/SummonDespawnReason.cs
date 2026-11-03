namespace AbilityKit.Demo.Moba.Events.Summon
{
    /// <summary>
    /// 召唤物消失原因
    /// </summary>
    public enum SummonDespawnReason
    {
        /// <summary>无</summary>
        None = 0,

        /// <summary>超时</summary>
        Timeout = 1,

        /// <summary>召唤者死亡</summary>
        OwnerDead = 2,

        /// <summary>被上限替换</summary>
        ReplacedByLimit = 3,

        /// <summary>手动移除</summary>
        ManualRemove = 4,

        /// <summary>被击杀</summary>
        Killed = 5,

        /// <summary>场景清理</summary>
        SceneCleanup = 6,
    }
}

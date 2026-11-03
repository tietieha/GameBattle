namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 条件检查策略。
    /// </summary>
    public enum EConditionCheckStrategy
    {
        /// <summary>
        /// 进入阶段时检查一次。
        /// </summary>
        OnEnter,

        /// <summary>
        /// 持续检查。
        /// </summary>
        Continuous,

        /// <summary>
        /// 指定事件触发时检查。
        /// </summary>
        OnEvent
    }
}

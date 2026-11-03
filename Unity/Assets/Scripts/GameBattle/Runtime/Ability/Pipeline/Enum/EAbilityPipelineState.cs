namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线运行状态。
    /// </summary>
    public enum EAbilityPipelineState
    {
        /// <summary>
        /// 已就绪，尚未开始执行。
        /// </summary>
        Ready = 0,

        /// <summary>
        /// 正在执行中。
        /// </summary>
        Executing = 1,

        /// <summary>
        /// 已完成。
        /// </summary>
        Completed = 2,

        /// <summary>
        /// 执行失败。
        /// </summary>
        Failed = 3,

        /// <summary>
        /// 已暂停。
        /// </summary>
        Paused = 4
    }
}

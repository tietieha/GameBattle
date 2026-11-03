using AbilityKit.Triggering.Runtime.Config;

namespace AbilityKit.Triggering.Runtime.Plan
{
    /// <summary>
    /// 调度模式运行时数据
    /// </summary>
    public readonly struct ScheduleModePlan
    {
        public static ScheduleModePlan None => default;

        /// <summary>
        /// 调度模式
        /// </summary>
        public readonly EScheduleMode Mode;

        /// <summary>
        /// 调度间隔（毫秒），0 表示每次 Update 都可驱动
        /// </summary>
        public readonly float IntervalMs;

        /// <summary>
        /// 最大执行次数，-1=无限
        /// </summary>
        public readonly int MaxExecutions;

        /// <summary>
        /// 是否可中断
        /// </summary>
        public readonly bool CanBeInterrupted;

        public ScheduleModePlan(EScheduleMode mode, float intervalMs = 0, int maxExecutions = -1, bool canBeInterrupted = true)
        {
            Mode = mode;
            IntervalMs = intervalMs;
            MaxExecutions = maxExecutions;
            CanBeInterrupted = canBeInterrupted;
        }

        /// <summary>
        /// 创建外部生命周期控制的持续调度计划。
        /// </summary>
        public static ScheduleModePlan Continuous(float intervalMs = 0, int maxExecutions = -1, bool canBeInterrupted = true)
            => new ScheduleModePlan(EScheduleMode.Continuous, intervalMs, maxExecutions, canBeInterrupted);

        public static ScheduleModePlan Periodic(float intervalMs, int maxExecutions = -1)
            => new ScheduleModePlan(EScheduleMode.Periodic, intervalMs, maxExecutions, canBeInterrupted: true);

        public static ScheduleModePlan Timed(float delayMs)
            => new ScheduleModePlan(EScheduleMode.Timed, delayMs, 1, canBeInterrupted: true);
    }
}

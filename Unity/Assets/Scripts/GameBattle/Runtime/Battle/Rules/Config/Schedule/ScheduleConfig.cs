using System;

namespace AbilityKit.Triggering.Runtime.Config.Schedule
{
    /// <summary>
    /// 调度配置实现（静态配置数据）
    /// </summary>
    [Serializable]
    public struct ScheduleConfig : IScheduleConfig
    {
        public EScheduleMode Mode { get; set; }
        public float DurationMs { get; set; }
        public float PeriodMs { get; set; }
        public int MaxExecutions { get; set; }
        public bool CanBeInterrupted { get; set; }

        public static ScheduleConfig Transient => new ScheduleConfig { Mode = EScheduleMode.Transient };

        public static ScheduleConfig Timed(float delayMs) => new ScheduleConfig
        {
            Mode = EScheduleMode.Timed,
            DurationMs = delayMs
        };

        public static ScheduleConfig Periodic(float periodMs, int maxExecutions = -1) => new ScheduleConfig
        {
            Mode = EScheduleMode.Periodic,
            PeriodMs = periodMs,
            MaxExecutions = maxExecutions
        };

        public static ScheduleConfig TimedPeriodic(float delayMs, float periodMs, int maxExecutions = -1) => new ScheduleConfig
        {
            Mode = EScheduleMode.Periodic,
            DurationMs = delayMs,
            PeriodMs = periodMs,
            MaxExecutions = maxExecutions
        };

        /// <summary>
        /// 外部生命周期控制的持续 tick 行为。
        /// PeriodMs 表示 tick 间隔，0 表示每帧；DurationMs 小于等于 0 表示不由调度配置自动结束。
        /// </summary>
        /// <param name="periodMs">tick 间隔，0=每帧</param>
        /// <param name="maxExecutions">最大 tick 次数，-1=无限</param>
        /// <param name="canBeInterrupted">是否可被中断</param>
        /// <param name="durationMs">最大持续时间，0=无限</param>
        public static ScheduleConfig Continuous(float periodMs = 0, int maxExecutions = -1, bool canBeInterrupted = true, float durationMs = 0) => new ScheduleConfig
        {
            Mode = EScheduleMode.Continuous,
            DurationMs = durationMs,
            PeriodMs = periodMs,
            CanBeInterrupted = canBeInterrupted,
            MaxExecutions = maxExecutions
        };

        /// <summary>
        /// 兼容旧调用：仅声明是否可中断和最大 tick 次数。
        /// </summary>
        public static ScheduleConfig Continuous(bool canBeInterrupted, int maxExecutions = -1) => Continuous(0, maxExecutions, canBeInterrupted);

        /// <summary>
        /// 语义更明确的旧式入口：仅声明是否可中断和最大 tick 次数。
        /// </summary>
        public static ScheduleConfig ContinuousInterruptible(bool canBeInterrupted = true, int maxExecutions = -1) => Continuous(0, maxExecutions, canBeInterrupted);

        public bool IsEmpty => Mode == EScheduleMode.Transient && DurationMs == 0 && PeriodMs == 0;
    }
}
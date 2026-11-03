using AbilityKit.Demo.Moba.Services.Motion;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    /// <summary>
    /// pull Action 的强类型参数。
    /// </summary>
    public readonly struct PullArgs
    {
        /// <summary>
        /// 拉取速度，单位为距离/秒。
        /// </summary>
        public readonly float Speed;

        /// <summary>
        /// 拉取持续时间，单位毫秒。
        /// </summary>
        public readonly float DurationMs;

        /// <summary>
        /// 拉取方向模式：0=从目标拉向技能释放者，1=从目标拉到指定距离，2=垂直击飞并回落。
        /// </summary>
        public readonly int DirectionMode;

        /// <summary>
        /// 目标距离，仅 DirectionMode=1 时使用。
        /// </summary>
        public readonly float TargetDistance;

        /// <summary>
        /// 运动优先级。
        /// </summary>
        public readonly int Priority;

        /// <summary>
        /// 位移组 ID；0 表示使用行为默认组。
        /// </summary>
        public readonly int MotionGroupId;

        public readonly MobaMotionContinuousSettings Continuous;

        public readonly MobaActionTargetRequest TargetRequest;

        public PullArgs(float speed, float durationMs, int directionMode = 0, float targetDistance = 0f, int priority = 12, int motionGroupId = 0, MobaMotionContinuousSettings continuous = default, MobaActionTargetRequest targetRequest = default)
        {
            Speed = speed;
            DurationMs = durationMs;
            DirectionMode = directionMode;
            TargetDistance = targetDistance;
            Priority = priority;
            MotionGroupId = motionGroupId;
            Continuous = continuous;
            TargetRequest = targetRequest;
        }

        public static PullArgs Default => new PullArgs(0f, 0f, 0, 0f, 12, 0, MobaMotionContinuousSettings.Empty, MobaActionTargetRequest.ContextTarget());
    }
}

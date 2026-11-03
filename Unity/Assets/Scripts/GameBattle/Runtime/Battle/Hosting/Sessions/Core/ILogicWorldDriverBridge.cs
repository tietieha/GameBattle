namespace AbilityKit.Coordinator
{
    /// <summary>
    /// 同步适配器使用的通用逻辑世界驱动桥接接口。
    /// </summary>
    public interface ILogicWorldDriverBridge
    {
        /// <summary>
        /// 当前逻辑帧号。
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 逻辑时间，单位为秒。
        /// </summary>
        double LogicTimeSeconds { get; }

        /// <summary>
        /// 驱动器是否正在运行。
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 启动驱动器生命周期。
        /// </summary>
        void Start();

        /// <summary>
        /// 停止驱动器生命周期。
        /// </summary>
        void Stop();

        /// <summary>
        /// 提交输入以供处理。
        /// </summary>
        void SubmitInputs(PlayerInput[] inputs);

        /// <summary>
        /// 推进一个逻辑帧。
        /// </summary>
        void AdvanceFrame(float deltaTime);

        /// <summary>
        /// 获取用于渲染或状态同步的全部实体状态。
        /// </summary>
        SnapshotEntityState[] GetAllEntityStates();
    }
}

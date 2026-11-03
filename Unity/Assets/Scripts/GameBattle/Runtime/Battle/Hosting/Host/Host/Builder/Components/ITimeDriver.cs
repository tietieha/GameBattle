using AbilityKit.Ability.Host.Framework;

namespace AbilityKit.Ability.Host.Builder.Components
{
    /// <summary>
    /// 时间驱动接口
    /// 负责驱动 Host 的 Tick 循环
    /// </summary>
    public interface ITimeDriver
    {
        /// <summary>
        /// 附加到 Runtime
        /// </summary>
        void Attach(HostRuntime runtime, HostRuntimeOptions options);

        /// <summary>
        /// 从 Runtime 分离
        /// </summary>
        void Detach();

        /// <summary>
        /// 是否运行中
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 开始驱动
        /// </summary>
        void Start();

        /// <summary>
        /// 停止驱动
        /// </summary>
        void Stop();

        /// <summary>
        /// 帧率
        /// </summary>
        int FrameRate { get; set; }
    }
}

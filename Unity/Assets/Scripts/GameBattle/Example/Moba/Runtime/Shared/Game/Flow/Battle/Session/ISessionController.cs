using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 会话控制器接口
    /// 定义会话生命周期控制契约，供不同平台实现
    /// </summary>
    public interface ISessionController
    {
        /// <summary>
        /// 开始会话
        /// </summary>
        void Start();

        /// <summary>
        /// 停止会话
        /// </summary>
        void Stop();

        /// <summary>
        /// 会话是否正在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 当前帧索引
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 获取固定时间步长（秒）
        /// </summary>
        float FixedDeltaSeconds { get; }
    }

    /// <summary>
    /// 会话状态
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// 空闲状态
        /// </summary>
        Idle = 0,

        /// <summary>
        /// 初始化中
        /// </summary>
        Initializing = 1,

        /// <summary>
        /// 运行中
        /// </summary>
        Running = 2,

        /// <summary>
        /// 暂停中
        /// </summary>
        Paused = 3,

        /// <summary>
        /// 停止中
        /// </summary>
        Stopping = 4,

        /// <summary>
        /// 已停止
        /// </summary>
        Stopped = 5,

        /// <summary>
        /// 错误状态
        /// </summary>
        Error = 6,
    }
}

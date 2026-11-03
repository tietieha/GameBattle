using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 帧时间提供者接口
    /// 定义获取帧时间和速率的契约
    /// </summary>
    public interface IFrameTimeProvider
    {
        /// <summary>
        /// 获取当前帧索引
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 获取上一帧索引
        /// </summary>
        int LastFrame { get; }

        /// <summary>
        /// 获取固定时间步长（秒）
        /// </summary>
        float FixedDeltaSeconds { get; }

        /// <summary>
        /// 获取帧率（每秒帧数）
        /// </summary>
        float FrameRate { get; }

        /// <summary>
        /// 获取逻辑时间（秒）
        /// </summary>
        double LogicTimeSeconds { get; }

        /// <summary>
        /// 获取累计时间（用于变步长积分）
        /// </summary>
        float TickAccumulator { get; }

        /// <summary>
        /// 是否首帧已接收
        /// </summary>
        bool IsFirstFrameReceived { get; }

        /// <summary>
        /// 获取时间缩放
        /// </summary>
        float TimeScale { get; set; }
    }

    /// <summary>
    /// 主线程调度器接口
    /// 定义在主线程上执行操作的契约
    /// </summary>
    public interface IMainThreadDispatcher
    {
        /// <summary>
        /// 在主线程上执行操作
        /// </summary>
        /// <param name="action">要执行的操作</param>
        void Dispatch(Action action);

        /// <summary>
        /// 在主线程上执行操作（异步）
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <returns>是否成功加入队列</returns>
        bool TryDispatch(Action action);

        /// <summary>
        /// 是否在主线程上
        /// </summary>
        bool IsOnMainThread { get; }

        /// <summary>
        /// 清空待执行的操作
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// 帧同步控制器接口
    /// 定义帧同步行为的控制契约
    /// </summary>
    public interface IFrameSyncController
    {
        /// <summary>
        /// 是否暂停
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// 暂停帧同步
        /// </summary>
        void Pause();

        /// <summary>
        /// 恢复帧同步
        /// </summary>
        void Resume();

        /// <summary>
        /// 前进到指定帧
        /// </summary>
        void AdvanceToFrame(int targetFrame);

        /// <summary>
        /// 设置帧率
        /// </summary>
        void SetFrameRate(int framesPerSecond);

        /// <summary>
        /// 获取目标帧
        /// </summary>
        int TargetFrame { get; }

        /// <summary>
        /// 是否正在追帧
        /// </summary>
        bool IsCatchingUp { get; }

        /// <summary>
        /// 获取帧延迟
        /// </summary>
        int FrameDelay { get; }
    }

    /// <summary>
    /// 帧同步状态
    /// </summary>
    public enum FrameSyncState
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle = 0,

        /// <summary>
        /// 运行中
        /// </summary>
        Running = 1,

        /// <summary>
        /// 暂停
        /// </summary>
        Paused = 2,

        /// <summary>
        /// 追帧
        /// </summary>
        CatchingUp = 3,

        /// <summary>
        /// 等待同步
        /// </summary>
        WaitingForSync = 4,
    }
}

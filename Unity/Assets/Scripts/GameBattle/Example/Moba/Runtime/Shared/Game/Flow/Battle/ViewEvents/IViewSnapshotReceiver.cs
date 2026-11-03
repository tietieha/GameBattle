using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 视图快照接收器接口
    /// 定义接收帧快照的契约，供不同平台实现
    /// </summary>
    public interface IViewSnapshotReceiver
    {
        /// <summary>
        /// 接收快照帧
        /// </summary>
        void OnSnapshot(int frameIndex);

        /// <summary>
        /// 定位到指定帧
        /// </summary>
        void SeekToFrame(int frameIndex, float secondsPerFrame);

        /// <summary>
        /// 当前帧是否有效
        /// </summary>
        bool HasValidFrame { get; }

        /// <summary>
        /// 当前帧索引
        /// </summary>
        int CurrentFrame { get; }
    }

    /// <summary>
    /// 视图触发器事件接收器接口
    /// 定义接收触发器事件的契约
    /// </summary>
    public interface IViewTriggerEventReceiver
    {
        /// <summary>
        /// 处理触发器事件
        /// </summary>
        void OnTriggerEvent(in TriggerEventData evt);
    }

    /// <summary>
    /// 组合视图事件接收器接口
    /// 组合快照和触发器事件接收能力
    /// </summary>
    public interface IViewEventReceiver : IViewSnapshotReceiver, IViewTriggerEventReceiver
    {
    }
}

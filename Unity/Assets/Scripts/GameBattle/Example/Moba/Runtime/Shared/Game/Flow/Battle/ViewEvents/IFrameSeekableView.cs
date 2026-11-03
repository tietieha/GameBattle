using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 帧可定位视图接口
    /// 可在帧时间轴上定位的视图实现此接口
    /// </summary>
    public interface IFrameSeekableView
    {
        /// <summary>
        /// 定位到指定帧
        /// </summary>
        void SeekToFrame(int frameIndex, float secondsPerFrame);
    }

    /// <summary>
    /// 视图事件源模式
    /// </summary>
    public enum BattleViewEventSourceMode
    {
        /// <summary>
        /// 仅快照模式
        /// </summary>
        SnapshotOnly = 0,

        /// <summary>
        /// 仅触发模式
        /// </summary>
        TriggerOnly = 1,

        /// <summary>
        /// 混合模式
        /// </summary>
        Hybrid = 2,
    }
}

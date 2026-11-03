using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 快照应用器接口
    /// 定义将快照数据应用到视图的契约
    /// 
    /// 平台层实现此接口，负责：
    /// 1. 接收帧快照数据
    /// 2. 创建/更新/销毁视图对象
    /// 3. 同步位置、旋转、缩放等变换数据
    /// </summary>
    public interface ISnapshotApplier
    {
        /// <summary>
        /// 应用进入游戏快照
        /// </summary>
        /// <param name="snapshot">快照数据</param>
        void ApplyEnterGame(in FrameSnapshotData snapshot);

        /// <summary>
        /// 应用角色变换快照
        /// </summary>
        /// <param name="snapshot">快照数据</param>
        void ApplyActorTransform(in FrameSnapshotData snapshot);

        /// <summary>
        /// 应用弹道事件快照
        /// </summary>
        /// <param name="snapshot">快照数据</param>
        void ApplyProjectileEvent(in FrameSnapshotData snapshot);

        /// <summary>
        /// 应用区域事件快照
        /// </summary>
        /// <param name="snapshot">快照数据</param>
        void ApplyAreaEvent(in FrameSnapshotData snapshot);

        /// <summary>
        /// 应用伤害事件快照
        /// </summary>
        /// <param name="snapshot">快照数据</param>
        void ApplyDamageEvent(in FrameSnapshotData snapshot);

        /// <summary>
        /// 应用状态哈希快照（调试/验证）
        /// </summary>
        /// <param name="snapshot">快照数据</param>
        void ApplyStateHash(in FrameSnapshotData snapshot);

        /// <summary>
        /// 重置所有视图到初始状态
        /// </summary>
        void ResetAllViews();
    }

    /// <summary>
    /// 快照应用器工厂接口
    /// </summary>
    public interface ISnapshotApplierFactory
    {
        /// <summary>
        /// 创建快照应用器
        /// </summary>
        /// <param name="viewFactory">视图工厂</param>
        /// <param name="viewSink">视图事件接收器</param>
        /// <returns>快照应用器实例</returns>
        ISnapshotApplier Create(IViewFactory viewFactory, IBattleViewEventSink viewSink);
    }
}

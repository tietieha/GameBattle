using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 视图事件适配器接口
    /// 定义将底层事件转换为视图事件的适配器
    /// </summary>
    public interface IViewEventAdapter
    {
        /// <summary>
        /// 适配器名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize(IBattleViewEventSink sink);

        /// <summary>
        /// 清理
        /// </summary>
        void Cleanup();
    }

    /// <summary>
    /// 快照视图事件适配器接口
    /// 专门处理快照相关的事件
    /// </summary>
    public interface ISnapshotViewAdapter : IViewEventAdapter
    {
        /// <summary>
        /// 订阅快照分发器
        /// </summary>
        void Subscribe(IFrameSnapshotDispatcher dispatcher);

        /// <summary>
        /// 取消订阅
        /// </summary>
        void Unsubscribe(IFrameSnapshotDispatcher dispatcher);
    }

    /// <summary>
    /// 触发器视图事件适配器接口
    /// 专门处理触发器相关的事件
    /// </summary>
    public interface ITriggerEventAdapter : IViewEventAdapter
    {
        /// <summary>
        /// 订阅触发器事件
        /// </summary>
        void Subscribe(ITriggerEventSource source);

        /// <summary>
        /// 取消订阅
        /// </summary>
        void Unsubscribe(ITriggerEventSource source);
    }

    /// <summary>
    /// 触发器事件源接口
    /// 定义触发器事件的分发源
    /// </summary>
    public interface ITriggerEventSource
    {
        /// <summary>
        /// 订阅事件
        /// </summary>
        void Subscribe(Action<TriggerEventData> handler);

        /// <summary>
        /// 取消订阅
        /// </summary>
        void Unsubscribe(Action<TriggerEventData> handler);
    }

    /// <summary>
    /// 视图事件路由接口
    /// 定义事件到视图的路由逻辑
    /// </summary>
    public interface IViewEventRouter
    {
        /// <summary>
        /// 路由帧快照
        /// </summary>
        void RouteSnapshot(in FrameSnapshotData snapshot);

        /// <summary>
        /// 路由触发器事件
        /// </summary>
        void RouteTriggerEvent(in TriggerEventData evt);
    }

    /// <summary>
    /// 视图事件处理链接口
    /// 定义事件处理链的构建和执行
    /// </summary>
    public interface IViewEventChain
    {
        /// <summary>
        /// 添加处理器到链
        /// </summary>
        void AddHandler(IViewEventHandler handler);

        /// <summary>
        /// 移除处理器
        /// </summary>
        void RemoveHandler(IViewEventHandler handler);

        /// <summary>
        /// 处理快照事件
        /// </summary>
        void HandleSnapshot(in FrameSnapshotData snapshot);

        /// <summary>
        /// 处理触发器事件
        /// </summary>
        void HandleTriggerEvent(in TriggerEventData evt);

        /// <summary>
        /// 清空所有处理器
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// 视图事件处理器接口
    /// </summary>
    public interface IViewEventHandler
    {
        /// <summary>
        /// 处理器名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 优先级
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 处理快照
        /// </summary>
        bool HandleSnapshot(in FrameSnapshotData snapshot);

        /// <summary>
        /// 处理触发事件
        /// </summary>
        bool HandleTriggerEvent(in TriggerEventData evt);
    }
}

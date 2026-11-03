using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// Feature 模块上下文接口
    /// 提供 Feature 运行时所需的上下文信息
    /// </summary>
    /// <typeparam name="THost">宿主类型</typeparam>
    public interface IFeatureModuleContext<THost> where THost : class
    {
        /// <summary>
        /// 获取宿主对象
        /// </summary>
        THost Host { get; }

        /// <summary>
        /// 获取阶段上下文
        /// </summary>
        GamePhaseContext PhaseContext { get; }
    }

    /// <summary>
    /// Feature 模块基类接口
    /// 所有 Feature 模块实现此接口
    /// </summary>
    public interface IFeatureModule
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 优先级（数值越小越先执行）
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }
    }

    /// <summary>
    /// Feature 模块标记接口
    /// 用于标记 Feature 模块的类型
    /// </summary>
    /// <typeparam name="THost">宿主类型</typeparam>
    public interface IFeatureModule<THost> : IFeatureModule where THost : class
    {
    }

    /// <summary>
    /// 可附加的 Feature 模块接口
    /// </summary>
    /// <typeparam name="THost">宿主类型</typeparam>
    public interface IAttachableModule<THost> : IFeatureModule<THost> where THost : class
    {
        /// <summary>
        /// 附加到宿主
        /// </summary>
        void OnAttach(IFeatureModuleContext<THost> ctx);

        /// <summary>
        /// 从宿主分离
        /// </summary>
        void OnDetach(IFeatureModuleContext<THost> ctx);
    }

    /// <summary>
    /// 可 Tick 的 Feature 模块接口
    /// </summary>
    /// <typeparam name="THost">宿主类型</typeparam>
    public interface ITickableModule<THost> : IFeatureModule<THost> where THost : class
    {
        /// <summary>
        /// 每帧 Tick
        /// </summary>
        void Tick(IFeatureModuleContext<THost> ctx, float deltaTime);
    }

    /// <summary>
    /// 主 Tick SubFeature 接口
    /// 在主逻辑 Tick 阶段执行
    /// </summary>
    /// <typeparam name="THost">宿主类型</typeparam>
    public interface ISessionMainTickSubFeature<THost> : IAttachableModule<THost>, ITickableModule<THost> where THost : class
    {
    }

    /// <summary>
    /// 视图 SubFeature 接口
    /// 用于视图层的功能模块
    /// </summary>
    /// <typeparam name="THost">宿主类型</typeparam>
    public interface IViewSubFeature<THost> : IAttachableModule<THost>, ITickableModule<THost> where THost : class
    {
    }

    /// <summary>
    /// Feature 模块宿主接口
    /// 管理多个 Feature 模块的生命周期
    /// </summary>
    /// <typeparam name="TContext">上下文类型</typeparam>
    /// <typeparam name="THost">宿主类型</typeparam>
    public interface IFeatureModuleHost<TContext, THost> where THost : class
    {
        /// <summary>
        /// 附加所有模块
        /// </summary>
        void Attach(TContext ctx);

        /// <summary>
        /// 分离所有模块
        /// </summary>
        void Detach(TContext ctx);

        /// <summary>
        /// Tick 所有模块
        /// </summary>
        void Tick(TContext ctx, float deltaTime);

        /// <summary>
        /// 添加模块
        /// </summary>
        void AddModule<TModule>() where TModule : IFeatureModule<THost>, new();

        /// <summary>
        /// 移除模块
        /// </summary>
        void RemoveModule<TModule>() where TModule : IFeatureModule<THost>;
    }

    /// <summary>
    /// Feature 模块上下文实现
    /// </summary>
    /// <typeparam name="THost">宿主类型</typeparam>
    public readonly struct FeatureModuleContext<THost> : IFeatureModuleContext<THost> where THost : class
    {
        public THost Host { get; }
        public GamePhaseContext PhaseContext { get; }

        public FeatureModuleContext(GamePhaseContext phaseContext, THost host)
        {
            PhaseContext = phaseContext;
            Host = host;
        }
    }
}

using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 游戏阶段 Feature 接口
    /// 定义游戏阶段内可附加的功能模块
    /// </summary>
    public interface IGamePhaseFeature
    {
        /// <summary>
        /// Feature 名称
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

        /// <summary>
        /// 附加到游戏阶段
        /// </summary>
        void OnAttach(in GamePhaseContext ctx);

        /// <summary>
        /// 从游戏阶段分离
        /// </summary>
        void OnDetach(in GamePhaseContext ctx);

        /// <summary>
        /// 每帧更新
        /// </summary>
        void Tick(in GamePhaseContext ctx, float deltaTime);
    }

    /// <summary>
    /// Feature 基类
    /// 提供 IGamePhaseFeature 的默认实现
    /// </summary>
    public abstract class BaseGamePhaseFeature : IGamePhaseFeature
    {
        public abstract string Name { get; }
        public virtual int Priority => 0;
        public virtual bool IsEnabled { get; set; } = true;

        public abstract void OnAttach(in GamePhaseContext ctx);
        public abstract void OnDetach(in GamePhaseContext ctx);
        public abstract void Tick(in GamePhaseContext ctx, float deltaTime);
    }

    /// <summary>
    /// 游戏阶段宿主接口
    /// 管理多个 IGamePhaseFeature 的生命周期
    /// </summary>
    public interface IGamePhaseFeatureHost
    {
        /// <summary>
        /// 获取当前上下文
        /// </summary>
        GamePhaseContext Context { get; }

        /// <summary>
        /// 附加 Feature
        /// </summary>
        void Attach(IGamePhaseFeature feature);

        /// <summary>
        /// 分离 Feature
        /// </summary>
        void Detach(IGamePhaseFeature feature);

        /// <summary>
        /// Tick 所有 Feature
        /// </summary>
        void Tick(float deltaTime);
    }

    /// <summary>
    /// 游戏阶段宿主实现
    /// 管理多个 IGamePhaseFeature 的生命周期
    /// </summary>
    public sealed class GamePhaseFeatureHost : IGamePhaseFeatureHost
    {
        private readonly System.Collections.Generic.List<IGamePhaseFeature> _features = new System.Collections.Generic.List<IGamePhaseFeature>();
        private GamePhaseContext _context;
        private bool _isAttached;
        private bool _isDisposed;
        private readonly object _lock = new object();

        /// <inheritdoc />
        public GamePhaseContext Context => _context;

        /// <inheritdoc />
        public void Attach(IGamePhaseFeature feature)
        {
            if (_isDisposed || feature == null) return;

            lock (_lock)
            {
                _features.Add(feature);

                if (_isAttached)
                {
                    try
                    {
                        feature.OnAttach(in _context);
                    }
                    catch (Exception ex)
                    {
                        System.Console.Error.WriteLine($"[GamePhaseFeatureHost] Feature attach error: {ex}");
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Detach(IGamePhaseFeature feature)
        {
            if (feature == null) return;

            lock (_lock)
            {
                int index = _features.IndexOf(feature);
                if (index >= 0)
                {
                    if (_isAttached)
                    {
                        try
                        {
                            feature.OnDetach(in _context);
                        }
                        catch (Exception ex)
                        {
                            System.Console.Error.WriteLine($"[GamePhaseFeatureHost] Feature detach error: {ex}");
                        }
                    }

                    _features.RemoveAt(index);
                }
            }
        }

        /// <inheritdoc />
        public void Tick(float deltaTime)
        {
            if (!_isAttached || _isDisposed) return;

            lock (_lock)
            {
                for (int i = 0; i < _features.Count; i++)
                {
                    var feature = _features[i];
                    if (feature.IsEnabled)
                    {
                        try
                        {
                            feature.Tick(in _context, deltaTime);
                        }
                        catch (Exception ex)
                        {
                            System.Console.Error.WriteLine($"[GamePhaseFeatureHost] Feature tick error: {ex}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 初始化宿主
        /// </summary>
        public void Initialize(in GamePhaseContext context)
        {
            lock (_lock)
            {
                _context = context;

                // 排序
                _features.Sort((a, b) => a.Priority.CompareTo(b.Priority));

                // 附加所有 Feature
                for (int i = 0; i < _features.Count; i++)
                {
                    try
                    {
                        _features[i].OnAttach(in _context);
                    }
                    catch (Exception ex)
                    {
                        System.Console.Error.WriteLine($"[GamePhaseFeatureHost] Feature attach error: {ex}");
                    }
                }

                _isAttached = true;
            }
        }

        /// <summary>
        /// 关闭宿主
        /// </summary>
        public void Shutdown()
        {
            lock (_lock)
            {
                if (!_isAttached) return;

                for (int i = _features.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        _features[i].OnDetach(in _context);
                    }
                    catch (Exception ex)
                    {
                        System.Console.Error.WriteLine($"[GamePhaseFeatureHost] Feature detach error: {ex}");
                    }
                }

                _isAttached = false;
                _context = default;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Shutdown();

            lock (_lock)
            {
                _features.Clear();
            }
        }
    }
}

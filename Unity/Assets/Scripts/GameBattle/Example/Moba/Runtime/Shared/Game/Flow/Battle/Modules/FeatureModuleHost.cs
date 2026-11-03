using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// Feature 模块宿主实现
    /// 管理多个 Feature 模块的生命周期
    /// 参考 view.runtime 的 ModuleHost 实现
    /// </summary>
    public sealed class FeatureModuleHost<TContext, THost> : IFeatureModuleHost<TContext, THost> where THost : class
    {
        private readonly List<IFeatureModule<THost>> _modules = new List<IFeatureModule<THost>>();
        private readonly List<IFeatureModule<THost>>[] _priorityBuckets;
        private readonly object _lock = new object();
        private TContext _context;
        private THost _host;
        private bool _isAttached;
        private bool _isDisposed;

        private const int PriorityRange = 100;

        public FeatureModuleHost()
        {
            _priorityBuckets = new List<IFeatureModule<THost>>[PriorityRange * 2 + 1];
            for (int i = 0; i < _priorityBuckets.Length; i++)
            {
                _priorityBuckets[i] = new List<IFeatureModule<THost>>();
            }
        }

        /// <summary>
        /// 附加所有模块
        /// </summary>
        public void Attach(TContext ctx)
        {
            if (_isDisposed) return;

            lock (_lock)
            {
                _context = ctx;

                for (int i = 0; i < _modules.Count; i++)
                {
                    var module = _modules[i];
                    if (module is IAttachableModule<THost> attachable)
                    {
                        try
                        {
                            var fctx = CreateContext();
                            attachable.OnAttach(fctx);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[FeatureModuleHost] Module attach error: {ex}");
                        }
                    }
                }

                _isAttached = true;
            }
        }

        /// <summary>
        /// 分离所有模块
        /// </summary>
        public void Detach(TContext ctx)
        {
            if (!_isAttached) return;

            lock (_lock)
            {
                for (int i = _modules.Count - 1; i >= 0; i--)
                {
                    var module = _modules[i];
                    if (module is IAttachableModule<THost> attachable)
                    {
                        try
                        {
                            var fctx = CreateContext();
                            attachable.OnDetach(fctx);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[FeatureModuleHost] Module detach error: {ex}");
                        }
                    }
                }

                _isAttached = false;
                _context = default;
            }
        }

        /// <summary>
        /// Tick 所有模块
        /// </summary>
        public void Tick(TContext ctx, float deltaTime)
        {
            if (!_isAttached || _isDisposed) return;

            lock (_lock)
            {
                for (int bucketIdx = 0; bucketIdx < _priorityBuckets.Length; bucketIdx++)
                {
                    var bucket = _priorityBuckets[bucketIdx];
                    for (int i = 0; i < bucket.Count; i++)
                    {
                        var module = bucket[i];
                        if (module.IsEnabled && module is ITickableModule<THost> tickable)
                        {
                            try
                            {
                                var fctx = CreateContext();
                                tickable.Tick(fctx, deltaTime);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[FeatureModuleHost] Module tick error: {ex}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加模块
        /// </summary>
        public void AddModule<TModule>() where TModule : IFeatureModule<THost>, new()
        {
            var module = new TModule();
            AddModule(module);
        }

        /// <summary>
        /// 添加模块
        /// </summary>
        public void AddModule(IFeatureModule<THost> module)
        {
            if (_isDisposed || module == null) return;

            lock (_lock)
            {
                _modules.Add(module);
                GetPriorityBucket(module.Priority).Add(module);

                if (_isAttached && module is IAttachableModule<THost> attachable)
                {
                    try
                    {
                        var fctx = CreateContext();
                        attachable.OnAttach(fctx);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[FeatureModuleHost] Module attach error: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// 移除模块
        /// </summary>
        public void RemoveModule<TModule>() where TModule : IFeatureModule<THost>
        {
            lock (_lock)
            {
                for (int i = _modules.Count - 1; i >= 0; i--)
                {
                    if (_modules[i] is TModule)
                    {
                        RemoveModuleAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 移除模块
        /// </summary>
        public void RemoveModule(IFeatureModule<THost> module)
        {
            if (module == null) return;

            lock (_lock)
            {
                int index = _modules.IndexOf(module);
                if (index >= 0)
                {
                    RemoveModuleAt(index);
                }
            }
        }

        /// <summary>
        /// 获取模块数量
        /// </summary>
        public int ModuleCount
        {
            get
            {
                lock (_lock)
                {
                    return _modules.Count;
                }
            }
        }

        /// <summary>
        /// 遍历所有指定类型的模块
        /// </summary>
        public void ForEach<TModule>(Action<TModule> action) where TModule : class
        {
            lock (_lock)
            {
                for (int i = 0; i < _modules.Count; i++)
                {
                    if (_modules[i] is TModule module)
                    {
                        action(module);
                    }
                }
            }
        }

        /// <summary>
        /// 设置宿主对象
        /// </summary>
        public void SetHost(THost host)
        {
            _host = host;
        }

        private void RemoveModuleAt(int index)
        {
            var module = _modules[index];

            if (_isAttached && module is IAttachableModule<THost> attachable)
            {
                try
                {
                    var fctx = CreateContext();
                    attachable.OnDetach(fctx);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[FeatureModuleHost] Module detach error: {ex}");
                }
            }

            GetPriorityBucket(module.Priority).Remove(module);
            _modules.RemoveAt(index);
        }

        private List<IFeatureModule<THost>> GetPriorityBucket(int priority)
        {
            int bucketIdx = Math.Max(0, Math.Min(PriorityRange * 2, priority + PriorityRange));
            return _priorityBuckets[bucketIdx];
        }

        private FeatureModuleContext<THost> CreateContext()
        {
            return new FeatureModuleContext<THost>(_context is GamePhaseContext gpc ? gpc : default, _host);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Detach(_context);

            lock (_lock)
            {
                _modules.Clear();
                _context = default;
                _host = null;
            }
        }
    }

    /// <summary>
    /// ViewSubFeature 模块实现基类
    /// 提供视图子模块的通用实现
    /// </summary>
    public abstract class BaseViewSubFeature<THost> : IViewSubFeature<THost> where THost : class
    {
        public virtual string Name => GetType().Name;
        public virtual string Id => GetType().Name;
        public virtual int Priority => 0;
        public virtual bool IsEnabled { get; set; } = true;

        public virtual void OnAttach(IFeatureModuleContext<THost> ctx) { }
        public virtual void OnDetach(IFeatureModuleContext<THost> ctx) { }
        public virtual void Tick(IFeatureModuleContext<THost> ctx, float deltaTime) { }
        public virtual void RebindAll(IFeatureModuleContext<THost> ctx) { }
    }

    /// <summary>
    /// SessionMainTickSubFeature 模块实现基类
    /// 提供主 Tick 模块的通用实现
    /// </summary>
    public abstract class BaseSessionMainTickSubFeature<THost> : ISessionMainTickSubFeature<THost> where THost : class
    {
        public virtual string Name => GetType().Name;
        public virtual string Id => GetType().Name;
        public virtual int Priority => 0;
        public virtual bool IsEnabled { get; set; } = true;

        public virtual void OnAttach(IFeatureModuleContext<THost> ctx) { }
        public virtual void OnDetach(IFeatureModuleContext<THost> ctx) { }
        public virtual void Tick(IFeatureModuleContext<THost> ctx, float deltaTime) { }
        public virtual void MainTick(IFeatureModuleContext<THost> ctx, float deltaTime) { }
        public virtual void RebindAll(IFeatureModuleContext<THost> ctx) { }
    }
}

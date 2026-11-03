using System;
using AbilityKit.Ability.World.DI;
using AbilityKit.Ability.World.Services;
using AbilityKit.Core.Eventing;
using AbilityKit.Core.Logging;

namespace AbilityKit.Demo.Moba.Services.Templates
{
    /// <summary>
    /// 需要统一日志与释放钩子的逻辑世界服务基类。
    /// </summary>
    public abstract class LogicWorldServiceBase<TService> : IService
        where TService : class
    {
        private bool _disposed;

        protected virtual string ServiceName => typeof(TService).Name;

        protected bool IsDisposed => _disposed;

        protected void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(ServiceName);
        }

        protected void LogInfo(string message) => Log.Info($"[{ServiceName}] {message}");

        protected void LogWarning(string message) => Log.Warning($"[{ServiceName}] {message}");

        protected void LogError(string message) => Log.Error($"[{ServiceName}] {message}");

        protected void LogException(Exception ex, string context = "")
        {
            var msg = string.IsNullOrEmpty(context) ? ex.Message : $"{context}: {ex.Message}";
            Log.Exception(ex, $"[{ServiceName}] {msg}");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }
    }

    /// <summary>
    /// 容器构建完成后需要访问世界服务解析器的服务基类。
    /// </summary>
    public abstract class LogicWorldInitializableServiceBase<TService> : LogicWorldServiceBase<TService>, IWorldInitializable
        where TService : class
    {
        protected IWorldResolver WorldResolver { get; private set; }

        protected bool TryResolve<T>(out T service) where T : class
        {
            service = null;
            return WorldResolver != null && WorldResolver.TryResolve(out service);
        }

        protected T Resolve<T>() where T : class
        {
            if (WorldResolver == null) throw new InvalidOperationException($"[{ServiceName}] WorldResolver is null");
            return WorldResolver.Resolve<T>();
        }

        public virtual void OnInit(IWorldResolver resolver)
        {
            WorldResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            OnServicesReady(resolver);
        }

        protected virtual void OnServicesReady(IWorldResolver resolver)
        {
        }
    }

    /// <summary>
    /// 同时需要世界初始化与反初始化钩子的服务基类。
    /// </summary>
    public abstract class LogicWorldLifecycleServiceBase<TService> : LogicWorldInitializableServiceBase<TService>, IWorldDeinitializable
        where TService : class
    {
        private bool _deinitialized;

        public virtual void OnDeinit(IWorldResolver resolver)
        {
            if (_deinitialized) return;
            _deinitialized = true;
            OnWorldDeinit(resolver);
        }

        protected virtual void OnWorldDeinit(IWorldResolver resolver)
        {
        }
    }

    /// <summary>
    /// 会发布触发事件的逻辑世界服务基类。
    /// </summary>
    public abstract class LogicWorldEventServiceBase<TService> : LogicWorldInitializableServiceBase<TService>
        where TService : class
    {
        protected AbilityKit.Triggering.Eventing.IEventBus EventBus { get; private set; }

        protected override void OnServicesReady(IWorldResolver resolver)
        {
            EventBus = resolver.Resolve<AbilityKit.Triggering.Eventing.IEventBus>();
        }

        protected void Publish<T>(string eventId, in T payload) where T : struct
        {
            if (EventBus == null)
            {
                LogWarning("EventBus is null, cannot publish event");
                return;
            }

            var eid = TriggeringIdUtil.GetEventEid(eventId);
            EventBus.Publish(new EventKey<T>(eid), in payload);

            var objectKey = new EventKey<object>(eid);
            if (EventBus.HasSubscribers(objectKey))
            {
                object boxed = payload;
                EventBus.Publish(objectKey, in boxed);
            }
        }
    }

    public abstract class GameServiceBase<TService> : LogicWorldServiceBase<TService>
        where TService : class
    {
    }

    public abstract class GameInitializableServiceBase<TService> : LogicWorldInitializableServiceBase<TService>
        where TService : class
    {
    }

    public abstract class GameLifecycleServiceBase<TService> : LogicWorldLifecycleServiceBase<TService>
        where TService : class
    {
    }

    public abstract class GameEventPublisherServiceBase<TService> : LogicWorldEventServiceBase<TService>
        where TService : class
    {
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;

namespace AbilityKit.Protocol
{
    /// <summary>
    /// Server Push Handler 注册表
    /// 
    /// 提供基于 Attribute 的自动 Handler 注册和查找
    /// 
    /// 使用方式：
    /// <code>
    /// // 启动时扫描程序集并注册所有 Handler
    /// ServerPushHandlerRegistry.Instance.ScanAndRegister(typeof(ServerPushHandlerRegistry).Assembly);
    /// 
    /// // 获取 Handler
    /// var handler = ServerPushHandlerRegistry.Instance.GetHandler(opCode);
    /// handler?.Handle(payload);
    /// </code>
    /// </summary>
    public sealed class ServerPushHandlerRegistry
    {
        private static readonly Lazy<ServerPushHandlerRegistry> _instance = new(() => new ServerPushHandlerRegistry());
        
        /// <summary>
        /// 单例实例
        /// </summary>
        public static ServerPushHandlerRegistry Instance => _instance.Value;

        private readonly Dictionary<uint, IServerPushHandler> _handlers = new();
        private bool _isScanned;

        private ServerPushHandlerRegistry() { }

        /// <summary>
        /// 扫描程序集并自动注册所有带 ServerPushHandlerAttribute 的 Handler
        /// </summary>
        public void ScanAndRegister(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                ScanAndRegister(assembly);
            }
        }

        /// <summary>
        /// 扫描程序集并自动注册所有带 ServerPushHandlerAttribute 的 Handler
        /// </summary>
        public void ScanAndRegister(Assembly assembly)
        {
            if (assembly == null) return;

            foreach (var type in assembly.GetTypes())
            {
                RegisterHandlerType(type);
            }
            _isScanned = true;
        }

        /// <summary>
        /// 注册单个 Handler 类型
        /// </summary>
        public void RegisterHandlerType(Type handlerType)
        {
            if (handlerType == null) return;
            if (!handlerType.IsClass || handlerType.IsAbstract) return;
            if (!typeof(IServerPushHandler).IsAssignableFrom(handlerType)) return;

            var attr = handlerType.GetCustomAttribute<ServerPushHandlerAttribute>();
            if (attr == null) return;

            IServerPushHandler handler;
            try
            {
                handler = (IServerPushHandler)Activator.CreateInstance(handlerType)!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create instance of handler {handlerType.FullName}", ex);
            }

            Register(attr.OpCode, handler);
        }

        /// <summary>
        /// 手动注册 Handler
        /// </summary>
        public void Register(uint opCode, IServerPushHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (_handlers.ContainsKey(opCode))
            {
                throw new InvalidOperationException($"Handler for OpCode {opCode} is already registered. Existing: {_handlers[opCode].GetType().FullName}, New: {handler.GetType().FullName}");
            }

            _handlers[opCode] = handler;
        }

        /// <summary>
        /// 注册 Handler（泛型版本）
        /// </summary>
        public void Register<T>() where T : IServerPushHandler, new()
        {
            var handler = new T();
            Register(handler.OpCode, handler);
        }

        /// <summary>
        /// 获取 Handler
        /// </summary>
        public IServerPushHandler? GetHandler(uint opCode)
        {
            return _handlers.TryGetValue(opCode, out var handler) ? handler : null;
        }

        /// <summary>
        /// 尝试获取 Handler
        /// </summary>
        public bool TryGetHandler(uint opCode, out IServerPushHandler? handler)
        {
            return _handlers.TryGetValue(opCode, out handler);
        }

        /// <summary>
        /// 获取所有已注册的 Handler
        /// </summary>
        public IReadOnlyDictionary<uint, IServerPushHandler> GetAllHandlers()
        {
            return _handlers;
        }

        /// <summary>
        /// 获取所有已注册的 OpCode
        /// </summary>
        public IReadOnlyCollection<uint> GetAllOpCodes()
        {
            return _handlers.Keys;
        }

        /// <summary>
        /// 检查是否已扫描
        /// </summary>
        public bool IsScanned => _isScanned;

        /// <summary>
        /// 取消注册 Handler
        /// </summary>
        public bool Unregister(uint opCode)
        {
            return _handlers.Remove(opCode);
        }

        /// <summary>
        /// 清空所有 Handler（通常用于测试）
        /// </summary>
        public void Clear()
        {
            _handlers.Clear();
            _isScanned = false;
        }

        /// <summary>
        /// 处理推送数据
        /// </summary>
        public void Handle(uint opCode, byte[] payload)
        {
            var handler = GetHandler(opCode);
            if (handler != null)
            {
                handler.Handle(payload);
            }
            else
            {
                OnUnhandledPush(opCode, payload);
            }
        }

        /// <summary>
        /// 未处理的推送回调
        /// </summary>
        private void OnUnhandledPush(uint opCode, byte[] payload)
        {
            Console.WriteLine($"[ServerPushHandlerRegistry] No handler registered for OpCode {opCode}");
        }
    }
}

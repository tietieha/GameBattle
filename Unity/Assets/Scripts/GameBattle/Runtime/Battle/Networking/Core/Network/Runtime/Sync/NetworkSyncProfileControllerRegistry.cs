#nullable enable

using System;
using System.Collections.Generic;

namespace AbilityKit.Network.Runtime.Sync
{
    /// <summary>
    /// 根据调用方持有的上下文构建指定 profile 的同步控制器。
    /// </summary>
    public delegate TController NetworkSyncProfileControllerBuilder<TController, TContext>(in TContext context);

    /// <summary>
    /// 面向示例与玩法客户端的可复用 profile 键控控制器注册表。
    /// <para>
    /// 框架代码负责 profile 选择与注册机制；玩法示例只需提供自身的强类型上下文和控制器构建器，
    /// 从而让示例工厂保持轻量，同时继续兼容基于旧模型的 API。
    /// </para>
    /// </summary>
    public sealed class NetworkSyncProfileControllerRegistry<TController, TContext>
    {
        private readonly object _syncRoot = new();
        private readonly IReadOnlyDictionary<NetworkSyncProfile, NetworkSyncProfileControllerBuilder<TController, TContext>> _defaultBuilders;
        private Dictionary<NetworkSyncProfile, NetworkSyncProfileControllerBuilder<TController, TContext>> _builders;

        public NetworkSyncProfileControllerRegistry(
            IReadOnlyDictionary<NetworkSyncProfile, NetworkSyncProfileControllerBuilder<TController, TContext>> defaultBuilders)
        {
            if (defaultBuilders == null) throw new ArgumentNullException(nameof(defaultBuilders));

            _defaultBuilders = Copy(defaultBuilders);
            _builders = Copy(_defaultBuilders);
        }

        public int Count => _builders.Count;

        public void Register(
            NetworkSyncModel syncModel,
            NetworkSyncProfileControllerBuilder<TController, TContext> builder)
        {
            Register(NetworkSyncProfileRegistry.Resolve(syncModel), builder);
        }

        public void Register(
            in NetworkSyncProfile syncProfile,
            NetworkSyncProfileControllerBuilder<TController, TContext> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            lock (_syncRoot)
            {
                var builders = new Dictionary<NetworkSyncProfile, NetworkSyncProfileControllerBuilder<TController, TContext>>(_builders)
                {
                    [syncProfile] = builder
                };
                _builders = builders;
            }
        }

        public void ResetToDefaults()
        {
            lock (_syncRoot)
            {
                _builders = Copy(_defaultBuilders);
            }
        }

        public bool Supports(in NetworkSyncProfile syncProfile)
        {
            return _builders.ContainsKey(syncProfile);
        }

        public bool Supports(NetworkSyncModel syncModel)
        {
            return Supports(NetworkSyncProfileRegistry.Resolve(syncModel));
        }

        public TController Create(
            NetworkSyncModel syncModel,
            in TContext context,
            string subjectName = "sync controller")
        {
            return Create(NetworkSyncProfileRegistry.Resolve(syncModel), in context, subjectName);
        }

        public TController Create(
            in NetworkSyncProfile syncProfile,
            in TContext context,
            string subjectName = "sync controller")
        {
            var builders = _builders;
            if (!builders.TryGetValue(syncProfile, out var builder))
            {
                throw new NotSupportedException(
                    $"No {subjectName} is registered for sync profile '{syncProfile.CompatibilityModel}'.");
            }

            return builder(in context);
        }

        private static Dictionary<NetworkSyncProfile, NetworkSyncProfileControllerBuilder<TController, TContext>> Copy(
            IReadOnlyDictionary<NetworkSyncProfile, NetworkSyncProfileControllerBuilder<TController, TContext>> source)
        {
            var copy = new Dictionary<NetworkSyncProfile, NetworkSyncProfileControllerBuilder<TController, TContext>>(source.Count);
            foreach (var entry in source)
            {
                if (entry.Value == null)
                {
                    throw new ArgumentException("Controller builder entries cannot be null.", nameof(source));
                }

                copy[entry.Key] = entry.Value;
            }

            return copy;
        }
    }
}

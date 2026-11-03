#nullable enable

using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Shooter.View.Network
{
    /// <summary>
    /// 宿主无关的运行时桥接器，用于发布当前运行中的 Shooter 会话 host，
    /// 让工具无需引用具体平台 host 即可发现并观察它。
    /// </summary>
    public static class ShooterHostSessionRegistry
    {
        private static readonly List<IShooterSessionHost> _hosts = new();

        /// <summary>已注册 host 集合变化时触发。</summary>
        public static event Action? HostsChanged;

        /// <summary>最近注册的活跃 host；没有运行中 host 时为 null。</summary>
        public static IShooterSessionHost? Active
        {
            get
            {
                for (var i = _hosts.Count - 1; i >= 0; i--)
                {
                    if (_hosts[i].IsRunning)
                    {
                        return _hosts[i];
                    }
                }

                return _hosts.Count > 0 ? _hosts[_hosts.Count - 1] : null;
            }
        }

        /// <summary>当前所有已注册 host。</summary>
        public static IReadOnlyList<IShooterSessionHost> All => _hosts;

        public static void Register(IShooterSessionHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            if (!_hosts.Contains(host))
            {
                _hosts.Add(host);
                NotifyHostsChanged();
            }
        }

        public static void Unregister(IShooterSessionHost host)
        {
            if (_hosts.Remove(host))
            {
                NotifyHostsChanged();
            }
        }

        public static void NotifyHostsChanged()
        {
            HostsChanged?.Invoke();
        }
    }

    /// <summary>
    /// 会话 host 暴露给工具的表面接口，让工具无需引用具体运行时或平台实现细节即可观察诊断并请求拆卸。
    /// </summary>
    public interface IShooterSessionHost
    {
        /// <summary>当前已经装配会话并正在推进时为 true。</summary>
        bool IsRunning { get; }

        /// <summary>host 的人类可读标签（场景/对象名称）。</summary>
        string DisplayName { get; }

        /// <summary>正在运行的验收会话；启动前或停止后为 null。</summary>
        ShooterAcceptanceSession? Session { get; }

        /// <summary>
        /// 请求 host 拆卸正在运行的会话，并释放 host 专属的平台接线（例如 PlayerLoop、输入、网络钩子）。
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// 向后兼容的 Play-mode 注册表门面。新工具应使用 <see cref="ShooterHostSessionRegistry"/>。
    /// </summary>
    public static class ShooterPlayModeSessionRegistry
    {
        /// <summary>已注册 host 集合变化时触发。</summary>
        public static event Action? HostsChanged
        {
            add => ShooterHostSessionRegistry.HostsChanged += value;
            remove => ShooterHostSessionRegistry.HostsChanged -= value;
        }

        /// <summary>最近注册的活跃 host；没有运行中 host 时为 null。</summary>
        public static IShooterPlayModeSessionHost? Active => ShooterHostSessionRegistry.Active as IShooterPlayModeSessionHost;

        /// <summary>当前所有已注册 Play-mode host。</summary>
        public static IReadOnlyList<IShooterPlayModeSessionHost> All
        {
            get
            {
                var hosts = ShooterHostSessionRegistry.All;
                var result = new List<IShooterPlayModeSessionHost>(hosts.Count);
                for (var i = 0; i < hosts.Count; i++)
                {
                    if (hosts[i] is IShooterPlayModeSessionHost playModeHost)
                    {
                        result.Add(playModeHost);
                    }
                }

                return result;
            }
        }

        public static void Register(IShooterPlayModeSessionHost host)
        {
            ShooterHostSessionRegistry.Register(host);
        }

        public static void Unregister(IShooterPlayModeSessionHost host)
        {
            ShooterHostSessionRegistry.Unregister(host);
        }

        public static void NotifyHostsChanged()
        {
            ShooterHostSessionRegistry.NotifyHostsChanged();
        }
    }

    /// <summary>
    /// 向后兼容的 Play-mode host 契约。新 host 除非需要旧版 Editor 集成，否则应直接实现
    /// <see cref="IShooterSessionHost"/>。
    /// </summary>
    public interface IShooterPlayModeSessionHost : IShooterSessionHost
    {
    }
}

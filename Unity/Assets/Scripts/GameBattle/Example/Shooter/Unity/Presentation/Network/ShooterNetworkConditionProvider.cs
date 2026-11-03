#nullable enable

using System;
using System.Collections.Generic;
using AbilityKit.Network.Runtime.Conditioning;

namespace AbilityKit.Demo.Shooter.View.Network
{
    /// <summary>
    /// 网络条件来源抽象。Editor 窗口（Editor-direct 模式）和 Play-mode 会话 host 都通过它获取当前
    /// <see cref="NetworkConditionProfile"/>，无需关心数值来自内置滑条、预设目录还是外部工具
    /// （例如 Clumsy、Network Link Conditioner 或自定义丢包注入器）。
    /// <para>
    /// 扩展方式：实现该接口即可把软件网络调节（操作系统级丢包、代理延迟注入等）接入内置参数滑条之外。
    /// 订阅方通过 <see cref="ProfileChanged"/> 感知变化，并转发给 <see cref="ShooterAcceptanceSession.ApplyNetwork"/>。
    /// </para>
    /// <para>
    /// 该类型位于 runtime 程序集（View.Runtime，无平台限制），方便 Play-mode 运行时代码注册与读取 provider。
    /// Editor 窗口也引用同一组类型，因此一个注册表即可同时服务 Editor-direct 与 Play-mode-attach 通道。
    /// </para>
    /// </summary>
    public interface IShooterNetworkConditionProvider : IDisposable
    {
        /// <summary>显示在 Editor 窗口中的人类可读标签。</summary>
        string DisplayName { get; }

        /// <summary>当前网络条件档案。</summary>
        NetworkConditionProfile Profile { get; }

        /// <summary>
        /// 当该 provider 正在主动控制网络条件时为 true（例如外部工具正在运行）。
        /// 为 false 时改用内置参数滑条。
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 档案变化时触发（可能来自外部工具反馈，也可能来自内部参数变化）。
        /// 订阅方会把变化转发给正在运行的会话。
        /// </summary>
        event Action<NetworkConditionProfile>? ProfileChanged;
    }

    /// <summary>
    /// 由滑条数值驱动的内置 provider。没有外部工具连接时默认使用它。
    /// </summary>
    public sealed class ShooterBuiltinNetworkConditionProvider : IShooterNetworkConditionProvider
    {
        private NetworkConditionProfile _profile;

        public ShooterBuiltinNetworkConditionProvider(NetworkConditionProfile initialProfile)
        {
            _profile = initialProfile;
        }

        public string DisplayName => "Built-in (Parameters)";

        public NetworkConditionProfile Profile => _profile;

        public bool IsActive => true;

        public event Action<NetworkConditionProfile>? ProfileChanged;

        /// <summary>
        /// 根据滑条数值更新档案并通知订阅方。用户在 Editor 窗口调整网络参数滑条时调用。
        /// </summary>
        public void ApplyProfile(NetworkConditionProfile profile)
        {
            _profile = profile;
            ProfileChanged?.Invoke(_profile);
        }

        /// <summary>
        /// 应用一个预设档案并通知订阅方。
        /// </summary>
        public void ApplyPreset(NetworkConditionProfile profile, string presetName)
        {
            _profile = profile;
            ProfileChanged?.Invoke(_profile);
        }

        public void Dispose()
        {
            ProfileChanged = null;
        }
    }

    /// <summary>
    /// <see cref="IShooterNetworkConditionProvider"/> 实例注册表。Editor 窗口查询它来填充网络来源下拉框。
    /// 外部工具启用时在这里注册，禁用时注销。Play-mode 会话 host 订阅该注册表，
    /// 让运行中的对局通过 Editor 写入的同一注册表完成热调参。
    /// <para>
    /// 扩展点：若要新增网络调节来源（例如 Clumsy 集成、自定义丢包中间件），实现
    /// <see cref="IShooterNetworkConditionProvider"/> 并在工具启动代码中调用 <see cref="Register"/>。
    /// </para>
    /// </summary>
    public static class ShooterNetworkConditionRegistry
    {
        private static readonly List<IShooterNetworkConditionProvider> _providers = new();
        private static readonly ShooterBuiltinNetworkConditionProvider _builtin =
            new(NetworkConditionProfile.Ideal);

        /// <summary>基于滑条的内置 provider，始终可用。</summary>
        public static ShooterBuiltinNetworkConditionProvider Builtin => _builtin;

        /// <summary>全部已注册 provider，包含内置 provider。</summary>
        public static IReadOnlyList<IShooterNetworkConditionProvider> All
        {
            get
            {
                var result = new List<IShooterNetworkConditionProvider>(_providers.Count + 1) { _builtin };
                result.AddRange(_providers);
                return result;
            }
        }

        /// <summary>注册一个外部网络条件 provider。</summary>
        public static void Register(IShooterNetworkConditionProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }

        /// <summary>注销一个外部网络条件 provider。</summary>
        public static void Unregister(IShooterNetworkConditionProvider provider)
        {
            _providers.Remove(provider);
        }
    }
}

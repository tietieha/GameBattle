#nullable enable

using System;
using AbilityKit.Demo.Shooter.View;
using AbilityKit.Demo.Shooter.View.Hosting;
using AbilityKit.Demo.Shooter.View.Network;
using AbilityKit.Network.Runtime.Conditioning;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace AbilityKit.Demo.Shooter.View.PlayMode
{
    /// <summary>
    /// Shooter 验收通道的 Unity Play-mode 绑定。
    /// <para>
    /// 会话语义位于 <see cref="ShooterPlaySessionRunner"/> 中；该类型只把 Unity 生命周期、PlayerLoop 时间、
    /// 旧输入和 GameObject 展示适配器绑定到这个宿主无关的 runner 上。
    /// </para>
    /// </summary>
    public static class ShooterPlayModeSessionHost
    {
        private static readonly PublishedHost Published = new();
        private static readonly UnityShooterPlayInputSource InputSource = new();
        private static readonly UnityShooterSwitchableViewSink ViewSink = new();
        private static ShooterPlaySessionRunner? _runner;
        private static bool _registered;
        private static bool _playerLoopInstalled;
        private static bool _networkHookInstalled;

        public static event Action<ShooterAcceptanceSession?>? SessionChanged;

        public static bool IsInstalled => _registered || _networkHookInstalled || _playerLoopInstalled;
        public static bool IsRunning => _runner?.IsRunning == true;
        public static ShooterAcceptanceSession? Current => _runner?.Session;
        public static ShooterPlayModeSessionOptions Options => _runner?.Options ?? ShooterPlayModeSessionOptions.Default;
        public static ShooterHostFrameInput LastInput => _runner?.LastInput ?? default;
        public static ShooterClientInputSubmitResult LastSubmitResult => _runner?.LastSubmitResult ?? default;
        public static ShooterClientFrameTickResult LastTickResult => _runner?.LastTickResult ?? default;
        public static int LastAuthorityAcceptedInputs => _runner?.LastAuthorityAcceptedInputs ?? 0;
        public static long StepCount => _runner?.StepCount ?? 0L;
        public static long RenderCount => _runner?.RenderCount ?? 0L;
        public static long DroppedCatchUpTicks => _runner?.DroppedCatchUpTicks ?? 0L;
        public static ShooterUnityViewRenderBackend ViewBackend => ViewSink.Backend;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Uninstall();
            SessionChanged = null;
        }

        public static void Install()
        {
            RegisterPublishedHost();
            InstallNetworkHook();
            InstallPlayerLoop();
            Application.quitting -= OnApplicationQuitting;
            Application.quitting += OnApplicationQuitting;
        }

        public static void Uninstall()
        {
            DisposeRunner();
            UninstallNetworkHook();
            UninstallPlayerLoop();
            UnregisterPublishedHost();
            Application.quitting -= OnApplicationQuitting;
        }

        public static ShooterAcceptanceSession Start(ShooterPlayModeSessionOptions options)
        {
            Install();
            EnsureRunner();
            var session = _runner!.Start(options);
            ShooterHostSessionRegistry.NotifyHostsChanged();
            return session;
        }

        public static void Stop()
        {
            if (_runner == null)
            {
                ViewSink.Clear();
                return;
            }

            _runner.Stop();
            ShooterHostSessionRegistry.NotifyHostsChanged();
        }

        public static void Tick(float deltaSeconds)
        {
            _runner?.Tick(deltaSeconds);
        }

        public static void RebuildViews()
        {
            ViewSink.RebuildAll();
        }

        public static void SetViewBackend(ShooterUnityViewRenderBackend backend)
        {
            ViewSink.SetBackend(backend);
            if (_runner?.IsRunning == true)
            {
                ViewSink.RebuildAll();
            }
        }

        private static void TickFromPlayerLoop()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Tick(Time.deltaTime);
        }

        private static void OnNetworkProfileChanged(NetworkConditionProfile profile)
        {
            _runner?.ApplyNetwork(profile);
        }

        private static void EnsureRunner()
        {
            if (_runner != null)
            {
                return;
            }

            _runner = new ShooterPlaySessionRunner(InputSource, ViewSink);
            _runner.SessionChanged += OnRunnerSessionChanged;
        }

        private static void DisposeRunner()
        {
            if (_runner == null)
            {
                ViewSink.Clear();
                return;
            }

            _runner.SessionChanged -= OnRunnerSessionChanged;
            _runner.Dispose();
            _runner = null;
        }

        private static void OnRunnerSessionChanged(ShooterAcceptanceSession? session)
        {
            ShooterHostSessionRegistry.NotifyHostsChanged();
            SessionChanged?.Invoke(session);
        }

        private static void InstallNetworkHook()
        {
            if (_networkHookInstalled)
            {
                return;
            }

            ShooterNetworkConditionRegistry.Builtin.ProfileChanged += OnNetworkProfileChanged;
            _networkHookInstalled = true;
        }

        private static void UninstallNetworkHook()
        {
            if (!_networkHookInstalled)
            {
                return;
            }

            ShooterNetworkConditionRegistry.Builtin.ProfileChanged -= OnNetworkProfileChanged;
            _networkHookInstalled = false;
        }

        private static void RegisterPublishedHost()
        {
            if (_registered)
            {
                return;
            }

            ShooterHostSessionRegistry.Register(Published);
            _registered = true;
        }

        private static void UnregisterPublishedHost()
        {
            if (!_registered)
            {
                return;
            }

            ShooterHostSessionRegistry.Unregister(Published);
            _registered = false;
        }

        private static void OnApplicationQuitting()
        {
            Uninstall();
        }

        private static void InstallPlayerLoop()
        {
            if (_playerLoopInstalled)
            {
                return;
            }

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            if (!InsertIntoUpdate(ref loop))
            {
                Debug.LogWarning("[ShooterPlayModeSessionHost] Failed to install PlayerLoop update node.");
                return;
            }

            PlayerLoop.SetPlayerLoop(loop);
            _playerLoopInstalled = true;
        }

        private static void UninstallPlayerLoop()
        {
            if (!_playerLoopInstalled)
            {
                return;
            }

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            if (RemoveFromPlayerLoop(ref loop))
            {
                PlayerLoop.SetPlayerLoop(loop);
            }

            _playerLoopInstalled = false;
        }

        private static bool InsertIntoUpdate(ref PlayerLoopSystem root)
        {
            if (root.subSystemList == null)
            {
                return false;
            }

            for (var i = 0; i < root.subSystemList.Length; i++)
            {
                ref var system = ref root.subSystemList[i];
                if (system.type == typeof(Update))
                {
                    system.subSystemList = AppendOrReplace(system.subSystemList, new PlayerLoopSystem
                    {
                        type = typeof(ShooterPlayModeSessionPlayerLoopNode),
                        updateDelegate = TickFromPlayerLoop
                    });
                    return true;
                }

                if (InsertIntoUpdate(ref system))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RemoveFromPlayerLoop(ref PlayerLoopSystem root)
        {
            if (root.subSystemList == null || root.subSystemList.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < root.subSystemList.Length; i++)
            {
                if (root.subSystemList[i].type == typeof(ShooterPlayModeSessionPlayerLoopNode))
                {
                    root.subSystemList = RemoveAt(root.subSystemList, i);
                    return true;
                }

                var child = root.subSystemList[i];
                if (RemoveFromPlayerLoop(ref child))
                {
                    root.subSystemList[i] = child;
                    return true;
                }
            }

            return false;
        }

        private static PlayerLoopSystem[] AppendOrReplace(PlayerLoopSystem[]? systems, PlayerLoopSystem node)
        {
            if (systems == null || systems.Length == 0)
            {
                return new[] { node };
            }

            for (var i = 0; i < systems.Length; i++)
            {
                if (systems[i].type == node.type)
                {
                    systems[i] = node;
                    return systems;
                }
            }

            var result = new PlayerLoopSystem[systems.Length + 1];
            Array.Copy(systems, result, systems.Length);
            result[result.Length - 1] = node;
            return result;
        }

        private static PlayerLoopSystem[] RemoveAt(PlayerLoopSystem[] systems, int index)
        {
            if (systems.Length == 1)
            {
                return Array.Empty<PlayerLoopSystem>();
            }

            var result = new PlayerLoopSystem[systems.Length - 1];
            if (index > 0)
            {
                Array.Copy(systems, 0, result, 0, index);
            }

            if (index < systems.Length - 1)
            {
                Array.Copy(systems, index + 1, result, index, systems.Length - index - 1);
            }

            return result;
        }

        private sealed class PublishedHost : IShooterSessionHost
        {
            public bool IsRunning => ShooterPlayModeSessionHost.IsRunning;
            public string DisplayName => "Shooter Play Session Host";
            public ShooterAcceptanceSession? Session => ShooterPlayModeSessionHost.Current;

            public void Stop()
            {
                // 完整生命周期拆卸：停止运行中的会话，并释放所有 Unity 平台绑定（PlayerLoop / 网络钩子 /
                // 注册表发布）。
                ShooterPlayModeSessionHost.Uninstall();
            }
        }

        private struct ShooterPlayModeSessionPlayerLoopNode
        {
        }
    }
}

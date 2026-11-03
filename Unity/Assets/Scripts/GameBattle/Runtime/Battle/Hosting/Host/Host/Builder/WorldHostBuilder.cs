using System;
using System.Collections.Generic;
using AbilityKit.Ability.Host.Builder.Components;
using AbilityKit.Ability.Host.Framework;
using AbilityKit.Ability.Host.WorldBlueprints;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Ability.World.Management;

namespace AbilityKit.Ability.Host.Builder
{
    /// <summary>
    /// WorldHost 构建器默认实现
    /// </summary>
    public sealed class WorldHostBuilder : IWorldHostBuilder
    {
        private IWorldFactory _worldFactory;
        private IConnectionManager _connectionManager;
        private ITimeDriver _timeDriver;
        private IInputDriver _inputDriver;
        private ISnapshotProvider _snapshotProvider;
        private IWorldBlueprintRegistry _blueprintRegistry;
        private readonly List<IHostRuntimeModule> _modules = new List<IHostRuntimeModule>();

        private WorldHostBuilder()
        {
        }

        public static WorldHostBuilder Create() => new WorldHostBuilder();

        public IWorldHostBuilder SetWorldFactory(IWorldFactory factory)
        {
            _worldFactory = factory ?? throw new ArgumentNullException(nameof(factory));
            return this;
        }

        public IWorldHostBuilder SetConnectionManager(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            return this;
        }

        public IWorldHostBuilder SetTimeDriver(ITimeDriver timeDriver)
        {
            _timeDriver = timeDriver;
            return this;
        }

        public IWorldHostBuilder SetInputDriver(IInputDriver inputDriver)
        {
            _inputDriver = inputDriver;
            return this;
        }

        public IWorldHostBuilder SetSnapshotProvider(ISnapshotProvider snapshotProvider)
        {
            _snapshotProvider = snapshotProvider;
            return this;
        }

        public IWorldHostBuilder SetBlueprintRegistry(IWorldBlueprintRegistry blueprintRegistry)
        {
            _blueprintRegistry = blueprintRegistry;
            return this;
        }

        public IWorldHostBuilder AddModule(IHostRuntimeModule module)
        {
            if (module != null)
            {
                _modules.Add(module);
            }
            return this;
        }

        public IWorldHostBuilder AddModules(IEnumerable<IHostRuntimeModule> modules)
        {
            if (modules != null)
            {
                foreach (var m in modules)
                {
                    if (m != null)
                    {
                        _modules.Add(m);
                    }
                }
            }
            return this;
        }

        public IWorldHost Build()
        {
            var (host, _) = BuildWithOptions();
            return host;
        }

        public (IWorldHost Host, HostRuntimeOptions Options) BuildWithOptions()
        {
            // 1. 创建 WorldManager
            IWorldFactory worldFactory = _worldFactory;

            if (worldFactory == null)
            {
                worldFactory = new DefaultWorldFactory(_blueprintRegistry);
            }
            else if (_blueprintRegistry != null)
            {
                worldFactory = new WorldBlueprintWorldFactory(worldFactory, _blueprintRegistry);
            }

            var worldManager = new WorldManager(worldFactory);

            // 2. 创建 Hooks
            var options = new HostRuntimeOptions();

            // 3. 创建 HostRuntime
            var runtime = new HostRuntime(worldManager, options);

            // 4. 连接 ConnectionManager
            if (_connectionManager != null)
            {
                _connectionManager.Attach(runtime);
            }

            // 5. 安装时间驱动
            if (_timeDriver != null)
            {
                _timeDriver.Attach(runtime, options);
            }

            // 6. 安装输入驱动
            if (_inputDriver != null)
            {
                _inputDriver.Attach(runtime, options);
            }

            // 7. 注册快照提供器
            if (_snapshotProvider != null)
            {
                _snapshotProvider.Register(runtime.Features);
            }

            // 8. 安装模块
            foreach (var module in _modules)
            {
                module.Install(runtime, options);
            }

            return (runtime, options);
        }
    }
}

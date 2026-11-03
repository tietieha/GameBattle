using System;
using AbilityKit.Ability.Host.Extensions.Rollback;
using AbilityKit.Ability.Host.Framework;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Ability.World.Management;

namespace AbilityKit.Game.Battle
{
    public sealed class BattleLogicSessionRuntime : IDisposable
    {
        private readonly IWorldManager _worldManager;
        private readonly HostRuntime _server;

        public BattleLogicSessionRuntime(
            IWorldManager worldManager,
            HostRuntime server,
            ServerRollbackModule rollbackModule)
        {
            _worldManager = worldManager;
            _server = server;
            RollbackModule = rollbackModule;
        }

        public HostRuntime Server => _server;

        public ServerRollbackModule RollbackModule { get; }

        public bool TryGetWorld(WorldId worldId, out IWorld world)
        {
            world = null;
            if (_worldManager == null) return false;
            return _worldManager.TryGet(worldId, out world);
        }

        public void Tick(float deltaTime)
        {
            _server?.Tick(deltaTime);
        }

        public void Dispose()
        {
            _worldManager?.DisposeAll();
        }
    }
}

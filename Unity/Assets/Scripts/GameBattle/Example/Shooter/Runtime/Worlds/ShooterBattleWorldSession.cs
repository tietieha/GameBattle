#nullable enable

using System;
using AbilityKit.Ability.World.Abstractions;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public sealed class ShooterBattleWorldSession : IDisposable
    {
        private readonly ShooterWorldHost _host;
        private bool _disposed;

        private ShooterBattleWorldSession(ShooterWorldHost host, IWorld world, ShooterBattleRuntimePort runtime)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            World = world ?? throw new ArgumentNullException(nameof(world));
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            WorldId = world.Id;
        }

        public WorldId WorldId { get; }

        public IWorld World { get; }

        public ShooterBattleRuntimePort Runtime { get; }

        public static ShooterBattleWorldSession Create(string? worldId = null, ShooterWorldHost? host = null)
        {
            var worldHost = host ?? new ShooterWorldHost();
            var resolvedWorldId = string.IsNullOrWhiteSpace(worldId)
                ? $"shooter-battle-{Guid.NewGuid():N}"
                : worldId!;
            var world = worldHost.CreateBattleWorld(resolvedWorldId);

            if (!world.Services.TryResolve<ShooterBattleRuntimePort>(out var runtime))
            {
                worldHost.DestroyBattleWorld(world.Id.Value);
                throw new InvalidOperationException("Shooter battle world did not register ShooterBattleRuntimePort.");
            }

            return new ShooterBattleWorldSession(worldHost, world, runtime);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _host.DestroyBattleWorld(WorldId.Value);
        }
    }
}

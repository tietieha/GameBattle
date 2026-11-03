using AbilityKit.Ability.World.DI;
using AbilityKit.Ability.World.Services;
using AbilityKit.Ability.World.Services.Attributes;
using AbilityKit.Core.Logging;
using AbilityKit.Demo.Moba.Config.BattleDemo.MO;

namespace AbilityKit.Demo.Moba.Services.Projectile.Launch
{
    public interface IMobaProjectileEmitterManager
    {
        bool TryCreateSequence(ProjectileLauncherMO launcher, out IMobaProjectileLaunchSequence sequence);
    }

    [WorldService(typeof(IMobaProjectileEmitterManager))]
    [WorldService(typeof(MobaProjectileEmitterManager))]
    public sealed class MobaProjectileEmitterManager : IMobaProjectileEmitterManager, IService, IWorldInitializable
    {
        private IMobaProjectileEmitterRegistry _registry;

        public void OnInit(IWorldResolver services)
        {
            _registry = MobaProjectileEmitterRegistry.CreateDefault(GetType().Assembly);
        }

        public bool TryCreateSequence(ProjectileLauncherMO launcher, out IMobaProjectileLaunchSequence sequence)
        {
            sequence = null;
            if (launcher == null) return false;

            EnsureRegistry();
            if (_registry.TryCreate(launcher.EmitterType, out sequence))
            {
                return true;
            }

            Log.Warning($"[MobaProjectileEmitterManager] No projectile emitter sequence registered. launcherId={launcher.Id} emitterType={launcher.EmitterType}");
            return false;
        }

        public void Dispose()
        {
            _registry = null;
        }

        private void EnsureRegistry()
        {
            if (_registry == null)
            {
                _registry = MobaProjectileEmitterRegistry.CreateDefault(GetType().Assembly);
            }
        }
    }
}

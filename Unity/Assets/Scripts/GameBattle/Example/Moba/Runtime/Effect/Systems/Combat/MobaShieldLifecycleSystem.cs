using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.World;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Services;

namespace AbilityKit.Demo.Moba.Systems.Combat
{
    [WorldSystem(order: MobaSystemOrder.ShieldLifecycle, Phase = WorldSystemPhase.PostExecute)]
    public sealed class MobaShieldLifecycleSystem : WorldSystemBase
    {
        private MobaShieldService _shields;
        private IFrameTime _frameTime;

        public MobaShieldLifecycleSystem(global::Entitas.IContexts contexts, IWorldResolver services)
            : base(contexts, services)
        {
        }

        protected override void OnInit()
        {
            Services.TryResolve(out _shields);
            Services.TryResolve(out _frameTime);
        }

        protected override void OnExecute()
        {
            if (_shields == null || _frameTime == null) return;
            _shields.CleanupExpired(_frameTime.Frame.Value);
        }
    }
}

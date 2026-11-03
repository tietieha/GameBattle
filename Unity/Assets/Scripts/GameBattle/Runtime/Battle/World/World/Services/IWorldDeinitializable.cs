using AbilityKit.Ability.World.DI;

namespace AbilityKit.Ability.World.Services
{
    public interface IWorldDeinitializable : IService
    {
        void OnDeinit(IWorldResolver services);
    }
}

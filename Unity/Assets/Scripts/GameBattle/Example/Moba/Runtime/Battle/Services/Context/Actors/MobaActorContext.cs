namespace AbilityKit.Demo.Moba.Services
{
    public interface IMobaActorContextProvider
    {
        bool TryGetSourceActorId(out int actorId);
        bool TryGetTargetActorId(out int actorId);
    }
}

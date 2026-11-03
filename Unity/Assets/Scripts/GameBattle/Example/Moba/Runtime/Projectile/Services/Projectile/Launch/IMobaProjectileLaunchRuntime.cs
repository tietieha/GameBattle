namespace AbilityKit.Demo.Moba.Services.Projectile.Launch
{
    public interface IMobaProjectileLaunchRuntime
    {
        int CurrentFrame { get; }
        long NowMs { get; }
        bool TryGetLauncherEntity(int launcherActorId, out global::ActorEntity launcherEntity);
    }
}

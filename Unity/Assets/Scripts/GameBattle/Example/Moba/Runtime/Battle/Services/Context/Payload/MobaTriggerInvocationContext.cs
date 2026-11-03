namespace AbilityKit.Demo.Moba.Services
{
    public interface IMobaTriggerInvocationContext
    {
        int TriggerId { get; }
        EffectContextKind Kind { get; }
        int SourceActorId { get; }
        int TargetActorId { get; }
        long SourceContextId { get; }
    }
}

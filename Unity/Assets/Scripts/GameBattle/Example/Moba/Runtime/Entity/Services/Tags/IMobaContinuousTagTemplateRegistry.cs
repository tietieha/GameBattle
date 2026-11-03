using AbilityKit.GameplayTags;

namespace AbilityKit.Demo.Moba.Services
{
    public interface IMobaContinuousTagTemplateRegistry
    {
        bool TryGet(int templateId, out ContinuousTagRequirements requirements);
        bool TryGet(string name, out ContinuousTagRequirements requirements);
    }
}

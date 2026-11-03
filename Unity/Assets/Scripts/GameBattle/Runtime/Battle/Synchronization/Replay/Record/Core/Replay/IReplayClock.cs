using AbilityKit.Ability.FrameSync;

namespace AbilityKit.Core.Recording.Core
{
    public interface IReplayClock
    {
        FrameIndex CurrentFrame { get; }

        float Speed { get; set; }

        void Reset(FrameIndex startFrame);

        bool TryConsume(float deltaTime, out FrameIndex nextFrame);
    }
}

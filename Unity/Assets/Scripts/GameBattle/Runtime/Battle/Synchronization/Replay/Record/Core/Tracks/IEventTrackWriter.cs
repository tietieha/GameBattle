using AbilityKit.Ability.FrameSync;

namespace AbilityKit.Core.Recording.Core
{
    public interface IEventTrackWriter
    {
        void Append(FrameIndex frame, RecordEventType eventType, byte[] payload);
    }
}

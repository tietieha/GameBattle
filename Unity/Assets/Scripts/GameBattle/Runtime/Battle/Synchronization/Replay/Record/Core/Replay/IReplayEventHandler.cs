namespace AbilityKit.Core.Recording.Core
{
    public interface IReplayEventHandler
    {
        void Handle(in RecordEvent e);
    }
}

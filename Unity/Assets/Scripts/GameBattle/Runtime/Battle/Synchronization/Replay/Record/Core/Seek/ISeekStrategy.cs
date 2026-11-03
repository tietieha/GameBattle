namespace AbilityKit.Core.Recording.Core
{
    public interface ISeekStrategy
    {
        bool TrySeek(in SeekRequest req);
    }
}

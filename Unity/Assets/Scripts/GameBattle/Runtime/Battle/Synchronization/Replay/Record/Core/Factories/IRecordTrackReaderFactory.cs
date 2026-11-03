namespace AbilityKit.Core.Recording.Core
{
    public interface IRecordTrackReaderFactory
    {
        bool TryCreateReader(RecordContainer container, RecordTrackId trackId, out IEventTrackReader reader);
    }
}

namespace AbilityKit.Core.Recording.Core
{
    public interface IRecordContainerSerializer
    {
        byte[] Serialize(RecordContainer container);

        RecordContainer Deserialize(byte[] data);
    }
}

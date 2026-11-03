using AbilityKit.Ability.Host;

namespace AbilityKit.Core.Recording.FrameRecord
{
    public interface IFrameRecordCodec
    {
        IFrameRecordWriter CreateWriter(string outputPath, FrameRecordMeta meta);

        FrameRecordFile Load(string path);
    }
}

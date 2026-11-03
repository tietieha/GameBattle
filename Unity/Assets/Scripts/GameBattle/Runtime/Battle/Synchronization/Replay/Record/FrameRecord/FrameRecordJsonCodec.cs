namespace AbilityKit.Core.Recording.FrameRecord
{
    public sealed class FrameRecordJsonCodec : IFrameRecordCodec
    {
        public IFrameRecordWriter CreateWriter(string outputPath, FrameRecordMeta meta)
        {
            return new FrameRecordJsonWriter(outputPath, meta);
        }

        public FrameRecordFile Load(string path)
        {
            return FrameRecordJsonReader.Load(path);
        }
    }
}

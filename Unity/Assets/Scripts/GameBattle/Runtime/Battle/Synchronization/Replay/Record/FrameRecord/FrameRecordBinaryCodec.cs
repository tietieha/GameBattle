namespace AbilityKit.Core.Recording.FrameRecord
{
    public sealed class FrameRecordBinaryCodec : IFrameRecordCodec
    {
        public IFrameRecordWriter CreateWriter(string outputPath, FrameRecordMeta meta)
        {
            return new FrameRecordBinaryWriter(outputPath, meta);
        }

        public FrameRecordFile Load(string path)
        {
            return FrameRecordBinaryReader.Load(path);
        }
    }
}

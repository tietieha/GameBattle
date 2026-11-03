using AbilityKit.Core.Recording.FrameRecord;

namespace AbilityKit.Record.MemoryPack
{
    public static class FrameRecordMemoryPackCodecInstaller
    {
        public static void InstallAsCurrent()
        {
            FrameRecordCodecs.Current = new FrameRecordMemoryPackCodec();
        }
    }
}

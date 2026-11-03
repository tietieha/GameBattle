using System;
using System.IO;

namespace AbilityKit.Core.Recording.FrameRecord
{
    public static class FrameRecordCodecs
    {
        private static IFrameRecordCodec s_current;

        public static IFrameRecordCodec Current
        {
            get
            {
                if (s_current != null) return s_current;
                s_current = new DefaultFrameRecordCodec();
                return s_current;
            }
            set
            {
                s_current = value;
            }
        }

        private sealed class DefaultFrameRecordCodec : IFrameRecordCodec
        {
            private readonly FrameRecordOptimizedBinaryCodec _bin = new FrameRecordOptimizedBinaryCodec();
            private readonly FrameRecordJsonCodec _json = new FrameRecordJsonCodec();

            public IFrameRecordWriter CreateWriter(string outputPath, FrameRecordMeta meta)
            {
                var ext = Path.GetExtension(outputPath);
                if (string.Equals(ext, ".bin", StringComparison.OrdinalIgnoreCase))
                {
                    return _bin.CreateWriter(outputPath, meta);
                }

                return _json.CreateWriter(outputPath, meta);
            }

            public FrameRecordFile Load(string path)
            {
                var ext = Path.GetExtension(path);
                if (string.Equals(ext, ".bin", StringComparison.OrdinalIgnoreCase))
                {
                    return _bin.Load(path);
                }

                return _json.Load(path);
            }
        }
    }
}

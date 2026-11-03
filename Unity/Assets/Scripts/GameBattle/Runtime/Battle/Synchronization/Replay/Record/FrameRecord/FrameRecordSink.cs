using System;
using AbilityKit.Ability.Host;
using AbilityKit.Core.Recording.Core;

namespace AbilityKit.Core.Recording.FrameRecord
{
    public sealed class FrameRecordSink : IFrameRecordSink
    {
        private readonly IFrameRecordWriter _writer;

        public FrameRecordSink(IFrameRecordWriter writer)
        {
            _writer = writer;
        }

        public void AppendInput(in PlayerInputCommand cmd)
        {
            _writer?.Append(in cmd);
        }

        public void AppendStateHash(int frame, int version, uint hash)
        {
            _writer?.AppendStateHash(frame, version, hash);
        }

        public void AppendSnapshot(int frame, int opCode, byte[] payload)
        {
            _writer?.AppendSnapshot(frame, opCode, payload);
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}

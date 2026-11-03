using System;
using AbilityKit.Ability.Host;

namespace AbilityKit.Core.Recording.FrameRecord
{
    public interface IFrameRecordWriter : IDisposable
    {
        void Append(in PlayerInputCommand cmd);

        void AppendStateHash(int frame, int version, uint hash);

        void AppendSnapshot(int frame, int opCode, byte[] payload);
    }
}

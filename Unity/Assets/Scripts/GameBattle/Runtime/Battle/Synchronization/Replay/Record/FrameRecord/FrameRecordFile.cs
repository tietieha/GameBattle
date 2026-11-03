using System;
using System.Collections.Generic;

namespace AbilityKit.Core.Recording.FrameRecord
{
    [Serializable]
    public sealed class FrameRecordFile
    {
        public FrameRecordMeta Meta;
        public List<FrameRecordInputFrame> Inputs;
        public List<FrameRecordStateHashFrame> StateHashes;
        public List<FrameRecordSnapshotFrame> Snapshots;
        public List<FrameRecordChunkIndex> Index;
    }

    [Serializable]
    public sealed class FrameRecordMeta
    {
        public string WorldId;
        public string WorldType;
        public int TickRate;
        public int RandomSeed;
        public string PlayerId;
        public long StartedAtUnixMs;
    }

    [Serializable]
    public sealed class FrameRecordInputFrame
    {
        public int Frame;
        public string PlayerId;
        public int OpCode;
        public string PayloadBase64;
    }

    [Serializable]
    public sealed class FrameRecordStateHashFrame
    {
        public int Frame;
        public int Version;
        public uint Hash;
    }

    [Serializable]
    public sealed class FrameRecordSnapshotFrame
    {
        public int Frame;
        public int OpCode;
        public string PayloadBase64;
    }

    [Serializable]
    public sealed class FrameRecordChunkIndex
    {
        public int StartFrame;
        public int EndFrame;

        public int InputStart;
        public int InputEnd;

        public int StateHashStart;
        public int StateHashEnd;

        public int SnapshotStart;
        public int SnapshotEnd;
    }
}

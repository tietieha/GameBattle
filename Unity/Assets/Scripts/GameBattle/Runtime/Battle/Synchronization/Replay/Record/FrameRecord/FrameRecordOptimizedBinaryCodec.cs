using System;
using System.Collections.Generic;

namespace AbilityKit.Core.Recording.FrameRecord
{
    public sealed class FrameRecordOptimizedBinaryCodec : IFrameRecordCodec
    {
        public IFrameRecordWriter CreateWriter(string outputPath, FrameRecordMeta meta)
        {
            return new FrameRecordOptimizedBinaryWriter(outputPath, meta);
        }

        public FrameRecordFile Load(string path)
        {
            return ToRecordFile(FrameRecordOptimizedBinaryReader.Load(path));
        }

        private static FrameRecordFile ToRecordFile(FrameRecordBinaryData data)
        {
            var inputs = new List<FrameRecordInputFrame>(data.InputCount);
            for (int i = 0; i < data.InputCount; i++)
            {
                var payload = data.InputPayloads[i];
                inputs.Add(new FrameRecordInputFrame
                {
                    Frame = data.InputFrames[i],
                    PlayerId = data.InputPlayers[i] ?? string.Empty,
                    OpCode = data.InputOpCodes[i],
                    PayloadBase64 = payload != null && payload.Length > 0 ? Convert.ToBase64String(payload) : string.Empty,
                });
            }

            var stateHashes = new List<FrameRecordStateHashFrame>(data.StateHashCount);
            for (int i = 0; i < data.StateHashCount; i++)
            {
                stateHashes.Add(new FrameRecordStateHashFrame
                {
                    Frame = data.StateHashFrames[i],
                    Version = data.StateHashVersions[i],
                    Hash = data.StateHashValues[i],
                });
            }

            var snapshots = new List<FrameRecordSnapshotFrame>(data.SnapshotCount);
            for (int i = 0; i < data.SnapshotCount; i++)
            {
                var payload = data.SnapshotPayloads[i];
                snapshots.Add(new FrameRecordSnapshotFrame
                {
                    Frame = data.SnapshotFrames[i],
                    OpCode = data.SnapshotOpCodes[i],
                    PayloadBase64 = payload != null && payload.Length > 0 ? Convert.ToBase64String(payload) : string.Empty,
                });
            }

            return new FrameRecordFile
            {
                Meta = data.Meta,
                Inputs = inputs,
                StateHashes = stateHashes,
                Snapshots = snapshots,
                Index = null,
            };
        }
    }
}

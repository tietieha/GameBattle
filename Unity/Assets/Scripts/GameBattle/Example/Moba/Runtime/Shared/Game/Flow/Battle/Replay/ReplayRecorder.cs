using System;
using System.Collections.Generic;
using System.IO;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 回放录制器实现
    /// 负责录制战斗回放数据
    /// </summary>
    public sealed class ReplayRecorder : IReplayRecorder
    {
        private string _replayId;
        private bool _isRecording;
        private int _startFrame;
        private DateTime _startTime;

        private readonly List<ReplaySnapshotEntry> _snapshots = new List<ReplaySnapshotEntry>();
        private readonly List<ReplayInputEntry> _inputs = new List<ReplayInputEntry>();
        private readonly ReplayHeader _header = new ReplayHeader();

        private bool _isDisposed;

        public ReplayRecorder()
        {
        }

        /// <summary>
        /// 是否正在录制
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// 获取已录制的快照数量
        /// </summary>
        public int SnapshotCount => _snapshots.Count;

        /// <summary>
        /// 获取已录制的输入数量
        /// </summary>
        public int InputCount => _inputs.Count;

        /// <summary>
        /// 开始录制
        /// </summary>
        public void StartRecording(string replayId)
        {
            if (_isRecording || _isDisposed) return;

            _replayId = replayId ?? Guid.NewGuid().ToString("N");
            _isRecording = true;
            _startFrame = 0;
            _startTime = DateTime.UtcNow;

            _snapshots.Clear();
            _inputs.Clear();

            _header.ReplayId = _replayId;
            _header.StartTime = _startTime.Ticks;
            _header.Version = ReplayConstants.Version;
        }

        /// <summary>
        /// 记录帧快照
        /// </summary>
        public void RecordSnapshot(int frameIndex, byte[] snapshotData)
        {
            if (!_isRecording || _isDisposed) return;
            if (snapshotData == null || snapshotData.Length == 0) return;

            if (_snapshots.Count == 0)
            {
                _startFrame = frameIndex;
                _header.StartFrame = frameIndex;
            }

            _snapshots.Add(new ReplaySnapshotEntry(
                frameIndex - _startFrame,
                snapshotData.Length,
                snapshotData));
        }

        /// <summary>
        /// 记录玩家输入
        /// </summary>
        public void RecordInput(int frameIndex, int playerId, byte[] inputData)
        {
            if (!_isRecording || _isDisposed) return;
            if (inputData == null || inputData.Length == 0) return;

            _inputs.Add(new ReplayInputEntry(
                frameIndex - _startFrame,
                playerId,
                inputData.Length,
                inputData));
        }

        /// <summary>
        /// 停止录制并返回回放数据
        /// </summary>
        public byte[] StopRecordingAndGetData()
        {
            if (!_isRecording || _isDisposed) return null;

            _isRecording = false;
            _header.EndTime = DateTime.UtcNow.Ticks;
            _header.TotalFrames = _snapshots.Count > 0
                ? _snapshots[_snapshots.Count - 1].RelativeFrame
                : 0;
            _header.SnapshotCount = _snapshots.Count;
            _header.InputCount = _inputs.Count;

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                _header.WriteTo(writer);

                foreach (var snapshot in _snapshots)
                {
                    snapshot.WriteTo(writer);
                }

                foreach (var input in _inputs)
                {
                    input.WriteTo(writer);
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public void StopRecording()
        {
            if (!_isRecording) return;
            _isRecording = false;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _isRecording = false;
            _snapshots.Clear();
            _inputs.Clear();
        }
    }

    /// <summary>
    /// 回放常量
    /// </summary>
    public static class ReplayConstants
    {
        public const string Version = "1.0.0";
        public const int MagicNumber = 0x5245504C; // "REPL"
    }

    /// <summary>
    /// 回放文件头
    /// </summary>
    public sealed class ReplayHeader
    {
        public string ReplayId { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public string Version { get; set; } = ReplayConstants.Version;
        public int StartFrame { get; set; }
        public int TotalFrames { get; set; }
        public int SnapshotCount { get; set; }
        public int InputCount { get; set; }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(ReplayConstants.MagicNumber);
            writer.Write(ReplayConstants.Version);
            writer.Write(StartTime);
            writer.Write(EndTime);
            writer.Write(StartFrame);
            writer.Write(TotalFrames);
            writer.Write(SnapshotCount);
            writer.Write(InputCount);
            writer.Write(ReplayId ?? string.Empty);
        }

        public bool ReadFrom(BinaryReader reader)
        {
            var magic = reader.ReadInt32();
            if (magic != ReplayConstants.MagicNumber)
            {
                return false;
            }

            Version = reader.ReadString();
            StartTime = reader.ReadInt64();
            EndTime = reader.ReadInt64();
            StartFrame = reader.ReadInt32();
            TotalFrames = reader.ReadInt32();
            SnapshotCount = reader.ReadInt32();
            InputCount = reader.ReadInt32();
            ReplayId = reader.ReadString();

            return true;
        }
    }

    /// <summary>
    /// 回放快照条目
    /// </summary>
    public struct ReplaySnapshotEntry
    {
        public int RelativeFrame { get; }
        public int DataLength { get; }
        public byte[] Data { get; }

        public ReplaySnapshotEntry(int relativeFrame, int dataLength, byte[] data)
        {
            RelativeFrame = relativeFrame;
            DataLength = dataLength;
            Data = data;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(RelativeFrame);
            writer.Write(DataLength);
            writer.Write(Data);
        }

        public static ReplaySnapshotEntry ReadFrom(BinaryReader reader)
        {
            var frame = reader.ReadInt32();
            var length = reader.ReadInt32();
            var data = reader.ReadBytes(length);
            return new ReplaySnapshotEntry(frame, length, data);
        }
    }

    /// <summary>
    /// 回放输入条目
    /// </summary>
    public struct ReplayInputEntry
    {
        public int RelativeFrame { get; }
        public int PlayerId { get; }
        public int DataLength { get; }
        public byte[] Data { get; }

        public ReplayInputEntry(int relativeFrame, int playerId, int dataLength, byte[] data)
        {
            RelativeFrame = relativeFrame;
            PlayerId = playerId;
            DataLength = dataLength;
            Data = data;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(RelativeFrame);
            writer.Write(PlayerId);
            writer.Write(DataLength);
            writer.Write(Data);
        }

        public static ReplayInputEntry ReadFrom(BinaryReader reader)
        {
            var frame = reader.ReadInt32();
            var playerId = reader.ReadInt32();
            var length = reader.ReadInt32();
            var data = reader.ReadBytes(length);
            return new ReplayInputEntry(frame, playerId, length, data);
        }
    }
}

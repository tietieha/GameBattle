using System;
using System.Collections.Generic;
using System.IO;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 回放播放器实现
    /// 负责回放战斗录制数据
    /// </summary>
    public sealed class ReplayPlayer : IReplayPlayer
    {
        private ReplayHeader _header;
        private readonly List<ReplaySnapshotEntry> _snapshots = new List<ReplaySnapshotEntry>();
        private readonly List<ReplayInputEntry> _inputs = new List<ReplayInputEntry>();

        private int _currentIndex;
        private float _playbackSpeed = 1.0f;
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isDisposed;

        private int _startFrame;

        /// <summary>
        /// 当前播放帧
        /// </summary>
        public int CurrentFrame => _header?.StartFrame + _currentRelativeFrame ?? 0;

        /// <summary>
        /// 总帧数
        /// </summary>
        public int TotalFrames => _header?.TotalFrames ?? 0;

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying => _isPlaying && !_isPaused;

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary>
        /// 播放速度
        /// </summary>
        public float PlaybackSpeed => _playbackSpeed;

        /// <summary>
        /// 回放元数据
        /// </summary>
        public ReplayMetadata Metadata => new ReplayMetadata
        {
            ReplayId = _header?.ReplayId,
            Version = _header?.Version,
            StartTime = _header != null ? new DateTime(_header.StartTime) : default,
            EndTime = _header != null ? new DateTime(_header.EndTime) : default,
            StartFrame = _header?.StartFrame ?? 0,
            TotalFrames = _header?.TotalFrames ?? 0
        };

        private int _currentRelativeFrame => _currentIndex < _snapshots.Count
            ? _snapshots[_currentIndex].RelativeFrame
            : TotalFrames;

        /// <summary>
        /// 回放元数据
        /// </summary>
        public struct ReplayMetadata
        {
            public string ReplayId { get; set; }
            public string Version { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public int StartFrame { get; set; }
            public int TotalFrames { get; set; }
        }

        public ReplayPlayer()
        {
        }

        /// <summary>
        /// 加载回放数据
        /// </summary>
        public void LoadReplay(byte[] replayData)
        {
            if (_isDisposed) return;
            if (replayData == null || replayData.Length == 0) return;

            Stop();
            _snapshots.Clear();
            _inputs.Clear();

            using (var stream = new MemoryStream(replayData))
            using (var reader = new BinaryReader(stream))
            {
                _header = new ReplayHeader();
                if (!_header.ReadFrom(reader))
                {
                    throw new InvalidDataException("Invalid replay file format");
                }

                for (int i = 0; i < _header.SnapshotCount; i++)
                {
                    _snapshots.Add(ReplaySnapshotEntry.ReadFrom(reader));
                }

                for (int i = 0; i < _header.InputCount; i++)
                {
                    _inputs.Add(ReplayInputEntry.ReadFrom(reader));
                }
            }

            _startFrame = _header.StartFrame;
            _currentIndex = 0;
            _isPlaying = false;
            _isPaused = false;
        }

        /// <summary>
        /// 开始播放
        /// </summary>
        public void Play()
        {
            if (_header == null || _isDisposed) return;

            if (_currentIndex >= _snapshots.Count)
            {
                _currentIndex = 0;
            }

            _isPlaying = true;
            _isPaused = false;
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            _isPaused = false;
            _currentIndex = 0;
        }

        /// <summary>
        /// 定位到指定帧
        /// </summary>
        public void SeekToFrame(int frameIndex)
        {
            if (_header == null || _isDisposed) return;

            int relativeFrame = frameIndex - _startFrame;
            if (relativeFrame < 0) relativeFrame = 0;
            if (relativeFrame >= TotalFrames) relativeFrame = TotalFrames - 1;

            for (int i = 0; i < _snapshots.Count; i++)
            {
                if (_snapshots[i].RelativeFrame >= relativeFrame)
                {
                    _currentIndex = i;
                    return;
                }
            }

            _currentIndex = _snapshots.Count - 1;
        }

        /// <summary>
        /// 设置播放速度
        /// </summary>
        public void SetPlaybackSpeed(float speed)
        {
            _playbackSpeed = Math.Max(0.1f, Math.Min(10.0f, speed));
        }

        /// <summary>
        /// 获取当前帧的快照数据
        /// </summary>
        public bool TryGetCurrentSnapshot(out int frameIndex, out byte[] snapshotData)
        {
            frameIndex = 0;
            snapshotData = null;

            if (_currentIndex < 0 || _currentIndex >= _snapshots.Count)
            {
                return false;
            }

            var entry = _snapshots[_currentIndex];
            frameIndex = _startFrame + entry.RelativeFrame;
            snapshotData = entry.Data;
            return true;
        }

        /// <summary>
        /// 获取指定帧的快照数据
        /// </summary>
        public bool TryGetSnapshotAt(int frameIndex, out byte[] snapshotData)
        {
            snapshotData = null;

            if (_header == null) return false;

            int relativeFrame = frameIndex - _startFrame;
            if (relativeFrame < 0) return false;

            foreach (var snapshot in _snapshots)
            {
                if (snapshot.RelativeFrame == relativeFrame)
                {
                    snapshotData = snapshot.Data;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取当前帧及之前的输入数据
        /// </summary>
        public IReadOnlyList<ReplayInputEntry> GetInputsUntilFrame(int frameIndex)
        {
            var result = new List<ReplayInputEntry>();

            if (_header == null) return result;

            int relativeFrame = frameIndex - _startFrame;
            if (relativeFrame < 0) return result;

            foreach (var input in _inputs)
            {
                if (input.RelativeFrame <= relativeFrame)
                {
                    result.Add(input);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 前进到下一帧
        /// </summary>
        public bool AdvanceFrame()
        {
            if (_currentIndex < 0 || _currentIndex >= _snapshots.Count)
            {
                return false;
            }

            _currentIndex++;
            return _currentIndex < _snapshots.Count;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Stop();
            _snapshots.Clear();
            _inputs.Clear();
            _header = null;
        }
    }
}

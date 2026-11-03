using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 输入缓冲实现
    /// 管理玩家输入的缓冲和调度
    /// </summary>
    public sealed class InputBuffer : IInputBuffer
    {
        private readonly Dictionary<int, SortedList<int, PlayerInputData>> _buffers = new Dictionary<int, SortedList<int, PlayerInputData>>();
        private readonly object _lock = new object();
        private bool _isDisposed;

        /// <summary>
        /// 添加输入到缓冲
        /// </summary>
        public void Enqueue(int playerId, in PlayerInputData input)
        {
            if (_isDisposed) return;

            lock (_lock)
            {
                if (!_buffers.TryGetValue(playerId, out var buffer))
                {
                    buffer = new SortedList<int, PlayerInputData>();
                    _buffers[playerId] = buffer;
                }

                // 如果同一帧已有输入，覆盖它
                if (buffer.ContainsKey(input.FrameIndex))
                {
                    buffer[input.FrameIndex] = input;
                }
                else
                {
                    buffer.Add(input.FrameIndex, input);
                }
            }
        }

        /// <summary>
        /// 获取指定帧的输入
        /// </summary>
        public bool TryDequeue(int playerId, int frameIndex, out PlayerInputData input)
        {
            input = default;
            if (_isDisposed) return false;

            lock (_lock)
            {
                if (!_buffers.TryGetValue(playerId, out var buffer))
                {
                    return false;
                }

                if (buffer.TryGetValue(frameIndex, out input))
                {
                    buffer.Remove(frameIndex);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 获取所有待处理输入
        /// </summary>
        public IReadOnlyList<PlayerInputData> GetPendingInputs(int playerId)
        {
            if (_isDisposed) return Array.Empty<PlayerInputData>();

            lock (_lock)
            {
                if (!_buffers.TryGetValue(playerId, out var buffer))
                {
                    return Array.Empty<PlayerInputData>();
                }

                var result = new List<PlayerInputData>(buffer.Count);
                foreach (var kvp in buffer)
                {
                    result.Add(kvp.Value);
                }
                return result;
            }
        }

        /// <summary>
        /// 清空指定玩家的缓冲
        /// </summary>
        public void Clear(int playerId)
        {
            if (_isDisposed) return;

            lock (_lock)
            {
                if (_buffers.TryGetValue(playerId, out var buffer))
                {
                    buffer.Clear();
                }
            }
        }

        /// <summary>
        /// 清空所有缓冲
        /// </summary>
        public void ClearAll()
        {
            if (_isDisposed) return;

            lock (_lock)
            {
                foreach (var buffer in _buffers.Values)
                {
                    buffer.Clear();
                }
                _buffers.Clear();
            }
        }

        /// <summary>
        /// 获取最早一帧的输入帧索引
        /// </summary>
        public int GetEarliestFrame(int playerId)
        {
            if (_isDisposed) return -1;

            lock (_lock)
            {
                if (!_buffers.TryGetValue(playerId, out var buffer) || buffer.Count == 0)
                {
                    return -1;
                }

                return buffer.Keys[0];
            }
        }

        /// <summary>
        /// 获取最新一帧的输入帧索引
        /// </summary>
        public int GetLatestFrame(int playerId)
        {
            if (_isDisposed) return -1;

            lock (_lock)
            {
                if (!_buffers.TryGetValue(playerId, out var buffer) || buffer.Count == 0)
                {
                    return -1;
                }

                return buffer.Keys[buffer.Count - 1];
            }
        }

        /// <summary>
        /// 获取缓冲中的输入数量
        /// </summary>
        public int GetBufferCount(int playerId)
        {
            if (_isDisposed) return 0;

            lock (_lock)
            {
                if (!_buffers.TryGetValue(playerId, out var buffer))
                {
                    return 0;
                }
                return buffer.Count;
            }
        }

        /// <summary>
        /// 获取所有玩家 ID
        /// </summary>
        public IReadOnlyList<int> GetAllPlayerIds()
        {
            lock (_lock)
            {
                return new List<int>(_buffers.Keys);
            }
        }

        /// <summary>
        /// 是否包含指定玩家的缓冲
        /// </summary>
        public bool HasPlayerBuffer(int playerId)
        {
            lock (_lock)
            {
                return _buffers.ContainsKey(playerId);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            ClearAll();
        }
    }

    /// <summary>
    /// 输入调度器
    /// 负责从缓冲中取出输入并提交
    /// </summary>
    public sealed class InputScheduler
    {
        private readonly InputBuffer _buffer;
        private readonly IPlayerInputSubmitter _submitter;
        private readonly int _localPlayerId;
        private int _lastConsumedFrame;
        private bool _isDisposed;

        public InputScheduler(InputBuffer buffer, IPlayerInputSubmitter submitter, int localPlayerId)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _submitter = submitter ?? throw new ArgumentNullException(nameof(submitter));
            _localPlayerId = localPlayerId;
            _lastConsumedFrame = -1;
        }

        /// <summary>
        /// 调度到指定帧
        /// </summary>
        /// <param name="targetFrame">目标帧</param>
        /// <returns>实际调度的帧数</returns>
        public int ScheduleTo(int targetFrame)
        {
            if (_isDisposed) return 0;
            if (targetFrame <= _lastConsumedFrame) return 0;

            int scheduled = 0;
            var earliestFrame = _buffer.GetEarliestFrame(_localPlayerId);

            if (earliestFrame < 0 || earliestFrame > targetFrame)
            {
                return 0;
            }

            for (int frame = Math.Max(_lastConsumedFrame + 1, earliestFrame); frame <= targetFrame; frame++)
            {
                if (_buffer.TryDequeue(_localPlayerId, frame, out var input))
                {
                    _submitter.SubmitInput(_localPlayerId, in input);
                    _lastConsumedFrame = frame;
                    scheduled++;
                }
            }

            return scheduled;
        }

        /// <summary>
        /// 获取最后消费的帧
        /// </summary>
        public int LastConsumedFrame => _lastConsumedFrame;

        /// <summary>
        /// 重置调度器
        /// </summary>
        public void Reset()
        {
            _lastConsumedFrame = -1;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
        }
    }
}

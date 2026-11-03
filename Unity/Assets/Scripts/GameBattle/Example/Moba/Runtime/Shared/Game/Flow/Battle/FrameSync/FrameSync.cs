using System;
using System.Threading;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 帧同步实现
    /// 负责帧时间管理和逻辑推进
    /// </summary>
    public sealed class FrameSync : IFrameSync, IFrameTimeProvider, IFrameSyncController
    {
        private int _currentFrame;
        private int _lastFrame;
        private float _fixedDeltaSeconds = 1f / 30f;
        private float _frameRate = 30f;
        private double _logicTimeSeconds;
        private float _tickAccumulator;
        private float _timeScale = 1.0f;
        private bool _isFirstFrameReceived;
        private bool _isPaused;
        private bool _isDisposed;

        private int _targetFrame;
        private bool _isCatchingUp;
        private int _frameDelay = 3;

        private readonly object _lock = new object();

        public FrameSync()
        {
            _currentFrame = 0;
            _lastFrame = -1;
            _targetFrame = 0;
        }

        #region IFrameTimeProvider

        public int CurrentFrame
        {
            get { lock (_lock) { return _currentFrame; } }
        }

        public int LastFrame
        {
            get { lock (_lock) { return _lastFrame; } }
        }

        public float FixedDeltaSeconds
        {
            get { lock (_lock) { return _fixedDeltaSeconds; } }
        }

        public float FrameRate
        {
            get { lock (_lock) { return _frameRate; } }
        }

        public double LogicTimeSeconds
        {
            get { lock (_lock) { return _logicTimeSeconds; } }
        }

        public float TickAccumulator
        {
            get { lock (_lock) { return _tickAccumulator; } }
        }

        public bool IsFirstFrameReceived
        {
            get { lock (_lock) { return _isFirstFrameReceived; } }
        }

        public float TimeScale
        {
            get { lock (_lock) { return _timeScale; } }
            set
            {
                lock (_lock)
                {
                    _timeScale = Math.Max(0f, Math.Min(10f, value));
                }
            }
        }

        #endregion

        #region IFrameSyncController

        public bool IsPaused
        {
            get { lock (_lock) { return _isPaused; } }
        }

        public int TargetFrame
        {
            get { lock (_lock) { return _targetFrame; } }
        }

        public bool IsCatchingUp
        {
            get { lock (_lock) { return _isCatchingUp; } }
        }

        public int FrameDelay
        {
            get { lock (_lock) { return _frameDelay; } }
            set
            {
                lock (_lock)
                {
                    _frameDelay = Math.Max(0, Math.Min(30, value));
                }
            }
        }

        public void Pause()
        {
            lock (_lock)
            {
                _isPaused = true;
            }
        }

        public void Resume()
        {
            lock (_lock)
            {
                _isPaused = false;
            }
        }

        public void AdvanceToFrame(int targetFrame)
        {
            lock (_lock)
            {
                _targetFrame = targetFrame;
            }
        }

        public void SetFrameRate(int framesPerSecond)
        {
            lock (_lock)
            {
                if (framesPerSecond <= 0) framesPerSecond = 30;
                _frameRate = framesPerSecond;
                _fixedDeltaSeconds = 1f / _frameRate;
            }
        }

        #endregion

        #region IFrameSync

        /// <summary>
        /// 设置固定时间步长
        /// </summary>
        public void SetFixedDeltaSeconds(float seconds)
        {
            lock (_lock)
            {
                if (seconds <= 0) seconds = 1f / 30f;
                _fixedDeltaSeconds = seconds;
                _frameRate = 1f / seconds;
            }
        }

        /// <summary>
        /// 标记首帧已接收
        /// </summary>
        public void MarkFirstFrameReceived()
        {
            lock (_lock)
            {
                _isFirstFrameReceived = true;
            }
        }

        /// <summary>
        /// 重置帧同步状态
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _currentFrame = 0;
                _lastFrame = -1;
                _logicTimeSeconds = 0;
                _tickAccumulator = 0;
                _isFirstFrameReceived = false;
                _isPaused = false;
                _isCatchingUp = false;
                _targetFrame = 0;
            }
        }

        /// <summary>
        /// 处理接收到的帧
        /// </summary>
        public void OnReceiveFrame(int frameIndex)
        {
            lock (_lock)
            {
                if (frameIndex > _currentFrame)
                {
                    _currentFrame = frameIndex;
                    _isFirstFrameReceived = true;
                }
            }
        }

        /// <summary>
        /// 尝试推进逻辑帧
        /// </summary>
        /// <param name="deltaTime">实际流逝的时间</param>
        /// <returns>推进了多少帧</returns>
        public int TryAdvanceFrame(float deltaTime)
        {
            lock (_lock)
            {
                if (_isPaused) return 0;

                deltaTime *= _timeScale;
                _tickAccumulator += deltaTime;
                _logicTimeSeconds += deltaTime;

                int framesAdvanced = 0;
                while (_tickAccumulator >= _fixedDeltaSeconds)
                {
                    _tickAccumulator -= _fixedDeltaSeconds;
                    _lastFrame = _currentFrame;
                    _currentFrame++;
                    framesAdvanced++;
                }

                if (_currentFrame < _targetFrame)
                {
                    _isCatchingUp = true;
                }
                else
                {
                    _isCatchingUp = false;
                }

                return framesAdvanced;
            }
        }

        /// <summary>
        /// 等待指定帧
        /// </summary>
        public bool WaitForFrame(int targetFrame, int timeoutMs = 5000)
        {
            int startTime = Environment.TickCount;
            while (Environment.TickCount - startTime < timeoutMs)
            {
                lock (_lock)
                {
                    if (_currentFrame >= targetFrame)
                    {
                        return true;
                    }
                }
                Thread.Sleep(1);
            }
            return false;
        }

        /// <summary>
        /// 获取帧延迟（当前帧与目标帧的差距）
        /// </summary>
        public int GetFrameLag()
        {
            lock (_lock)
            {
                return Math.Max(0, _currentFrame - _targetFrame);
            }
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        public FrameSyncState GetState()
        {
            lock (_lock)
            {
                if (_isPaused) return FrameSyncState.Paused;
                if (_isCatchingUp) return FrameSyncState.CatchingUp;
                if (_isFirstFrameReceived) return FrameSyncState.Running;
                return FrameSyncState.Idle;
            }
        }

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Reset();
        }
    }

    /// <summary>
    /// 帧同步接口
    /// 定义帧同步的核心行为
    /// </summary>
    public interface IFrameSync : IDisposable
    {
        /// <summary>
        /// 设置固定时间步长
        /// </summary>
        void SetFixedDeltaSeconds(float seconds);

        /// <summary>
        /// 标记首帧已接收
        /// </summary>
        void MarkFirstFrameReceived();

        /// <summary>
        /// 重置帧同步状态
        /// </summary>
        void Reset();

        /// <summary>
        /// 处理接收到的帧
        /// </summary>
        void OnReceiveFrame(int frameIndex);

        /// <summary>
        /// 尝试推进逻辑帧
        /// </summary>
        int TryAdvanceFrame(float deltaTime);

        /// <summary>
        /// 等待指定帧
        /// </summary>
        bool WaitForFrame(int targetFrame, int timeoutMs = 5000);

        /// <summary>
        /// 获取帧延迟
        /// </summary>
        int GetFrameLag();

        /// <summary>
        /// 获取状态
        /// </summary>
        FrameSyncState GetState();
    }
}

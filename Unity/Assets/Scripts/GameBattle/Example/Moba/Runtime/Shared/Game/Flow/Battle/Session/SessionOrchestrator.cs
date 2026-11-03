using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 会话编排器实现
    /// 管理战斗会话的生命周期
    /// 参考 view.runtime 的 SessionOrchestrator 实现
    /// </summary>
    public sealed class SessionOrchestrator : ISessionOrchestrator
    {
        private BattleStartPlan _plan;
        private SessionState _state = SessionState.Idle;
        private int _currentFrame;
        private float _tickAcc;
        private bool _isFirstFrameReceived;

        private ISessionOrchestratorHost _host;
        private bool _isDisposed;

        /// <summary>
        /// 获取当前会话状态
        /// </summary>
        public SessionState State => _state;

        /// <summary>
        /// 获取当前帧索引
        /// </summary>
        public int CurrentFrame => _currentFrame;

        /// <summary>
        /// 获取启动计划
        /// </summary>
        public BattleStartPlan Plan => _plan;

        /// <summary>
        /// 初始化编排器
        /// </summary>
        public void Initialize(in BattleStartPlan plan)
        {
            if (_isDisposed) return;

            _plan = plan;
            _state = SessionState.Idle;
            _currentFrame = 0;
            _tickAcc = 0f;
            _isFirstFrameReceived = false;
        }

        /// <summary>
        /// 设置宿主
        /// </summary>
        public void SetHost(ISessionOrchestratorHost host)
        {
            _host = host;
        }

        /// <summary>
        /// 启动会话
        /// </summary>
        public void StartSession()
        {
            if (_isDisposed) return;
            if (_state == SessionState.Running) return;

            StopSession();

            _state = SessionState.Initializing;

            if (_host != null)
            {
                _host.InvokeSessionStartingPipeline();
            }

            _currentFrame = 0;
            _tickAcc = 0f;
            _isFirstFrameReceived = false;

            _state = SessionState.Running;
        }

        /// <summary>
        /// 停止会话
        /// </summary>
        public void StopSession()
        {
            if (_state == SessionState.Idle || _state == SessionState.Stopped) return;

            _state = SessionState.Stopping;

            if (_host != null)
            {
                _host.InvokeSessionStoppingPipeline();
                _host.TryDestroyBattleWorlds();
                _host.ResetHandles();
            }

            _state = SessionState.Stopped;
        }

        /// <summary>
        /// 暂停会话
        /// </summary>
        public void PauseSession()
        {
            if (_state != SessionState.Running) return;
            _state = SessionState.Paused;
        }

        /// <summary>
        /// 恢复会话
        /// </summary>
        public void ResumeSession()
        {
            if (_state != SessionState.Paused) return;
            _state = SessionState.Running;
        }

        /// <summary>
        /// 处理帧数据
        /// </summary>
        public void OnFrameReceived(int frameIndex, byte[] snapshotData)
        {
            if (_isDisposed) return;
            if (frameIndex <= _currentFrame) return;

            _currentFrame = frameIndex;
            _isFirstFrameReceived = true;

            if (_host != null)
            {
                var ctx = _host.Context;
                if (ctx != null)
                {
                    ctx.LastFrame = frameIndex;
                }
            }
        }

        /// <summary>
        /// 处理玩家输入
        /// </summary>
        public void OnPlayerInput(int playerId, byte[] inputData)
        {
            if (_isDisposed) return;
            // 玩家输入处理逻辑由具体实现提供
        }

        /// <summary>
        /// 获取固定时间步长（秒）
        /// </summary>
        public float GetFixedDeltaSeconds()
        {
            var tickRate = _plan.TickRate;
            if (_plan.HostMode == HostMode.GatewayRemote && _plan.UseGatewayTransport)
            {
                tickRate = 30;
            }
            if (tickRate <= 0) tickRate = 30;
            return 1f / tickRate;
        }

        /// <summary>
        /// 执行帧推进
        /// </summary>
        /// <param name="deltaTime">帧时间</param>
        public void Tick(float deltaTime)
        {
            if (_isDisposed) return;
            if (_state != SessionState.Running) return;

            var fixedDelta = GetFixedDeltaSeconds();
            if (fixedDelta <= 0) return;

            _tickAcc += deltaTime;

            while (_tickAcc >= fixedDelta)
            {
                _tickAcc -= fixedDelta;
                _currentFrame++;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            StopSession();

            _host = null;
            _state = SessionState.Idle;
        }
    }

    /// <summary>
    /// 会话句柄持有者
    /// 用于在编排器中引用各种会话对象
    /// </summary>
    public sealed class SessionHandles
    {
        public IBattleLogicSession Session { get; set; }
        public object ConfirmedWorld { get; set; }
        public object RemoteDrivenWorld { get; set; }
        public object ConfirmedView { get; set; }
        public object SnapshotRouting { get; set; }
        public object NetworkIoDispatcher { get; set; }
        public IReplayRecorder ReplayRecorder { get; set; }
        public IReplayPlayer ReplayPlayer { get; set; }

        /// <summary>
        /// 重置所有句柄
        /// </summary>
        public void Reset()
        {
            Session = null;
            ConfirmedWorld = null;
            RemoteDrivenWorld = null;
            ConfirmedView = null;
            SnapshotRouting = null;
            NetworkIoDispatcher = null;
            ReplayRecorder = null;
            ReplayPlayer = null;
        }
    }
}

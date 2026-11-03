using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 网络事件发送器接口
    /// 定义向网络层派发事件的契约
    /// </summary>
    public interface INetworkEventSender
    {
        /// <summary>
        /// 发送帧快照到网络
        /// </summary>
        void SendSnapshot(int frameIndex, byte[] snapshotData);

        /// <summary>
        /// 发送触发器事件到网络
        /// </summary>
        void SendTriggerEvent(int frameIndex, int eventType, int sourceId, int targetId);

        /// <summary>
        /// 发送玩家输入
        /// </summary>
        void SendPlayerInput(int playerId, byte[] inputData);

        /// <summary>
        /// 广播聊天消息
        /// </summary>
        void BroadcastChatMessage(int senderId, string message);
    }

    /// <summary>
    /// 帧数据接收器接口
    /// 定义接收网络帧数据的契约
    /// </summary>
    public interface INetworkFrameReceiver
    {
        /// <summary>
        /// 接收到帧快照
        /// </summary>
        void OnReceiveSnapshot(int frameIndex, byte[] snapshotData);

        /// <summary>
        /// 接收到触发器事件
        /// </summary>
        void OnReceiveTriggerEvent(int frameIndex, int eventType, int sourceId, int targetId);

        /// <summary>
        /// 接收到玩家输入确认
        /// </summary>
        void OnReceiveInputConfirm(int frameIndex, int playerId);

        /// <summary>
        /// 延迟补偿数据
        /// </summary>
        void OnReceiveDelayCompensation(int frameIndex, int playerId, float roundTripTime);
    }

    /// <summary>
    /// 帧同步广播器接口
    /// 定义服务器帧同步行为
    /// </summary>
    public interface IFrameBroadcaster
    {
        /// <summary>
        /// 当前帧索引
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 是否正在广播
        /// </summary>
        bool IsBroadcasting { get; }

        /// <summary>
        /// 开始广播
        /// </summary>
        void StartBroadcast(int startFrame);

        /// <summary>
        /// 停止广播
        /// </summary>
        void StopBroadcast();

        /// <summary>
        /// 设置帧率
        /// </summary>
        void SetFrameRate(float framesPerSecond);

        /// <summary>
        /// 获取连接的玩家数量
        /// </summary>
        int ConnectedPlayerCount { get; }
    }

    /// <summary>
    /// 回放录制器接口
    /// 定义回放录制行为
    /// </summary>
    public interface IReplayRecorder
    {
        /// <summary>
        /// 开始录制
        /// </summary>
        void StartRecording(string replayId);

        /// <summary>
        /// 停止录制
        /// </summary>
        void StopRecording();

        /// <summary>
        /// 记录帧快照
        /// </summary>
        void RecordSnapshot(int frameIndex, byte[] snapshotData);

        /// <summary>
        /// 记录玩家输入
        /// </summary>
        void RecordInput(int frameIndex, int playerId, byte[] inputData);

        /// <summary>
        /// 是否正在录制
        /// </summary>
        bool IsRecording { get; }
    }

    /// <summary>
    /// 回放播放器接口
    /// 定义回放播放行为
    /// </summary>
    public interface IReplayPlayer
    {
        /// <summary>
        /// 加载回放数据
        /// </summary>
        void LoadReplay(byte[] replayData);

        /// <summary>
        /// 开始播放
        /// </summary>
        void Play();

        /// <summary>
        /// 暂停播放
        /// </summary>
        void Pause();

        /// <summary>
        /// 定位到指定帧
        /// </summary>
        void SeekToFrame(int frameIndex);

        /// <summary>
        /// 设置播放速度
        /// </summary>
        void SetPlaybackSpeed(float speed);

        /// <summary>
        /// 当前播放帧
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 总帧数
        /// </summary>
        int TotalFrames { get; }

        /// <summary>
        /// 是否正在播放
        /// </summary>
        bool IsPlaying { get; }
    }
}

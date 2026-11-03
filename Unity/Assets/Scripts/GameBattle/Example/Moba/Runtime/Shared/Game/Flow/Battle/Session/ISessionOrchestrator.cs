using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// ET/Share 层战斗启动计划。
    /// 该类型用于和 View 层 BattleStartPlan 区分，避免两个同名启动计划继续产生语义漂移。
    /// </summary>
    public readonly struct EtBattleStartPlan
    {
        public int MapId { get; }
        public int WorldId { get; }
        public int GameplayId { get; }
        public int PlayerId { get; }
        public int ClientId { get; }
        public SyncMode SyncMode { get; }
        public HostMode HostMode { get; }
        public int TickRate { get; }
        public bool UseGatewayTransport { get; }
        public bool EnableConfirmedAuthorityWorld { get; }
        public bool EnableReplayRecording { get; }
        public bool EnableReplayPlayback { get; }
        public IReadOnlyList<int> PlayerIds { get; }
        public int InputDelayFrames { get; }
        public string ServerAddress { get; }
        public int ServerPort { get; }

        public EtBattleStartPlan(
            int mapId,
            int worldId,
            int playerId,
            int clientId,
            SyncMode syncMode,
            HostMode hostMode,
            int tickRate,
            bool useGatewayTransport,
            bool enableConfirmedAuthorityWorld,
            bool enableReplayRecording,
            bool enableReplayPlayback,
            IReadOnlyList<int> playerIds,
            string serverAddress = null,
            int serverPort = 0,
            int inputDelayFrames = 0,
            int gameplayId = 1)
        {
            MapId = mapId;
            WorldId = worldId;
            GameplayId = gameplayId;
            PlayerId = playerId;
            ClientId = clientId;
            SyncMode = syncMode;
            HostMode = hostMode;
            TickRate = tickRate;
            UseGatewayTransport = useGatewayTransport;
            EnableConfirmedAuthorityWorld = enableConfirmedAuthorityWorld;
            EnableReplayRecording = enableReplayRecording;
            EnableReplayPlayback = enableReplayPlayback;
            PlayerIds = playerIds;
            InputDelayFrames = inputDelayFrames;
            ServerAddress = serverAddress;
            ServerPort = serverPort;
        }
    }

    /// <summary>
    /// 会话启动计划。
    /// 兼容旧 ET/Share 调用点；新代码优先使用 EtBattleStartPlan 表达边界语义。
    /// </summary>
    public readonly struct BattleStartPlan
    {
        /// <summary>
        /// 地图 ID
        /// </summary>
        public int MapId { get; }

        /// <summary>
        /// 世界 ID
        /// </summary>
        public int WorldId { get; }

        /// <summary>
        /// 玩法 ID
        /// </summary>
        public int GameplayId { get; }

        /// <summary>
        /// 玩家 ID
        /// </summary>
        public int PlayerId { get; }

        /// <summary>
        /// 客户端 ID
        /// </summary>
        public int ClientId { get; }

        /// <summary>
        /// 同步模式
        /// </summary>
        public SyncMode SyncMode { get; }

        /// <summary>
        /// 主机模式
        /// </summary>
        public HostMode HostMode { get; }

        /// <summary>
        /// Tick 率（每秒帧数）
        /// </summary>
        public int TickRate { get; }

        /// <summary>
        /// 是否使用网关传输
        /// </summary>
        public bool UseGatewayTransport { get; }

        /// <summary>
        /// 是否启用确认权威世界
        /// </summary>
        public bool EnableConfirmedAuthorityWorld { get; }

        /// <summary>
        /// 是否启用回放录制
        /// </summary>
        public bool EnableReplayRecording { get; }

        /// <summary>
        /// 是否启用回放播放
        /// </summary>
        public bool EnableReplayPlayback { get; }

        /// <summary>
        /// 玩家 ID 列表
        /// </summary>
        public IReadOnlyList<int> PlayerIds { get; }

        /// <summary>
        /// 输入延迟帧数
        /// </summary>
        public int InputDelayFrames { get; }

        /// <summary>
        /// 服务器地址（客户端模式）
        /// </summary>
        public string ServerAddress { get; }

        /// <summary>
        /// 服务器端口（客户端模式）
        /// </summary>
        public int ServerPort { get; }

        public BattleStartPlan(
            int mapId,
            int worldId,
            int playerId,
            int clientId,
            SyncMode syncMode,
            HostMode hostMode,
            int tickRate,
            bool useGatewayTransport,
            bool enableConfirmedAuthorityWorld,
            bool enableReplayRecording,
            bool enableReplayPlayback,
            IReadOnlyList<int> playerIds,
            string serverAddress = null,
            int serverPort = 0,
            int inputDelayFrames = 0,
            int gameplayId = 1)
        {
            MapId = mapId;
            WorldId = worldId;
            GameplayId = gameplayId;
            PlayerId = playerId;
            ClientId = clientId;
            SyncMode = syncMode;
            HostMode = hostMode;
            TickRate = tickRate;
            UseGatewayTransport = useGatewayTransport;
            EnableConfirmedAuthorityWorld = enableConfirmedAuthorityWorld;
            EnableReplayRecording = enableReplayRecording;
            EnableReplayPlayback = enableReplayPlayback;
            PlayerIds = playerIds;
            InputDelayFrames = inputDelayFrames;
            ServerAddress = serverAddress;
            ServerPort = serverPort;
        }
        public BattleStartPlan(EtBattleStartPlan plan)
            : this(
                plan.MapId,
                plan.WorldId,
                plan.PlayerId,
                plan.ClientId,
                plan.SyncMode,
                plan.HostMode,
                plan.TickRate,
                plan.UseGatewayTransport,
                plan.EnableConfirmedAuthorityWorld,
                plan.EnableReplayRecording,
                plan.EnableReplayPlayback,
                plan.PlayerIds,
                plan.ServerAddress,
                plan.ServerPort,
                plan.InputDelayFrames,
                plan.GameplayId)
        {
        }

        public EtBattleStartPlan ToEtBattleStartPlan()
        {
            return new EtBattleStartPlan(
                MapId,
                WorldId,
                PlayerId,
                ClientId,
                SyncMode,
                HostMode,
                TickRate,
                UseGatewayTransport,
                EnableConfirmedAuthorityWorld,
                EnableReplayRecording,
                EnableReplayPlayback,
                PlayerIds,
                ServerAddress,
                ServerPort,
                InputDelayFrames,
                GameplayId);
        }
    }
 
    /// <summary>
    /// 同步模式
    /// </summary>
    public enum SyncMode
    {
        /// <summary>
        /// 锁步同步
        /// </summary>
        Lockstep = 0,

        /// <summary>
        /// 快照权威同步
        /// </summary>
        SnapshotAuthority = 1,

        /// <summary>
        /// 状态同步
        /// </summary>
        StateSync = 2,

        /// <summary>
        /// 混合模式（帧同步 + 预测）
        /// </summary>
        Hybrid = 3,
    }

    /// <summary>
    /// 主机模式
    /// </summary>
    public enum HostMode
    {
        /// <summary>
        /// 本地模式（单机）
        /// </summary>
        Local = 0,

        /// <summary>
        /// 网关远程模式
        /// </summary>
        GatewayRemote = 1,
    }

    /// <summary>
    /// 逻辑模式
    /// </summary>
    public enum LogicMode
    {
        /// <summary>
        /// 本地逻辑
        /// </summary>
        Local = 0,

        /// <summary>
        /// 远程逻辑
        /// </summary>
        Remote = 1,
    }

    /// <summary>
    /// 运行模式
    /// </summary>
    public enum RunMode
    {
        /// <summary>
        /// 正常模式
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 录制模式
        /// </summary>
        Record = 1,

        /// <summary>
        /// 回放模式
        /// </summary>
        Replay = 2,
    }

    /// <summary>
    /// 会话编排器接口
    /// 定义会话生命周期管理的核心契约
    /// 
    /// 这是 Share 层定义的核心接口，负责：
    /// 1. 管理战斗逻辑会话的生命周期
    /// 2. 协调各个子系统（Snapshot、View、Replay）
    /// 3. 处理帧同步
    /// </summary>
    public interface ISessionOrchestrator
    {
        /// <summary>
        /// 获取当前会话状态
        /// </summary>
        SessionState State { get; }

        /// <summary>
        /// 获取当前帧索引
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 获取启动计划
        /// </summary>
        BattleStartPlan Plan { get; }

        /// <summary>
        /// 初始化编排器
        /// </summary>
        /// <param name="plan">启动计划</param>
        void Initialize(in BattleStartPlan plan);

        /// <summary>
        /// 启动会话
        /// </summary>
        void StartSession();

        /// <summary>
        /// 停止会话
        /// </summary>
        void StopSession();

        /// <summary>
        /// 暂停会话
        /// </summary>
        void PauseSession();

        /// <summary>
        /// 恢复会话
        /// </summary>
        void ResumeSession();

        /// <summary>
        /// 处理帧数据
        /// </summary>
        /// <param name="frameIndex">帧索引</param>
        /// <param name="snapshotData">快照数据</param>
        void OnFrameReceived(int frameIndex, byte[] snapshotData);

        /// <summary>
        /// 处理玩家输入
        /// </summary>
        /// <param name="playerId">玩家 ID</param>
        /// <param name="inputData">输入数据</param>
        void OnPlayerInput(int playerId, byte[] inputData);

        /// <summary>
        /// 获取固定时间步长（秒）
        /// </summary>
        float GetFixedDeltaSeconds();

        /// <summary>
        /// 释放资源
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// 会话编排器宿主接口
    /// 定义会话编排器所需的外部依赖
    /// </summary>
    public interface ISessionOrchestratorHost
    {
        /// <summary>
        /// 获取启动计划
        /// </summary>
        BattleStartPlan Plan { get; }

        /// <summary>
        /// 获取会话上下文
        /// </summary>
        ISessionContext Context { get; }

        /// <summary>
        /// 获取视图事件接收器
        /// </summary>
        IBattleViewEventSink ViewEventSink { get; }

        /// <summary>
        /// 获取快照反序列化器
        /// </summary>
        IFrameSnapshotDeserializer SnapshotDeserializer { get; }

        /// <summary>
        /// 启动战斗逻辑会话
        /// </summary>
        IBattleLogicSession StartBattleLogicSession(BattleLogicSessionOptions options);

        /// <summary>
        /// 启动远程驱动的本地世界
        /// </summary>
        void StartRemoteDrivenLocalWorld();

        /// <summary>
        /// 启动确认权威世界
        /// </summary>
        void StartConfirmedAuthorityWorld();

        /// <summary>
        /// 调用会话启动管道
        /// </summary>
        void InvokeSessionStartingPipeline();

        /// <summary>
        /// 调用会话停止管道
        /// </summary>
        void InvokeSessionStoppingPipeline();

        /// <summary>
        /// 调用回放设置管道
        /// </summary>
        void InvokeReplaySetupPipeline();

        /// <summary>
        /// 尝试销毁战斗世界
        /// </summary>
        void TryDestroyBattleWorlds();

        /// <summary>
        /// 重置句柄
        /// </summary>
        void ResetHandles();
    }

    /// <summary>
    /// 会话上下文接口
    /// 提供会话运行时的共享状态
    /// </summary>
    public interface ISessionContext
    {
        /// <summary>
        /// 获取战斗逻辑会话
        /// </summary>
        IBattleLogicSession Session { get; set; }

        /// <summary>
        /// 获取最后处理的帧索引
        /// </summary>
        int LastFrame { get; set; }

        /// <summary>
        /// 获取输入录制写入器
        /// </summary>
        IInputRecordWriter InputRecordWriter { get; set; }
    }

    /// <summary>
    /// 输入录制写入器接口
    /// </summary>
    public interface IInputRecordWriter : IDisposable
    {
        /// <summary>
        /// 写入输入
        /// </summary>
        void Write(int frameIndex, int playerId, byte[] inputData);
    }

    /// <summary>
    /// 战斗逻辑会话选项
    /// </summary>
    public class BattleLogicSessionOptions
    {
        public LogicMode Mode { get; set; }
        public int WorldId { get; set; }
        public int WorldType { get; set; }
        public int ClientId { get; set; }
        public int PlayerId { get; set; }
        public System.Reflection.Assembly[] ScanAssemblies { get; set; }
        public string[] NamespacePrefixes { get; set; }
        public bool AutoConnect { get; set; }
        public bool AutoCreateWorld { get; set; }
        public bool AutoJoin { get; set; }
    }

    /// <summary>
    /// 战斗逻辑会话接口
    /// 定义战斗逻辑会话的基本契约
    /// </summary>
    public interface IBattleLogicSession : IDisposable
    {
        /// <summary>
        /// 会话 ID
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// 是否已连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 连接
        /// </summary>
        void Connect();

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 加入世界
        /// </summary>
        void Join();

        /// <summary>
        /// 离开世界
        /// </summary>
        void Leave();

        /// <summary>
        /// 提交输入
        /// </summary>
        void SubmitInput(int playerId, byte[] inputData);

        /// <summary>
        /// 帧接收事件
        /// </summary>
        event Action<int, byte[]> FrameReceived;
    }
}

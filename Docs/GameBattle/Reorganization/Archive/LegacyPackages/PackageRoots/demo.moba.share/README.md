# com.abilitykit.demo.moba.share

MOBA 演示项目的跨平台共享模块。

## 概述

Share 模块定义了 MOBA 游戏战斗系统在 Console、Unity 等不同展示层之间共享的接口、数据结构和核心实现。它不包含任何平台特定的代码，只提供抽象契约和通用逻辑。

## 模块结构

```
Runtime/Game/Flow/Battle/
├── Session/           # 会话管理
├── Snapshot/         # 帧快照系统
├── View/             # 视图绑定系统
├── ViewEvents/       # 视图事件系统
├── Input/            # 输入系统
├── Network/          # 网络通信
├── FrameSync/        # 帧同步
├── Phase/            # 游戏阶段
└── Replay/           # 回放系统
```

## 模块说明

### Session (会话管理)

管理战斗会话的生命周期，包括：
- `SessionOrchestrator` - 会话编排器
- `ISessionOrchestrator` - 会话管理接口
- `BattleStartPlan` - 战斗启动配置
- `FeatureModule` - Feature 模块系统

### Snapshot (帧快照系统)

帧同步的核心数据结构：
- `FrameSnapshotData` - 快照帧数据
- `FrameSnapshotDispatcher` - 快照分发器
- `FrameSnapshotAssembler` - 快照组装器
- `MobaOpCode` - MOBA 操作码定义

### View (视图绑定)

实体与视图的绑定关系：
- `IBattleView` - 战斗视图接口
- `IViewFactory` - 视图工厂接口
- `ViewBinder` - 视图绑定器

### ViewEvents (视图事件)

视图事件的发布订阅：
- `IBattleViewEventSink` - 视图事件接收器
- `SnapshotViewAdapter` - 快照事件适配器
- `TriggerEventBridge` - 触发器事件桥接

### Input (输入系统)

玩家输入管理：
- `PlayerInputData` - 玩家输入数据
- `InputBuffer` - 输入缓冲
- `InputScheduler` - 输入调度器

### Network (网络通信)

网络事件发送：
- `INetworkEventSender` - 网络事件发送器
- `NetworkEventSender` - 默认实现
- `NullNetworkEventSender` - 空实现（本地使用）

### FrameSync (帧同步)

帧时间管理：
- `FrameSync` - 帧同步实现
- `IFrameSyncController` - 帧同步控制器
- `IFrameTimeProvider` - 帧时间提供者

### Phase (游戏阶段)

游戏阶段状态机：
- `GamePhase` - 阶段枚举
- `GamePhaseStateMachine` - 状态机实现
- `PhaseTransitionRule` - 转换规则

### Replay (回放系统)

战斗回放录制和播放：
- `ReplayRecorder` - 录制器
- `ReplayPlayer` - 播放器
- `ReplayHeader` - 回放文件头

## 核心接口

### IBattleViewEventSink

视图事件的核心接收器，由平台层实现：

```csharp
public interface IBattleViewEventSink
{
    void OnEnterGameSnapshot(in FrameSnapshotData snapshot);
    void OnActorTransformSnapshot(in FrameSnapshotData snapshot);
    void OnProjectileEventSnapshot(in FrameSnapshotData snapshot);
    void OnAreaEventSnapshot(in FrameSnapshotData snapshot);
    void OnDamageEventSnapshot(in FrameSnapshotData snapshot);
    void OnTriggerEvent(in TriggerEventData evt);
    void OnBattleStart(int frameIndex);
    void OnBattleEnd(int frameIndex, int winTeamId);
}
```

### ISessionOrchestrator

会话生命周期管理接口：

```csharp
public interface ISessionOrchestrator
{
    SessionState State { get; }
    int CurrentFrame { get; }
    BattleStartPlan Plan { get; }
    void Initialize(in BattleStartPlan plan);
    void StartSession();
    void StopSession();
    void PauseSession();
    void ResumeSession();
}
```

## 使用示例

### 1. 创建战斗会话

```csharp
var plan = new BattleStartPlan(
    mapId: 1,
    worldId: 1,
    playerId: localPlayerId,
    clientId: 1,
    syncMode: SyncMode.SnapshotAuthority,
    hostMode: HostMode.Local,
    tickRate: 30,
    useGatewayTransport: false,
    enableConfirmedAuthorityWorld: true,
    enableReplayRecording: true,
    enableReplayPlayback: false,
    playerIds: new[] { 1, 2, 3, 4, 5, 6 }
);

var orchestrator = new SessionOrchestrator();
orchestrator.Initialize(plan);
```

### 2. 订阅视图事件

```csharp
var dispatcher = new FrameSnapshotDispatcher();

dispatcher.Subscribe((int)MobaOpCode.DamageEventSnapshot, (frame, data) =>
{
    foreach (var damage in data)
    {
        Console.WriteLine($"Actor #{damage.TargetId} took {damage.DamageValue} damage");
    }
});
```

### 3. 使用回放系统

```csharp
// 录制
var recorder = new ReplayRecorder();
recorder.StartRecording("battle_001");
recorder.RecordSnapshot(frameIndex, snapshotData);
var replayData = recorder.StopRecordingAndGetData();

// 播放
var player = new ReplayPlayer();
player.LoadReplay(replayData);
player.Play();
```

## 平台适配

Share 模块被以下平台使用：

| 平台 | 项目 | 说明 |
|------|------|------|
| Console | `AbilityKit.Demo.Moba.Console` | 命令行演示 |
| Unity | `com.abilitykit.demo.moba.view` | Unity 3D 演示 |

### Console 平台实现

Console 端的视图层使用 ASCII 字符渲染：
- `ConsoleBattleView` - Console 战斗视图
- `ConsoleViewEventSink` - Console 事件接收
- `ConsoleAreaViewSystem` - ASCII 区域渲染
- `ConsoleFloatingTextSystem` - ASCII 飘字

### Unity 平台实现

Unity 端的视图层使用 GameObject 渲染：
- `BattleView` - Unity 战斗视图
- `BattleViewEventSink` - Unity 事件接收
- `AreaViewManager` - Unity 区域渲染
- `VfxManager` - Unity 特效系统

## 依赖关系

```
com.abilitykit.demo.moba.share
├── com.abilitykit.core
├── com.abilitykit.host
├── com.abilitykit.demo.moba.runtime
└── com.unity.mathematics
```

## 许可证

项目许可证

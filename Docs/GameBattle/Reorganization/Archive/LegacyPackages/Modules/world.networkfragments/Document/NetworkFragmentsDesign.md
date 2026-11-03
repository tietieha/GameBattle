# com.abilitykit.world.networkfragments：网络帧数据层

## 目标

本包提供帧数据包结构和远程帧缓冲区，与具体网络传输解耦。

## 核心类型

| 类型 | 命名空间 | 说明 |
|------|----------|------|
| `ISnapshotEnvelope` | `AbilityKit.Ability.Host` | 快照信封接口：包含 `WorldId` 和可选的 `WorldStateSnapshot` |
| `FramePacket` | `AbilityKit.Ability.Host` | 帧数据包：包含 `WorldId`、`FrameIndex`、输入列表和可选快照 |
| `RemoteFrameBuffer<TFrame>` | `AbilityKit.Ability.Host` | 通用帧缓冲区（实现 `IRemoteFrameSource/Sink`），支持 `TrimBefore` 清理 |
| `RemoteFrameAggregator` | `AbilityKit.Ability.Host` | 输入/快照聚合器，同一帧多条消息合并 |
| `RemoteSnapshotFrame` | `AbilityKit.Ability.Host` | 快照帧聚合结果：包含 `FrameIndex` 和 `ISnapshotEnvelope[]` |
| `RemoteInputFrame` | `AbilityKit.Ability.Host` | 输入帧聚合结果：包含 `FrameIndex` 和 `PlayerInputCommand[]` |

## 目录结构

```
com.abilitykit.world.networkfragments/Runtime/
└── Frames/
    ├── ISnapshotEnvelope.cs       # 快照信封接口
    ├── FramePacket.cs            # 帧数据包 + SnapshotProviderDrain
    ├── RemoteFrameBuffer.cs       # 通用帧缓冲区
    ├── RemoteFrameAggregator.cs   # 帧聚合器
    ├── RemoteSnapshotFrame.cs     # 快照帧聚合结果
    └── RemoteInputFrame.cs       # 输入帧聚合结果
```

## 依赖关系

```
AbilityKit.Core
├── Network.Runtime
├── framesync
├── host
│     └── WorldStateSnapshot (opCode + payload)
└── networkfragments
      └── Frames: FramePacket, ISnapshotEnvelope, RemoteBuffer, Aggregator
```

## 适配器位置

`FramePacketNetAdapter`（处理帧数据 + 快照分发的适配器）放置在 `com.abilitykit.host.extension` 包的 `Runtime/Session/` 子目录下，因为它需要同时引用：

- `networkfragments`（提供 `FramePacket`、`ISnapshotEnvelope`）
- `world.snapshot`（提供 `FrameSnapshotDispatcher`）
- `host`（提供 `IWorldStateSnapshotProvider`）

## 非目标

- 不处理 socket/kcp/websocket 连接
- 不定义网络协议格式
- 不负责序列化/反序列化

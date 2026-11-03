# com.abilitykit.world.snapshot：快照路由/解码层职责边界

## 目标

本包提供**与网络传输无关**的"世界快照（World Snapshot）"处理框架：

- 以 `opCode` 为键注册解码器（decoder）
- 将快照字节流解码为强类型 payload
- 以管线（pipeline）方式将 payload 分发给处理器（handler）

本包**不负责**：连接管理、可靠性、重连、线程模型、消息拆包/组包、非快照协议等"通用网络协议层"问题。

## 核心类型

| 类型 | 命名空间 | 说明 |
|------|----------|------|
| `FrameSnapshotDispatcher` | `AbilityKit.Core.Snapshots.Routing` | 按 opCode 分发快照 payload |
| `SnapshotPipeline` | `AbilityKit.Core.Snapshots.Routing` | 有序阶段管线，按 order 执行 |
| `SnapshotCmdHandler` | `AbilityKit.Core.Snapshots.Routing` | 命令型处理器 |
| `ISnapshotDecoderRegistry` | `AbilityKit.Core.Snapshots.Routing` | decoder 注册接口 |
| `ISnapshotPipelineStageRegistry` | `AbilityKit.Core.Snapshots.Routing` | 管线阶段注册接口 |
| `ISnapshotEnvelope` | `AbilityKit.Ability.Host` | 快照信封接口（来自 `world.networkfragments`） |

## 架构约束

**`world.snapshot` 只依赖 `core` + `host` + `framesync` + `networkfragments`**（不含直接 Network 依赖）：

| 包 | 依赖 | 说明 |
|----|------|------|
| `world.snapshot` | `core`, `host`, `framesync`, `networkfragments` | 纯路由/解码基础设施 |
| `world.statesync` | `snapshot`, `framesync`, `core` | 业务状态同步、客户端预测 |
| `world.networkfragments` | `core`, `framesync`, `host`, `Network.Runtime` | 帧数据结构 |
| `host` | `framesync`, `Network.Runtime` | Host 框架、WorldStateSnapshot |

## 快照类型归属

### 通用快照（`host` 包）

`WorldStateSnapshot`（业务快照 opCode + payload 容器）定义在 `AbilityKit.Ability.Host`（`com.abilitykit.host` 包）。

### 业务快照（`world.statesync` 包）

`WorldStateSnapshot` 的完整 MemoryPackable 实现（包含 Vec3/Quat 序列化）定义在 `AbilityKit.Ability.StateSync.Snapshot`（`com.abilitykit.world.statesync` 包），类名同样为 `WorldStateSnapshot`，通过命名空间区分。

## 与 networkfragments 的关系

```
网络层收到帧数据
  → 构造 FramePacket（来自 networkfragments）
  → 调用 FramePacketNetAdapter（来自 host.extension/Session/）
  → FrameSnapshotDispatcher.Feed()（来自 world.snapshot）
  → 路由到具体 handler
```

`host.extension/Session/FramePacketNetAdapter` 负责将网络层的 `FramePacket` 转换为 `ISnapshotEnvelope` 并喂给 `FrameSnapshotDispatcher`。

`snapshot` 包不知道"谁负责收包/如何收包"，`networkfragments` 不知道"某个 opCode 的 payload 怎么 decode"。

## 非目标（明确不做的事情）

- 不处理 socket/kcp/websocket 的连接与收发
- 不处理网络消息的通用路由（message id -> handler）
- 不规定序列化格式（protobuf/flatbuffers/自定义二进制均可），只要求上层提供 decoder
- 不负责帧缓冲、抖动处理（由 `host.extension/Session` 负责）

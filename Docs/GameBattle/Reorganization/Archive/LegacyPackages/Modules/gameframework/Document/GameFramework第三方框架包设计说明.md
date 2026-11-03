# Ability-Kit GameFramework 第三方框架包设计说明

> **阅读对象**：需要理解项目内 GameFramework 源码包角色和 AbilityKit 对它的使用边界的开发者。
>
> **文档目标**：说明该包是较完整的 GameFramework 运行时源码集合，主要用于兼容和网络适配，不应被当作 AbilityKit 自研核心模块随意改造。

---

## 一、包定位

`com.abilitykit.gameframework` 包含 GameFramework 的运行时源码、编辑器 asmdef、README、LICENSE 和大量基础模块。它提供事件、网络、资源、实体、UI、声音、场景、设置、数据表、数据节点、工具函数等通用游戏框架能力。

当前 AbilityKit 更重点使用它的 Network 部分，并由 `com.abilitykit.gameframework.network` 提供 Gateway 连接适配。

---

## 二、模块边界

负责：

- 保留 GameFramework 上游基础框架能力。
- 提供 `Runtime/GameFramework` 下的事件、网络、资源、实体、UI、声音等模块。
- 为 AbilityKit 网络适配包提供 `INetworkChannel`、`IPacketHeader`、`Packet` 等基础类型。

不负责：

- 不承载 AbilityKit 自研战斗/技能/World 逻辑。
- 不建议直接在该包中加入 AbilityKit 特定业务。
- 不负责 Protocol.Moba 或 Network.Runtime 的协议模型。

---

## 三、关键目录

| 路径 | 职责 |
|------|------|
| `Runtime/GameFramework/Network` | 网络通道、包头、包处理、网络管理器 |
| `Runtime/GameFramework/Event` | 事件管理 |
| `Runtime/GameFramework/Entity` | 实体显示/隐藏管理 |
| `Runtime/GameFramework/UI` | UI 组和窗口管理 |
| `Runtime/GameFramework/Resource` | 资源相关接口 |
| `Runtime/GameFramework/Scene` | 场景加载/卸载事件和管理 |
| `Runtime/GameFramework/Sound` | 声音播放和声音组 |
| `Runtime/GameFramework/Utility` | 文本、路径、随机、校验等工具 |

---

## 四、维护建议

- 尽量把 AbilityKit 适配逻辑放到独立包，例如 `gameframework.network`。
- 如需修改上游源码，应记录原因和影响范围。
- 网络模块是当前最重要的使用面，改动前应同步检查 `GameFrameworkNetworkChannelConnection`。
- 保留 LICENSE 和 README，避免第三方授权信息丢失。

---

*文档版本：1.0*  
*最后更新：2026-06-05*

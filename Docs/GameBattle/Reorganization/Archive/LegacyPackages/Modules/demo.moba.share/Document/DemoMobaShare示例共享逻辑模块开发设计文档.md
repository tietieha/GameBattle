# Ability-Kit Demo Moba Share 示例共享逻辑模块开发设计文档

> **阅读对象**：需要理解 Moba 示例中客户端和服务端共享逻辑、配置 DTO、战斗流程接口的开发者。
>
> **文档目标**：说明该包是最佳实践示例工程的共享逻辑层，不是所有项目必须引入的框架核心。

---

## 一、设计理念

Demo Moba Share 放置 Moba 示例跨端共享内容：枚举、配置 DTO、战斗流程、输入、快照、回放、网络事件、视图事件接口等。它展示 AbilityKit 框架如何被组合成一个完整玩法工程。

---

## 二、模块边界

负责：

- 定义 Moba 通用枚举：属性、Buff、实体、技能、队伍。
- 定义配置 DTO：角色、技能、效果、投射物、表现、玩法等。
- 定义战斗流程模块、阶段状态机和 session orchestrator。
- 定义输入缓冲、帧同步、快照组装/派发、回放录制/播放。
- 定义视图事件接口和 SnapshotViewAdapter。

不负责：

- 不负责 Unity 编辑器配置资产，编辑器资产在 `demo.moba.editor`。
- 不负责 Unity 表现 prefab。
- 不作为通用框架强依赖。

---

## 三、关键目录

| 路径 | 职责 |
|------|------|
| `Runtime/Game/Common` | Moba 枚举 |
| `Runtime/Game/Config/Dto` | 配置 DTO |
| `Runtime/Game/Flow/Battle/Input` | 输入模型和输入缓冲 |
| `Runtime/Game/Flow/Battle/FrameSync` | 帧同步接口和实现 |
| `Runtime/Game/Flow/Battle/Snapshot` | 帧快照数据、组装、派发 |
| `Runtime/Game/Flow/Battle/Replay` | 回放录制和播放 |
| `Runtime/Game/Flow/Battle/Session` | session controller/orchestrator |
| `Runtime/Game/Flow/Battle/ViewEvents` | 表现事件接口和适配器 |

---

## 四、注意事项

- 这是最佳实践示例包，真实项目应按自己的领域拆分共享逻辑。
- DTO 字段会影响编辑器导出、协议和服务端读取，修改时需同步。
- Share 层不应依赖 UnityEditor 或具体 prefab。

---

*文档版本：1.0*  
*最后更新：2026-06-05*

# Ability-Kit ThirdParty Entitas 第三方 ECS 源码包说明

> **阅读对象**：需要理解 Entitas 在 AbilityKit 中角色和维护方式的开发者。
>
> **文档目标**：说明该包是 Entitas 第三方 ECS 源码/程序集包，AbilityKit 通过 `world.entitas` 做适配。

---

## 一、包定位

该包保存 Entitas ECS 第三方内容。它提供 Context、Entity、Group、Matcher、Systems 等 ECS 基础能力。AbilityKit 不在该包中实现 World 适配，而是通过 `com.abilitykit.world.entitas` 包装。

---

## 二、边界说明

负责：

- 提供 Entitas ECS 基础类型。
- 为 World Entitas 适配包提供底层能力。

不负责：

- 不定义 AbilityKit World 接口。
- 不定义 Moba 组件。
- 不直接参与 Host/DI 组合。

---

## 三、维护建议

- 尽量避免修改上游 Entitas 源码。
- 若升级 Entitas，需要同步检查 generated contexts、world.entitas 和 thirdparty.desperatedevs。
- 业务系统应继承/组合 `world.entitas` 的系统基类。

---

*文档版本：1.0*  
*最后更新：2026-06-05*

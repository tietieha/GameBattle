# Ability-Kit ThirdParty DesperateDevs 第三方 Entitas 依赖包说明

> **阅读对象**：需要维护 Entitas/DesperateDevs 相关依赖的开发者。
>
> **文档目标**：说明该包作为第三方依赖源码/程序集集合的边界。

---

## 一、包定位

DesperateDevs 是 Entitas 生态中的基础依赖。该包用于保存 AbilityKit World Entitas 适配所需的第三方依赖，属于外部基础设施，不是 AbilityKit 自研逻辑。

---

## 二、维护边界

负责：

- 提供 Entitas 相关工具链所需的 DesperateDevs 类型。
- 支撑 `thirdparty.entitas` 和 `world.entitas`。

不负责：

- 不定义游戏组件。
- 不定义 AbilityKit World 生命周期。
- 不承载业务代码。

---

## 三、维护建议

- 与 Entitas 版本一起升级，避免依赖版本错配。
- 保留上游授权文件。
- 自研扩展应放在 `world.entitas`。

---

*文档版本：1.0*  
*最后更新：2026-06-05*

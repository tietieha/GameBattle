# Ability-Kit ThirdParty ActionEditor 第三方动作编辑器源码包说明

> **阅读对象**：需要了解 ActionEditor 第三方源码在项目中角色的工具开发者。
>
> **文档目标**：说明该包是第三方/上游动作编辑器源码集合，AbilityKit 自研逻辑应通过适配包扩展。

---

## 一、包定位

`com.abilitykit.thirdparty.actioneditor` 保存动作编辑器第三方源码。它为 AbilityKit 的动作编辑器实现和预览工具提供基础类型、窗口能力或 Directable 体系。

---

## 二、维护边界

负责：

- 保存第三方动作编辑器源码。
- 为 `actioneditor.impl` 和 `base.editor` 提供底层编辑能力。

不负责：

- 不承载 AbilityKit 领域技能规则。
- 不承载 Moba 示例配置。
- 不建议直接修改上游源码实现自研需求。

---

## 三、维护建议

- 自研扩展优先放在 `actioneditor.impl` 或 `base.editor`。
- 如必须修改第三方源码，应记录修改点和原因。
- 升级上游时先对比 API 兼容性，再检查预览器和 SkillAsset。

---

*文档版本：1.0*  
*最后更新：2026-06-05*

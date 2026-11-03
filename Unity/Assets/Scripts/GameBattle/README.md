# GameBattle

日期：2026-10-26。目录整理后，框架程序集收敛为 Editor、Runtime、Unity 各一个；不重写战斗算法。

## 目录边界

```text
Assets/Scripts/GameBattle/
  Editor/           框架编辑、导入导出、验证、调试和编辑器依赖
    Runtime/        按 Runtime 的模块目录组织对应编辑工具
      Ability/
      Battle/
      Content/
      Decision/
      Entity/
      Foundation/
      Pooling/
    Unity/          按 Unity 的模块目录组织对应编辑工具
      Content/
      Decision/
      Entity/
      Foundation/
    GameBattle.Editor.asmdef
  Runtime/          不依赖 Unity 的逻辑与数据
    Battle/         战斗世界、命令、驱动和协调
    Entity/         实体、索引、存储、标签和纯逻辑 ECS 适配
    Attribute/      属性、修饰器和计算
    Ability/        技能执行、管线、可选时间轴
    Effect/         效果执行、伤害和持续过程
    Buff/           Buff 状态、存储、策略、命令、生命周期、计时和上下文身份
    Targeting/      目标查询和选择
    Movement/       运动、碰撞与导航接口
    Projectile/     投射物与持续区域
    Decision/       行为、行为树、状态机和决策契约
    Content/        定义、契约、目录索引、来源和序列化
    Foundation/     通用数学、集合、时间、日志和基础依赖
    Pooling/        纯 C# 对象池与缓冲区
  Unity/            引擎启动、输入、资源、表现、计算和对象池适配
  Example/          具体战斗类型的逻辑、适配、编辑器、测试和资源
    Moba/
    Shooter/
    Tiny/
    Foundation/
    Shared/
    FrameworkTests/ 暂存原框架测试，保留各测试程序集
```

## 本轮约束

- 类型名、命名空间和原脚本/资源 GUID 保留。框架程序集身份变为 GameBattle.Runtime、GameBattle.Unity、GameBattle.Editor。
- Editor、Runtime、Unity 内不再保留子程序集或 asmref；子目录只表达模块职责。
- Editor/Runtime、Editor/Unity 表示被编辑模块的归属，不代表编辑器代码必须纯 C#；二者均属于 GameBattle.Editor。
- 编辑器按对应模块组织，不再把 Authoring、Debugging、ImportExport、Workspace 作为 Editor 顶层分类。没有编辑工具的模块不创建空目录。
- 依赖方向为 Editor -> Unity -> Runtime，Editor 也直接引用 Runtime。Example 原程序集名称保留，仅修正依赖。
- Runtime 程序集启用 noEngineReferences；框架程序集不得引用 Example。
- Unity 启动钩子、引擎采样器移入 Unity，日志与录像路径通过外部注入。
- Buff 通用状态、存储、命令、应用/结束、生命周期调和、计时与上下文身份已提取到 Runtime/Buff；MOBA 的配置、实体访问、效果/标签/触发器和快照适配留 Example。Buff 跟战斗逻辑帧推进，不引入独立时钟；全局 Buff 暂缓。
- 时间轴仍可选，玩家输入和程序决策的既有执行机制保留。
- 网络、宿主、同步、录像、诊断作为 Battle 内的独立子模块保留，未重写其内部设计。
- 原框架的八组 Unity 测试暂存到 Example/FrameworkTests，保留 NUnit、平台和编译约束；测试辅助逻辑仍可留在 Runtime/Battle/Testing。
- 样例与工具中的隐藏目录继续隐藏，避免历史源码进入默认程序集。
- 历史包文档、许可证、合并目录元数据和旧构建缓存归档到 Docs/GameBattle/Reorganization/Archive。
- src 下的 .NET 工程继续作为构建与测试入口，链接到新源码目录，不复制实现。

## 程序集

| 目录 | 程序集 | 内容与约束 |
| --- | --- | --- |
| Runtime | GameBattle.Runtime | 纯逻辑，noEngineReferences=true |
| Unity | GameBattle.Unity | Unity 适配，引用 Runtime |
| Editor | GameBattle.Editor | 仅 Editor 平台，引用 Runtime 和 Unity |
| Example | 原名称保留 | 本轮不合并，后续再整理 |

Compute Jobs 仍由 com.unity.collections 的版本条件启用；MLAgents 桥接仍受
ABILITYKIT_ML_AGENTS 控制。Svelto 的版本条件保留。当前未安装 Entities，因此
不添加失效的 Unity.Entities 引用；以后启用 DOTS 适配时，需安装兼容包并给
GameBattle.Unity 补上该引用。

旧 asmdef/asmref 归档在 Docs/GameBattle/AssemblyConsolidation/Archive，不再参与 Unity 编译。

## 可核对记录

2026-11-03 已清理历史构建日志；下列文档和结果摘要保留，原日志路径不再保证存在。清理范围与本机残留见 `Docs/GameBattle/Cleanup.md`。

- `Docs/GameBattle/Reorganization/plan.json`：本轮逐文件映射、哈希、程序集归属和进度。
- `Docs/GameBattle/Reorganization/verification.json`：完整性、元数据、程序集和 Runtime 边界检查。
- `Docs/GameBattle/Reorganization/Progress.md`：本轮范围、分步进度及保留限制。
- `Docs/GameBattle/AssemblyConsolidation/Validation.md`：当前三个框架程序集的结果与验证限制。
- `Docs/GameBattle/AssemblyConsolidation/verification.json`：原文件/GUID、程序集身份、引用和工程输入检查。
- `Docs/GameBattle/AssemblyConsolidation/builds.json`：十九个生成工程的逐项编译结果。
- `Docs/GameBattle/EditorMirroring/Validation.md`：当前 Editor 镜像目录、路径迁移和验证限制。
- `Docs/GameBattle/EditorMirroring/plan.json`：编辑器目录的逐文件映射与元数据基线。
- `Docs/GameBattle/EditorMirroring/builds.json`：镜像迁移后的生成工程编译结果。
- `Docs/GameBattle/EditorDiagnostics/Validation.md`：MOBA 编译生成器修复与真实 Unity MCP 验证。
- `Docs/GameBattle/ExampleExtraction/Integration.md`：本轮提取清单、保留内容和外部驱动/命令/寻路/资源/表现接口规范。
- `Docs/GameBattle/ExampleExtraction/changes.json`：本轮逐文件迁移、原文件备份和明确授权的代码改动。
- `Docs/GameBattle/BuffExtraction/Integration.md`：后续 Buff 提取清单、业务端口和逻辑帧驱动规范。
- `Docs/GameBattle/BuffExtraction/Validation.md`：Buff 编译、测试、Unity 烟测和验证限制。
- `Docs/GameBattle/BuffExtraction/changes.json`：Buff 原源码备份、元数据基线和授权代码变更哈希。

目录归属与实现解耦是两件事。Buff 生命周期核心已共享；跨玩法业务适配、全局作用域、数据设计和 Example 内部整理仍需后续工作。

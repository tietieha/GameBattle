# Editor 镜像目录

日期：2026-10-26。主工程：E:\Unity\AbilityKit\Unity。

## 当前结构

```text
Assets/Scripts/GameBattle/Editor/
  GameBattle.Editor.asmdef
  Runtime/
    Ability/                         技能配置、管线和编辑工具
    Battle/
      Rules/                         规则、触发器和代码生成
      Diagnostics/                   分析、指标、Trace、AbilityExplain
      Networking/Sdk/                网络监视
      Serialization/Protocol/        协议配置编辑
      Testing/BattleScenario/         战斗场景辅助工具
    Content/Serialization/Excel/      数据浏览、Excel 导入导出
    Decision/BehaviorTree/            行为树编辑、预览、观察和导出
    Entity/Tags/                     两套既有标签编辑工具
    Foundation/Dependencies/          基础依赖的编辑器部分
    Pooling/Diagnostics/              逻辑对象池监视
  Unity/
    Content/Timeline/                 时间轴 GUI、预览、表现采样
    Decision/StateMachine/            Unity 状态机图和调试
    Entity/Dependencies/              Entitas、Svelto 编辑器工具
    Foundation/                      SceneView 绘制、公共编辑平台
```

Runtime、Unity 两个分支表示“所编辑模块”的归属。两者仍使用 UnityEditor，
都编入同一个仅 Editor 平台的 GameBattle.Editor，没有新建子程序集。
模块名称与外部 Runtime、Unity 一致；具体工具在对应模块中再细分。
没有编辑器实现的模块不创建空目录。

## 迁移与约束

- 迁移 23 组目录，核对 1,082 个原文件，包括 402 个有效编辑器源码和
  3 个隐藏历史源码。原文件和 .meta/GUID 全部保留。
- 旧分类下的空父目录清理，原父目录元数据归档到本目录 Archive。
- Runtime、Unity、Example 的源码、资源和元数据未移动、未改写。
- 编辑器源码仅更新 StateMachineEditorWindow 的 UXML/USS 加载路径。
- 协议编辑器路径冻结测试更新路径；工程源文件链接、插件 DLL 和 Analyzer
  路径同步更新。未修改类型名、命名空间、序列化结构或战斗算法。
- Example/FrameworkTests 保持原位置；本阶段不整理 Example。
- 程序集验证工具识别本阶段逐文件映射，继续验证上一阶段的原 GUID 和文件完整性。

## 验证记录

- plan.json：基线哈希、目录路由、逐文件去向和外部模块不变性基线。
- verification.json：原文件/元数据完整性、单程序集、镜像模块、路径引用检查。
- builds.json / BuildLogs：三个框架程序集、八组框架测试和代表性样例的生成工程编译。
- StateMachine 的 EditorLayout.uxml、EditorStyles.uss 在新路径均存在。
- 上一阶段的程序集/GUID和工程输入检查仍通过，无 GameBattle 范围的失效输入。

## 已知限制

以下源生成器问题是本阶段最初观察到的状态。后续已修复 DLL 的程序集作用域，
并通过已连接的 Unity MCP 验证真实编译完成、两个方法已生成、Console 错误为零。
详见 ../EditorDiagnostics/Validation.md。

Unity 自动生成工程后，MOBA 的现有源生成器 Analyzer 引用会缺失，导致
SkillCastContextPayloadAccessor 出现两个 CS0535。本轮初次生成工程回归和
读取的 Unity Editor.log 均可见该错误。恢复生成 csproj 中的现有 Analyzer
引用用于 .NET 回归，不代表 Unity 内的源生成器作用域问题已经解决。
该问题属于 Example 的现有代码生成布局，本轮没有移动生成器或改变 Example。

未执行 Unity 窗口交互、UI 渲染、Play Mode 或 Player Build。
生成工程的编译通过不能替代这些验证；源码路径存在也不代表窗口视觉预览通过。
独立 net10.0 工程未完整构建；本机 SDK 为 9.0，未降级工程目标。

复核命令（在 E:\Unity\AbilityKit 执行）：

```powershell
node tools/migration/mirror-editor.cjs verify
node tools/migration/consolidate-assemblies.cjs verify
node tools/migration/mirror-editor.cjs build
```

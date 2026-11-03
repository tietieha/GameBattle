# GameBattle 程序集整理

日期：2026-10-26。主工程：E:\Unity\AbilityKit\Unity。

后续已将 Editor 内容镜像组织到 Editor/Runtime 和 Editor/Unity，
仍保持本阶段的三个框架程序集。最新目录记录见 ../EditorMirroring/Validation.md。
后续的 Unity MCP 接入、MOBA 生成器修复和实际编译验证见 ../EditorDiagnostics/Validation.md。

## 当前边界

```text
Assets/Scripts/GameBattle/
  Runtime/GameBattle.Runtime.asmdef
  Unity/GameBattle.Unity.asmdef
  Editor/GameBattle.Editor.asmdef
  Example/                         原程序集名称保留，不合并
    FrameworkTests/                暂存八组原框架测试
```

Editor -> Unity -> Runtime，Editor 同时直接引用 Runtime。
Runtime 的 noEngineReferences=true，不引用 Unity、Editor 或 Example。
框架三个目录内没有子 asmdef 或 asmref。既有命名空间不改名。

## 已完成

- 原 99 个非测试框架程序集收敛为三个；八个测试程序集保留名称和 GUID。
- 原 Example 的 55 个程序集保留身份，仅更新依赖并清除历史失效引用。
- 三个根程序集复用原 Core、Core.Unity、Base.Editor 的 asmdef GUID。
- 原框架中的编辑器池调试器归入 Editor；测试移入 Example/FrameworkTests。
- 14,854 个本阶段基线文件均已核对去向，原元数据字节和脚本 GUID 保留。
- 项目内技能/触发器资产的程序集字段更新，类型名、命名空间和资源 GUID 保留。
- 旧边界文件归档至 Archive；生成的 csproj 和 Unity.sln 已更新。
- src/Server 下的 .NET 工程仍保持原独立程序集和目标框架，只更新必要的源文件路径。

## 最小兼容修正

- Unity 合并 Runtime 后只编译一份 IsExternalInit；原 .NET 布局保留兼容文件。
- 两处线程 Timer 使用完整类型名，避免与 AbilityKit.Timer 命名空间冲突。
- 默认服务模块反射先查 GameBattle.Runtime，再兼容原 AbilityKit.Ability。
- Compute Jobs / MLAgents 的程序集约束转成文件编译条件；Jobs 和 Svelto 的版本条件保留。
- Compute 测试拥有自己的版本条件，避免依赖其他程序集的局部编译符号。
- MLAgents 桥接改用 Unity C# 9 支持的块命名空间。
- HFSM 测试使用明确的 Diagnostics 类型别名，避免合并后命名空间遮蔽。
- 生成的 MOBA csproj 补上现有 Roslyn 代码生成器引用，不移动或修改生成器。
- 移除不存在的旧 HFSM、TestKit、Protocol.MemoryPack 引用。
- 当前没有 Entities 包，删除失效的 Unity.Entities 引用；DOTS 条件源码保留但未启用。
  将来启用 DOTS 时需安装兼容包并补上 GameBattle.Unity 的 Entities 引用。

## 验证

- verification.json：三个根程序集、框架/Example 身份、原文件/元数据、引用解析、
  源文件归属和工程输入检查。当前 GameBattle 范围没有失效输入。
- builds.json / BuildLogs：十九个生成的 Unity 工程顺序编译，全部零错误。
  包含三个根程序集、八组框架测试、MOBA、Shooter、Tiny、Legacy UnitTests
  和默认 Assembly-CSharp-Editor 的依赖图。
- runtime-binaries.json：检查一个 GameBattle.Runtime 产物，无直接 Unity 程序集引用。
- net9.0 边界检查实际执行七项：默认/注入路径、绝对/空路径、录像读写和日志注入，均通过。
- 编译警告仍存在；NUnit 工程编译通过不代表已在 Unity Test Runner 执行测试。

复核命令（在 E:\Unity\AbilityKit 执行）：

```powershell
node tools/migration/consolidate-assemblies.cjs verify
node tools/migration/consolidate-assemblies.cjs build
dotnet run --project tools/migration/boundary-tests/BoundaryTests.csproj
dotnet run --project tools/migration/boundary-tests/BoundaryTests.csproj -- inspect
```

## 验证限制

本轮没有已连接的 Unity MCP 实例。实际 Unity 重新导入、Console、Play Mode、
冷启动导入和 Player Build 尚未验证。生成工程编译不能替代这些检查。
检查到的七条 ET 工程引用和一条 Godot 样例文件缺失不在本轮范围，没有修改。
本机 SDK 为 9.0，独立 net10.0 工程未执行完整构建，目标框架未降级。
项目之外的已有存档、外部消费者和未启用的可选平台需要单独验证程序集身份迁移。

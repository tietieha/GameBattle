# Unity MCP 编译诊断

日期：2026-10-26。Unity 2022.3.62f3，项目 E:\Unity\AbilityKit\Unity。

## 错误与原因

最初 MCP HTTP 服务未启动，启动后编辑器尚未接入。Unity Editor.log 中有两条
CS0535，指向 SkillCastContextPayloadAccessor 缺少 int/double 两个 TryGet 实现。

该类型的方法由 AbilityKit.Demo.Moba.CodeGen.dll 的 Roslyn 源生成器提供。
DLL 原来位于 Example/Moba/Editor/CodeGen，作用域归入 MOBA Editor，
无法参与 MOBA Runtime 的实际 Unity 编译。手工补生成 csproj 的 Analyzer
引用只能使外部 .NET 构建通过，不能解决 Unity 的程序集生成器作用域。

## 修复

- 将 DLL 及原 .meta 移至 Example/Moba/Runtime/CodeGen。
- 两个原文件 SHA-256 完全一致，原 GUID 和 RoslynAnalyzer 标签保留。
- DLL 的运行平台导入仍关闭，仅参与编译期生成，不成为运行时加载插件。
- 生成器源码和测试源码仍在原 Editor/CodeGen/DotNet~ 目录，未重写实现。
- 不新增程序集，不合并 Example，不修改战斗算法或手工补写生成方法。
- 同步必要的生成工程 DLL 路径；迁移验证工具识别本次修正路由。
- 启动本地 HTTP MCP 服务，通过一次性编辑器连接脚本接入现有 Unity。
  确认接入后已删除临时脚本和 .meta，并通过 MCP 请求重新导入和编译。

## 实际验证

以下结果来自临时脚本移除、重新刷新后的已连接 Unity MCP：

- Application.dataPath = E:/Unity/AbilityKit/Unity/Assets。
- EditorApplication.isCompiling = false。
- EditorApplication.isUpdating = false。
- 运行中的 SkillCastContextPayloadAccessor 有两个 TryGet 方法。
- read_console(types=error, count=100) 返回零条错误；没有执行 Console 清空。
- Editor 镜像完整性和程序集/工程输入检查通过。

证据：

- changes.json：两个文件的迁移路由、原始哈希和工程路径更新清单。
- mcp-runtime-proof.json：项目身份、编辑器状态、实际生成的方法数量。
- mcp-console-final.json：刷新完成后的 Console 错误读取结果。
- mcp-server.stdout.log / mcp-server.stderr.log：本地服务连接日志。

本轮已验证实际 Unity 编译和接入状态，没有执行 Play Mode、Player Build 或
完整 NUnit 测试；零错误不等于零警告。

# Example 提取验证

日期：2026-10-26。目标工程：`E:/Unity/AbilityKit/Unity`。

本报告保留前轮验证范围。后续 Buff 生命周期核心和上下文身份已提取，
当前验证见 `../BuffExtraction/Validation.md`；下文 Buff 保留项描述的是前轮状态。

## 已完成

- 通用源码和外部接入规范：见 `Integration.md`。
- 6 个原源码迁移，原 meta/GUID 保留；新增 6 个框架源码。
- 15 个明确记录的原文件代码/程序集引用改动，原文备份和前后哈希见 `changes.json`。
- `Editor`、`Runtime`、`Unity` 各一个框架程序集，未新增子程序集。
- Example 仅补直接依赖，未合并程序集、修改程序集名称或批量重写业务。
- `src` 原工程保持原目标框架，只更新源码链接和必要的新增 Compile 输入。

## 验证结果

| 验证 | 结果 | 证据 |
| --- | --- | --- |
| 本轮路径、源码哈希和 GUID | 通过 | `verification.json` |
| 原目录迁移/程序集基线 | 通过，14,854 个原文件可追踪 | `../AssemblyConsolidation/verification.json` |
| Editor 镜像与原文件边界 | 通过 | `../EditorMirroring/verification.json` |
| Runtime/Unity/Editor 和代表性样例/测试工程编译 | 19/19 通过 | `builds.json`、`BuildLogs` |
| 定向回归测试 | 39/39 通过 | `TestResults/extraction.trx` |
| Runtime 程序集二进制边界 | 无直接 Unity 引用 | `../AssemblyConsolidation/runtime-binaries.json` |
| Unity 真实编译后 Console | 0 条 Error | `mcp-console-final.json` |
| Unity 工程与已加载类型归属 | 正确，未编译/更新 | `mcp-ownership-proof.json` |
| MOBA Buff 适配烟测 | 共享叠层策略、池化初始化通过 | `mcp-buff-adapter-proof.json` |

39 项测试包括：复用原瞬态战斗数值、表现校正、表现实例注册和固定步长循环的测试；
新增 Buff 策略、程序生成命令、缓存拒绝和外部寻路注入的覆盖。
测试宿主在 `tools/migration/extraction-tests`，使用本机 .NET 9；
测试 Runtime 真实生成 DLL，直接链接纯 C# Unity 调度/注册代码，
没有降级或重建整个 net10 `src` 工程图。

真实 Unity 验证通过 MCP 执行，并确认 `Application.dataPath`，
不是仅凭 IDE csproj 编译推断。验证过程中发现并修复本轮资源类命名空间遮蔽和
两处样例直接程序集引用遗漏；最终结果如上，未清除 Console 来掩盖错误。

## 未验证或保留

- 没有运行 Play Mode 战斗场景、Player Build 或设备测试。
- Unity Resources 默认适配已通过编译，但未验证具体资源加载；
  不声称已有 Addressables 异步后端。
- 没有重写 MOBA 的完整 Buff 生命周期、来源溯源、标签/触发器或全局 Buff。
- 外部寻路测试验证接口注入，不代表已完成 NavMesh 或其它实际后端。
- 新命令来源接口由外部组合根调用，不会自动接管现有样例 AI 和玩家采集循环。
- 全仓 `src` net10 测试未执行：本机使用 .NET 9，且既有 ET/Godot 工程仍有
  8 项任务外的缺失输入，见程序集验证报告。
- 既有编译警告保留，未为目录迁移顺带修改无关的可空性、依赖版本或实现。

## 重复验证

```powershell
node tools/migration/extract-example.cjs verify
node tools/migration/consolidate-assemblies.cjs verify
node tools/migration/mirror-editor.cjs verify
node tools/migration/extract-example.cjs build
dotnet test tools/migration/extraction-tests/ExtractionTests.csproj --no-restore --verbosity quiet --logger "trx;LogFileName=extraction.trx" --results-directory Docs/GameBattle/ExampleExtraction/TestResults
dotnet run --project tools/migration/boundary-tests/BoundaryTests.csproj -- inspect
```

迁移的 `plan`、`move` 是一次性操作，不要重跑；`seal` 是完成授权代码编辑后的哈希封存，
不应拿来覆盖未解释的变化。新增改动应先扩展变更记录并备份，再更新验证结果。

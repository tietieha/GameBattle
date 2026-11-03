# Buff 提取验证

日期：2026-10-26。目标工程：`E:/Unity/AbilityKit/Unity`。

## 已完成

- 新增 9 个 Runtime 源码，修改 11 个既有源码；原 meta/GUID 保留。
- 存储、命令消费、应用/结束流程、生命周期调和、上下文身份和定点计时核心已共享。
- `RuntimeContextId`、`RuntimeContextVersion`、`RuntimeContextIdentity` 归属 `BuffState`。
- MOBA 通过业务端口调用共享实现，不新增独立 Buff 时钟。
- Runtime 保持无 Unity 直接引用；框架仍只有 Runtime、Unity、Editor 三个程序集。

## 验证结果

| 验证 | 结果 | 证据 |
| --- | --- | --- |
| 当前 Buff 源码、元数据与哈希 | 通过 | `verification.json` |
| 前轮迁移、程序集与 Editor 基线 | 4 个阶段验证脚本均通过 | 各阶段 `verification.json` |
| Runtime/Unity/Editor 及代表性样例/测试生成工程编译 | 19/19 通过 | `builds.json`、`BuildLogs` |
| Buff 定向测试 | 51/51 通过 | `TestResults/buff.trx` |
| 前轮提取回归 | 39/39 通过 | `PreviousRegression/extraction.trx` |
| Runtime DLL 边界检查 | 无直接 Unity 程序集引用 | `../AssemblyConsolidation/runtime-binaries.json` |
| Unity MCP Console | 0 条 Error | `mcp-console.json` |
| Unity 内上下文绑定与结束烟测 | 创建、销毁、最终快照保留通过 | `mcp-runtime-proof.json` |

Buff 测试覆盖共享存储、叠层策略、候选失败清理、替换流程、结束清理异常、
命令预算/重入、定点持续时间/间隔；同时复用原 MOBA 上下文恢复和计时回滚预检测试。
测试宿主位于 `tools/migration/buff-tests`，目标 net9，引用真实生成 DLL，
未降级原 `src` net10 工程。

Unity 烟测确认 `Application.dataPath` 为 AbilityKit 工程、没有正在编译/更新，
字段声明类型为 `AbilityKit.Buffs.BuffState`，绑定类型来自 `GameBattle.Runtime`。
MCP CodeDom 不支持显式 `in` 调用，因此烟测用反射调用已编译的 EnsureBuffContext；
这是验证工具兼容处理，不改变工程 API。

## 限制

- 未运行 Play Mode 战斗场景、Player Build 或设备测试。
- 未执行整个 net10 测试图；既有 ET/Godot 缺失输入仍保留在前阶段报告中。
- 全局 Buff 暂缓；MOBA 标签、效果、触发器与快照业务仍留 Example。
- 通用生命周期核心已提取，不宣称所有 Buff 规则都完善或已有完整跨玩法服务。
- 保留既有编译警告，不顺带修改无关实现。

## 重复验证

```powershell
node tools/migration/extract-buff.cjs verify
node tools/migration/extract-example.cjs verify
node tools/migration/consolidate-assemblies.cjs verify
node tools/migration/mirror-editor.cjs verify
node tools/migration/extract-buff.cjs build
dotnet test tools/migration/buff-tests/BuffTests.csproj --no-restore --verbosity quiet --logger "trx;LogFileName=buff.trx" --results-directory Docs/GameBattle/BuffExtraction/TestResults
dotnet test tools/migration/extraction-tests/ExtractionTests.csproj --no-restore --verbosity quiet --logger "trx;LogFileName=extraction.trx" --results-directory Docs/GameBattle/BuffExtraction/PreviousRegression
dotnet run --project tools/migration/boundary-tests/BoundaryTests.csproj -- inspect
```

`plan`、`storage` 是一次性操作，不能重跑。
`seal` 仅用于已解释且授权的代码改动，不能用于掩盖哈希漂移。

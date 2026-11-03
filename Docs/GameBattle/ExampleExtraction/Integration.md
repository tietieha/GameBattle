# GameBattle 外部接入规范

日期：2026-10-26。代码目录：`Unity/Assets/Scripts/GameBattle`。

## 边界

```text
外部调用方 / Example
  玩家操作、AI、自走棋决策、地图、资源后端、具体表现
          |
          v
Unity
  外部时钟 -> 会话循环 -> 逻辑帧 / 网络 / 表现
  表现实例注册表 -> 外部创建 / 更新 / 销毁端口
  Unity 文本资源默认适配
          |
          v
Runtime
  按帧命令缓存 -> 提交战斗命令 -> Tick 世界
  技能 / 效果 / 属性 / Buff 通用策略
  目标 / 移动 -> 外部寻路与碰撞接口
  状态 / 事件 -> 平台无关表现校正决策
```

Runtime 不引用 Unity、Editor 或 Example。Unity 不引用 Example。
业务规则放 Example；不是按游戏类型在框架内增加 Moba、Shooter 分支。
Editor 本轮不新增工具或程序集。技能时间轴仍是可选执行方式。

## 驱动

| 入口 | 所在层 | 框架负责 | 调用方负责 |
| --- | --- | --- | --- |
| `ITimeDriver` | Runtime/Battle/Hosting | Host 生命周期接口 | Start/Stop、帧率、实际时间来源 |
| `IBattleTickDriver<TInput>` | Runtime/Battle/Hosting | 默认实现先 Drain 输入、提交命令，再 Tick 世界 | 构造时提供 submitter、ticker |
| `ISessionTickClock` | Unity/Foundation/Driving | 读取 `NowSeconds` 计算网络流逝时间 | 注入单调时钟，不直接读取 Unity Time |
| `ISessionTickLoopPort` | Unity/Foundation/Driving | 默认循环调度固定逻辑帧、网络和表现 | 实现各 Tick 回调和会话状态 |

`SessionTickLoopController.MainTick(deltaTime)` 的顺序：

1. 无会话时不推进，清除网络时间基准。
2. 使用外部单调时钟执行 `TickTransport`。
3. 固定步长补帧，每次 `TickSimulationFrame` 推进一个逻辑帧。
4. 执行远端驱动和确认状态的推进。
5. 最后执行 `TickPresentation`。

默认每次最多补 5 帧，保留最多 10 帧积压，并记录丢弃时间和无效 delta。
这是已有的客户端预算策略，不是锁步服务器的严格追帧策略。锁步/回放宿主可以直接按帧调用
Runtime 的 Tick，不必采用这个 Unity 会话循环。

宿主必须确保只有一个逻辑帧推进者，暂停、重连、回放切换时负责明确驱动权。
接口不自动解决重复 Tick、线程安全或业务命令合法性。

## 命令来源

新增接口在 `Runtime/Battle/Commands/IBattleCommandSource.cs`：

```csharp
public interface IBattleCommandSource<TCommand>
{
    void CollectCommands(int targetFrame, IBattleCommandSink<TCommand> sink);
}

public interface IBattleCommandSink<in TCommand>
{
    bool TrySubmit(int targetFrame, TCommand command);
}
```

`BattleInputBuffer<TInput>` 已实现提交接口，原有 `Enqueue` API 保留。
泛型命令没有 Keyboard、Mouse、Player 的限制；命令数据类型由调用方定义。
这两个接口是可选的外部组合入口，不是注册后会自动被世界扫描调用的插件。

调用顺序：

```csharp
var buffer = new BattleInputBuffer<AttackCommand>();
IBattleCommandSource<AttackCommand> decision = new AutoChessDecision();

decision.CollectCommands(state.Frame, buffer);
var result = tickDriver.Tick(state, buffer);
```

- 来源在目标帧被 Drain 之前采集。AI 可以在本帧读状态，然后提交本帧执行前的命令；
  也可以提交后续帧命令。
- `TrySubmit=false` 必须由外部处理；例如队列容量不足、负帧号。
- 默认缓存按插入顺序保留同帧命令。多来源的采集顺序由宿主明确，不能依赖不稳定的遍历顺序。
- Runtime 提交入口只缓存，不在采集阶段直接修改战斗世界。
- 网络帧映射、权限、迟到输入、输入验证由对应宿主/同步适配器负责。
- 既有 `IInputDriver` 仍用于现有 Host 的玩家输入协议。程序命令不强制包装成按键。

## 寻路与碰撞

复用 `Runtime/Movement/Navigation/Math/INavigationWorld.cs`：

```csharp
PathStatus FindPath(in Vec3 start, in Vec3 target, float agentRadius, out NavigationPath path);
bool IsWalkable(in Vec3 position, float radius);
bool TryProjectToWalkable(in Vec3 position, float radius, out Vec3 projected);
```

- 结果坐标是世界空间坐标，`NavigationPath.Status` 明确区分完整、部分和失败。
- 路径点数组返回后不能被后端悄悄覆盖；失败返回 `NavigationPath.Failed`。
- 外部实现必须处理 agentRadius，或明确自己支持的半径范围，不得把“不支持”当作已成功规划。
- 默认 `NavigationWorld`/`GridPathfinder` 保留。调用方可以实现 NavMesh、A* 或其它地图后端。
- 新增 `NavigationService(INavigationWorld world)` 构造入口，
  可以把外部寻路器包装为现有 `INavigationService`，再安装进该战斗世界的服务容器。
- `NavigationService.Dispose()` 仅清除引用，不销毁调用方拥有的寻路器；
  `Rebuild(grid)` 会切回默认网格实现。使用外部后端时不要再调用它。
- MOBA 的地图数据和 `MobaNavigationBake` 仍是样例实现。
  替换后端时在组合根安装自己的 `INavigationService`，不要同时安装同 ID 的 MOBA 默认服务。
- 不直接把 `NavMeshAgent`、Transform 或 Unity 的路径对象传入 Runtime。

碰撞继续复用 `ICollisionWorld`、`IMotionCollisionWorld` 和相应的 Sweep 扩展接口。
不是所有战斗都必须具备导航/碰撞；只有启用这些模块的调用方需要安装对应实现。

## 资源

通用接口：`Runtime/Content/Resources/ITextResourceProvider.cs`。

- `LoadText`：缺失时抛 `FileNotFoundException`。
- `TryLoadText`：缺失时返回 false，content 为 null；其它后端错误不伪装为缺失。
- `Exists`：按相同逻辑 key 查询。
- 路径是后端逻辑 key，不由 Runtime 自动拼接磁盘路径。

Unity 默认实现：`Unity/Content/Resources/UnityTextResourceProvider.cs`。
使用 `Resources.Load<TextAsset>`，调用方传入相对 Resources 目录、无扩展名的 key。
这是新增的同步默认适配，不包含 Addressables 异步资源所有权管理，也不自动设置样例全局加载器。
Addressables、AssetBundle、磁盘和服务器调用方自行实现该接口。
Resources API 的调用应留在 Unity 主线程。

样例 `IResourceProvider` 现在继承通用接口，保留旧 API 与原有实现兼容。
旧样例的全局 `ResourceProviders` 仍留 Example，不迁进 Runtime；
新业务应在组合根注入，不引入框架级静态资源定位器。

## 表现

通用校正数据和 `PresentationCueReconciliationController<TKey, TPayload>` 放
`Runtime/Battle/Presentation`：纯数据决策，不持有 GameObject。

`PresentationCueViewRegistry<TKey, TPayload, THandle>` 放 `Unity/Presentation/Cues`：
管理实例句柄，创建、更新、销毁由外部实现
`IPresentationCueViewPort<TKey, TPayload, THandle>`。

- 不要求使用 MOBA 的 key、payload 或 VFX 实例类型。
- `TryCreate=false` 表示本次创建失败，可由后续 EnsureActive 重试。
- `IsAlive=false` 表示句柄失效，下次 EnsureActive 会重建。
- Destroy 抛异常时该项仍保留，可重试清理；Clear 聚合失败，不吞掉资源泄漏。
- 场景切换和对局结束时，调用方负责切换 generation，并清理旧表现实例。
- MOBA 的协议 cue 解码、技能动画、实体位置映射、VFX 资源选择仍在 Example。

## Buff

本节记录前轮仅提取状态/策略时的范围；后续已扩大到存储、命令、生命周期核心、
上下文身份与定点计时，当前接入规范见 `../BuffExtraction/Integration.md`。
下文“留在 Example”的生命周期内容以该后续文档为准。

通用部分放 `Runtime/Buff`：

```text
State/BuffState.cs
  BuffId / SourceId / SourceContextId
  StackCount / Remaining / IntervalRemainingSeconds

Policies/BuffStackingSettings.cs
  StackingPolicy / RefreshPolicy / MaxStacks / IntervalMs
  ApplyOutcome / ApplyResult

Policies/BuffStackingPolicyApplier.cs
  Initialize / ApplyToExisting / ResetInterval
```

MOBA `BuffRuntime` 继承 `BuffState`，保留原字段访问方式。
MOBA 的策略执行器改为配置转换、租用对象池和持续行为间隔同步的薄适配，
叠层/刷新算法只有 Runtime 一份。

- None、忽略、替换、加层、刷新，以及保留/重置/累加持续时间保持原行为。
- 非正 MaxStacks 仍表示不限制层数。
- 策略只改状态，不产生效果、不绑定实体、不注册触发器、不推进时间。
- 调用方负责 Buff 容器、实例匹配、添加/移除、效果执行、回收和生命周期驱动。
- 状态沿用原实现的 float 秒数，本轮不顺带改变时间精度或回滚格式。

**这次不是完整的通用 BuffService。**
MOBA 的来源追踪、上下文注册、技能运行时保留、标签、持续行为和 Entitas 生命周期仍在 Example。
通用状态本身没有限定必须挂 Actor，但不等于框架已经实现全局 Buff 容器和广播；
完整的实体/队伍/全局作用域仍需要后续设计。

## 提取清单

| 内容 | 新位置 | 处理方式 |
| --- | --- | --- |
| Buff 基础状态、叠层/刷新/初始化 | Runtime/Buff | 提取共享算法，MOBA 调用薄适配 |
| 战斗瞬态定点数值、修饰符和句柄 | Runtime/Effect/Calculation | 原文件迁移；命名空间改为 `AbilityKit.Combat.Calculation` |
| 表现预测/确认/纠正/拒绝决策 | Runtime/Battle/Presentation | 原文件迁移，平台无关 |
| 文本资源契约 | Runtime/Content/Resources | 从样例契约提取；旧契约继承兼容 |
| 表现实例注册、清理和生命周期策略 | Unity/Presentation/Cues | 两个原文件迁移，渲染实现留调用方 |
| 固定步长会话循环、外部时钟/端口和状态投影 | Unity/Foundation/Driving | 两个原文件迁移 |
| Unity 文本资源后端 | Unity/Content/Resources | 新增默认实现 |
| 按帧命令来源/提交契约 | Runtime/Battle/Commands | 新增接口，复用现有缓存与 Tick |
| 外部寻路服务包装 | Runtime/Movement/Navigation | 为原默认服务补注入构造入口 |

没有迁入 Runtime 的内容：

- MOBA 的伤害管线阶段、护盾规则、治疗条件、战斗事务：带有 Actor、触发器和业务配置依赖。
- MOBA 的 Buff 事件、标签、技能关联和完整生命周期。
- MOBA 地图配置、烘焙规则、英雄 AI、技能选目标策略。
- Shooter 枪械/弹药规则、Unity Jobs 样例实现。
- 房间大厅、示例目录、样例全局服务定位器和测试工具。
- 网络协议 cue 到实际角色/特效的映射。

原迁移的 6 个源码文件及其 meta/GUID 保留，新文件独立生成 meta。
完整逐文件路径和代码变更记录见 `changes.json`，备份见 `Backups`。
部分迁移类型保留历史 `AbilityKit.Game.Flow` 命名空间，以减少样例接口改动；
目录和程序集已经归框架所有，不再因此依赖 Example。

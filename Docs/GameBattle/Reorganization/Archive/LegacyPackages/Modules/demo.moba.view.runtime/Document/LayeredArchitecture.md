# MOBA Demo 层级架构文档

> **阅读对象**：希望理解 MOBA Demo 项目层级架构的开发者
>
> **文档目标**：明确 `moba.runtime`、`moba.view` 和 `Console Demo` 三个组件的定位和职责边界

---

## 一、整体架构

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         MOBA Demo 整体架构                                    │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                       Unity 完整客户端                                │  │
│   │                                                                       │  │
│   │   ┌───────────────────────┐     ┌───────────────────────┐          │  │
│   │   │   moba.runtime        │     │   moba.view           │          │  │
│   │   │   (逻辑层)            │     │   (表现层)            │          │  │
│   │   │                       │     │                       │          │  │
│   │   │  • 伤害计算           │     │  • 渲染实体          │          │  │
│   │   │  • 技能执行           │────▶│  • 播放特效          │          │  │
│   │   │  • Buff/弹道         │事件 │  • 显示飘字          │          │  │
│   │   │  • 帧同步快照         │     │  • 位置插值          │          │  │
│   │   │  • 状态管理           │     │  • HUD 显示          │          │  │
│   │   └───────────────────────┘     └───────────────────────┘          │  │
│   │                                                                       │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                       Console Demo                                   │  │
│   │                                                                       │  │
│   │   ┌─────────────────────────────────────────────────────────────┐   │  │
│   │   │              Console Demo (简化实现)                          │   │  │
│   │   │                                                             │   │  │
│   │   │   逻辑层职责 + 表现层职责 混合在单一进程中                     │   │  │
│   │   │   用于快速验证和调试                                          │   │  │
│   │   └─────────────────────────────────────────────────────────────┘   │  │
│   │                                                                       │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 二、三个组件的定位

### 2.1 moba.runtime（逻辑层）

| 属性 | 说明 |
|-----|------|
| **包名** | `com.abilitykit.demo.moba.runtime` |
| **类型** | Unity Runtime Package |
| **定位** | 游戏逻辑核心，不依赖 Unity Engine |
| **依赖** | AbilityKit 框架核心包 |

#### 核心职责

| 职责 | 说明 |
|-----|------|
| **伤害计算** | 基础伤害、护甲减免、暴击、穿甲等 |
| **技能流程执行** | 技能管线（吟唱、弹道、命中等） |
| **Buff 管理** | Buff 添加、叠加、持续时间、移除 |
| **弹道/投射物** | 子弹移动、碰撞检测、追踪逻辑 |
| **区域效果** | AOE 区域、持续伤害、条件触发 |
| **目标搜索** | 范围搜索、优先级规则、过滤条件 |
| **帧同步快照** | 收集状态快照，供给网络层 |
| **回滚支持** | 预测错误时的状态回滚 |

#### 关键组件

```
Services/
├── Skill/
│   ├── Cast/SkillExecutor.cs              # 技能释放执行器
│   ├── Effects/MobaEffectExecutionService.cs  # 效果执行
│   └── Pipeline/                          # 技能管线
├── Buffs/MobaBuffService.cs               # Buff 管理
├── Projectile/MobaProjectileService.cs    # 弹道管理
├── Actor/                                # 角色服务
└── Snapshot/                             # 快照服务
    ├── MobaSnapshotRouter.cs             # 快照路由
    ├── MobaActorTransformSnapshotService.cs
    ├── MobaDamageEventSnapshotService.cs
    └── ...
```

#### 事件接口设计

```csharp
// 事件应携带完整状态，表现层无需计算
public readonly struct DamageEvent
{
    public int TargetId { get; }
    public int SourceId { get; }
    public float Damage { get; }
    public float CurrentHp { get; }       // 最终 HP
    public float MaxHp { get; }
    public bool IsDead { get; }           // 死亡判定
    public bool IsCritical { get; }
}
```

---

### 2.2 moba.view（表现层）

| 属性 | 说明 |
|-----|------|
| **包名** | `com.abilitykit.demo.moba.view.runtime` |
| **类型** | Unity Runtime Package |
| **定位** | Unity 视图渲染，依赖 Unity Engine |
| **依赖** | moba.runtime、Unity Engine |

#### 核心职责

| 职责 | 说明 |
|-----|------|
| **实体渲染** | GameObject 创建/销毁、模型切换 |
| **特效管理** | VFX 播放、特效池化管理 |
| **飘字系统** | 伤害数字、加血数字、状态文字 |
| **位置插值** | 网络延迟补偿、平滑移动 |
| **HUD 显示** | 血条、技能图标、地图、小地图 |
| **区域显示** | 火圈、结界等视觉效果 |
| **输入采集** | 键盘、鼠标、触摸输入 |
| **游戏流程** | 分层状态机管理（Loading、Battle 等） |

#### 关键组件

```
Runtime/Game/
├── App/
│   ├── Entry/                         # Unity 入口
│   └── Flow/GameFlowDomain.cs         # 根状态机
└── Battle/
    ├── Client/Session/Features/       # 会话与帧同步客户端
    ├── Input/Sources/                 # 输入采集
    ├── Input/Mapping/                 # 输入命令映射
    ├── Input/Submission/              # 输入提交
    ├── Presentation/Features/View/    # 视图系统
    ├── Presentation/Hud/              # HUD
    ├── Presentation/Vfx/              # 特效管理
    ├── Presentation/View/             # 实体绑定
    └── Shared/Context/                # 战斗上下文
```

#### 与逻辑层交互

```csharp
// IBattleViewEventSink - 接收逻辑层事件
public interface IBattleViewEventSink
{
    void OnDamage(DamageEvent evt);      // 只做渲染
    void OnHeal(HealEvent evt);         // 只做渲染
    void OnBuffApplied(BuffAppliedEvent evt);
    void OnEntityDestroyed(EntityDestroyedEvent evt);
}

// 事件应包含渲染所需的一切数据
// 表现层不应做任何计算
```

---

### 2.3 Console Demo（简化实现）

| 属性 | 说明 |
|-----|------|
| **项目路径** | `src/AbilityKit.Demo.Moba.Console` |
| **类型** | .NET Console Application |
| **定位** | 快速验证、自动化测试、调试 |
| **依赖** | AbilityKit 框架（无 Unity） |

#### 核心职责

| 职责 | 说明 |
|-----|------|
| **逻辑层职责** | 技能执行、伤害计算、Buff 管理 |
| **表现层职责** | 控制台输出、日志显示 |
| **自动化测试** | AutoTestRunner、测试场景 |
| **帧同步验证** | 帧同步逻辑验证 |

#### 目录结构

```
src/AbilityKit.Demo.Moba.Console/
├── AutoTest/           # 自动测试
├── Battle/             # 战斗功能
├── Bootstrap/          # 启动器
├── Configs/            # 配置文件
├── Core/               # 核心组件
├── Flow/               # 流程控制
├── MobaCore/           # MOBA 核心（逻辑层职责）
├── Services/           # 服务层
├── View/               # 视图层
└── Platform/           # 平台抽象
```

#### 简化说明

Console Demo 是**逻辑层和表现层混合**的简化实现：

- 技能执行在 `MobaCore/MobaCoreSkillExecutor.cs`
- 伤害计算在 `Services/SkillExecutor.cs`（简化版）
- 视图输出到控制台日志

---

## 三、依赖关系图

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           依赖关系图                                         │
│                                                                             │
│   AbilityKit Framework                                                       │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │  com.abilitykit.core | ability | triggering | combat | hfsm | ...   │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                      ▲                                      │
│                                      │                                      │
│           ┌──────────────────────────┴──────────────────────────┐          │
│           │                                                      │          │
│   ┌───────▼───────────┐                               ┌────────▼────────┐  │
│   │  moba.runtime    │                               │   moba.view    │  │
│   │  (逻辑层)        │                               │   (表现层)     │  │
│   │                  │                               │                │  │
│   │  依赖 Framework  │                               │  依赖 Framework│  │
│   │  无 Unity 依赖   │                               │  依赖 Unity    │  │
│   │                  │  事件/快照                    │                │  │
│   └──────────────────┘ ─────────────────────────────▶  └────────────────┘  │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                         Console Demo                                 │  │
│   │                                                                       │  │
│   │   依赖 moba.runtime 的核心服务                                        │  │
│   │   自包含逻辑层 + 表现层（控制台输出）                                 │  │
│   │                                                                       │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 四、数据流

### 4.1 Unity 完整客户端数据流

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           数据流                                             │
│                                                                             │
│   逻辑层 (moba.runtime)                                                     │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                                                                       │  │
│   │   玩家输入 ──▶ 技能执行 ──▶ 伤害计算 ──▶ Buff/弹道更新              │  │
│   │                                                        │             │  │
│   │                                                        ▼             │  │
│   │                                              事件发布 (DamageEvent)  │  │
│   │                                                        │             │  │
│   │                                                        ▼             │  │
│   │                                              快照收集 (Snapshot)    │  │
│   │                                                                       │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│   表现层 (moba.view)                                                            │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                                                                       │  │
│   │   事件接收 ──▶ 特效播放 ──▶ 飘字显示                                │  │
│   │                                                                       │  │
│   │   快照解析 ──▶ 实体同步 ──▶ 位置插值 ──▶ 渲染更新                  │  │
│   │                                                                       │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│   单向数据流：逻辑层计算 ──▶ 事件/快照 ──▶ 表现层渲染                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Console Demo 数据流

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Console Demo 数据流                                   │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                                                                       │  │
│   │   ConsoleInputFeature                                                │  │
│   │         │                                                            │  │
│   │         ▼                                                            │  │
│   │   MobaCoreSkillExecutor  ──▶ 逻辑执行 ──▶ 伤害计算                   │  │
│   │         │                      │                                      │  │
│   │         │                      ▼                                      │  │
│   │         │               事件发布 (BattleEventBus)                     │  │
│   │         │                      │                                      │  │
│   │         │                      ▼                                      │  │
│   │         │               ConsoleViewEventSink                         │  │
│   │         │                      │                                      │  │
│   │         │                      ▼                                      │  │
│   │         │               ConsoleBattleView ──▶ Log 输出               │  │
│   │         │                                                            │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│   注意：Console Demo 中逻辑层和表现层在同一个进程中                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 五、职责边界总结

### 5.1 逻辑层专属

| 职责 | 说明 |
|-----|------|
| 伤害计算 | 护甲、穿甲、暴击、伤害类型 |
| HP 管理 | 扣血、加血、上限 |
| 死亡判定 | HP ≤ 0 时触发死亡 |
| Buff 逻辑 | 添加、移除、叠加规则 |
| 冷却管理 | 技能冷却同步 |
| 目标搜索 | 范围、优先级、过滤 |
| 弹道逻辑 | 移动、碰撞、追踪 |
| 技能流程 | 吟唱、引导、释放 |

### 5.2 表现层专属

| 职责 | 说明 |
|-----|------|
| 实体渲染 | GameObject 创建/销毁 |
| 特效播放 | VFX、粒子、音效 |
| 飘字显示 | 伤害数字、文字浮动 |
| 位置插值 | 网络平滑、预测 |
| HUD 更新 | 血条、图标、地图 |
| 输入采集 | 键盘、鼠标、触摸 |
| 区域特效 | 火圈、结界显示 |

### 5.3 禁止交叉

| 逻辑层禁止 | 表现层禁止 |
|-----------|-----------|
| ❌ 直接操作 GameObject | ❌ 计算伤害值 |
| ❌ 播放 Unity 音效 | ❌ 修改 HP 值 |
| ❌ 访问 Unity 资源 | ❌ 判定死亡 |
| ❌ 控制 Unity 动画 | ❌ 管理冷却 |

---

## 六、事件设计规范

### 6.1 原则

1. **事件携带完整状态** - 渲染所需的所有数据都应在事件中
2. **表现层不做计算** - 事件应包含最终结果，而非原始数据
3. **单向数据流** - 事件只从逻辑层流向表现层

### 6.2 正确示例

```csharp
// ✅ 正确：事件携带完整状态
public readonly struct DamageEvent
{
    public int TargetId { get; }
    public float Damage { get; }
    public float CurrentHp { get; }        // 逻辑层计算好的最终 HP
    public float MaxHp { get; }
    public bool IsDead { get; }            // 逻辑层判定好的死亡状态
}

// ✅ 正确：表现层只渲染
private void OnDamage(DamageEvent evt)
{
    ShowFloatingText(evt.TargetId, $"-{evt.Damage:F0}");
    UpdateHpBar(evt.TargetId, evt.CurrentHp, evt.MaxHp);
    if (evt.IsDead)
    {
        ShowDeathEffect(evt.TargetId);
    }
}
```

### 6.3 错误示例

```csharp
// ❌ 错误：表现层计算最终状态
private void OnDamage(DamageEvent evt)
{
    var entity = GetEntity(evt.TargetId);
    var finalHp = entity.Hp - evt.Damage;  // ❌ 越权计算
    UpdateHpBar(evt.TargetId, finalHp, entity.MaxHp);
}
```

---

## 七、快速参考

| 组件 | 包/路径 | 职责 |
|-----|---------|------|
| moba.runtime | `com.abilitykit.demo.moba.runtime/Runtime/Scripts/` | 逻辑层核心 |
| moba.view | `com.abilitykit.demo.moba.view.runtime/Runtime/Game/` | 表现层核心 |
| Console Demo | `src/AbilityKit.Demo.Moba.Console/` | 简化实现 |
| 逻辑层事件 | `moba.runtime` 内定义 | 携带完整状态 |
| 表现层接口 | `moba.view` 内定义 | 只做渲染 |

---

*文档版本：1.0*
*最后更新：2026-05-17*

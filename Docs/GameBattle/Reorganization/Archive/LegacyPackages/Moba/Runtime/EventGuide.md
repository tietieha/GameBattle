# 事件系统规范

本文档定义了 moba.runtime 战斗逻辑世界层的事件系统标准模式。

## 1. 概述

事件系统采用**字符串键 + 双重发布**模式：
- 字符串键提供灵活的事件标识
- 双重发布支持类型安全和通用订阅

## 2. 事件架构

```
┌─────────────────────────────────────────────────────────┐
│ IEventBus                                                │
│                                                          │
│ Publish(EventKey<T>, in T payload)  ← 类型安全订阅      │
│ Publish(EventKey<object>, in object)  ← 通用订阅         │
└─────────────────────────────────────────────────────────┘
```

## 3. 事件常量定义

### 3.1 事件常量类

```csharp
namespace AbilityKit.Demo.Moba.Services
{
    /// <summary>
    /// 游戏事件常量
    /// 遵循 "{子系统}.{事件名}" 命名规范
    /// </summary>
    public static class GameEvents
    {
        // ========== 战斗事件 ==========
        /// <summary>伤害创建</summary>
        public const string Combat_AttackCreated = "combat.attack_created";
        /// <summary>伤害计算前</summary>
        public const string Combat_BeforeCalc = "combat.before_calc";
        /// <summary>伤害计算开始</summary>
        public const string Combat_CalcBegin = "combat.calc_begin";
        /// <summary>伤害计算结束</summary>
        public const string Combat_CalcFinal = "combat.calc_final";
        /// <summary>伤害应用后</summary>
        public const string Combat_AfterApply = "combat.after_apply";

        // ========== 技能事件 ==========
        /// <summary>技能释放</summary>
        public const string Skill_Cast = "skill.cast";
        /// <summary>技能冷却开始</summary>
        public const string Skill_CooldownStart = "skill.cooldown_start";
        /// <summary>技能冷却结束</summary>
        public const string Skill_CooldownEnd = "skill.cooldown_end";

        // ========== Buff 事件 ==========
        /// <summary>Buff 应用</summary>
        public const string Buff_Apply = "buff.apply";
        /// <summary>Buff 移除</summary>
        public const string Buff_Remove = "buff.remove";
        /// <summary>Buff Tick</summary>
        public const string Buff_Tick = "buff.tick";

        // ========== 投射物事件 ==========
        /// <summary>投射物生成</summary>
        public const string Projectile_Spawn = "projectile.spawn";
        /// <summary>投射物击中</summary>
        public const string Projectile_Hit = "projectile.hit";
        /// <summary>投射物离开</summary>
        public const string Projectile_Exit = "projectile.exit";

        // ========== 单位事件 ==========
        /// <summary>单位死亡</summary>
        public const string Unit_Die = "unit.die";
        /// <summary>单位重生</summary>
        public const string Unit_Reborn = "unit.reborn";
    }
}
```

### 3.2 事件 ID 转换

使用 `TriggeringIdUtil.GetEventEid()` 将字符串转换为 `StableStringId`：

```csharp
var eid = TriggeringIdUtil.GetEventEid(GameEvents.Combat_AttackCreated);
```

## 4. 事件发布模板

### 4.1 发布事件

```csharp
public void PublishDamage(AttackInfo attack)
{
    // 1. 获取事件 ID
    var eid = TriggeringIdUtil.GetEventEid(GameEvents.Combat_AttackCreated);

    // 2. 发布类型安全事件（类型安全消费者）
    _eventBus.Publish(new EventKey<AttackInfo>(eid), in attack);

    // 3. 发布对象事件（通用消费者）
    object boxed = attack;
    _eventBus.Publish(new EventKey<object>(eid), in boxed);
}
```

### 4.2 发布多个事件

```csharp
public void PublishDamageEvents(AttackCalcInfo calc, DamageResult result)
{
    var eid = TriggeringIdUtil.GetEventEid(GameEvents.Combat_CalcBegin);

    // 发布计算开始事件
    _eventBus.Publish(new EventKey<AttackCalcInfo>(eid), in calc);
    _eventBus.Publish(new EventKey<object>(eid), in calc);

    // 发布最终结果
    var finalEid = TriggeringIdUtil.GetEventEid(GameEvents.Combat_CalcFinal);
    _eventBus.Publish(new EventKey<AttackCalcInfo>(eid), in calc);
    _eventBus.Publish(new EventKey<object>(eid), in calc);
}
```

## 5. 事件订阅模板

### 5.1 类型安全订阅

```csharp
public class DamageEventHandler : IService
{
    private readonly IEventBus _eventBus;
    private IDisposable _sub;

    public DamageEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public void Subscribe()
    {
        var eid = TriggeringIdUtil.GetEventEid(GameEvents.Combat_AttackCreated);
        _sub = _eventBus.Subscribe(new EventKey<AttackInfo>(eid), HandleAttack);
    }

    private void HandleAttack(AttackInfo attack)
    {
        Log.Info($"[DamageEventHandler] Attack: {attack.AttackerActorId} -> {attack.TargetActorId}");
    }
}
```

### 5.2 通用订阅

```csharp
public class GeneralEventHandler : IService
{
    private readonly IEventBus _eventBus;
    private IDisposable _sub;

    public void Subscribe()
    {
        // 订阅所有 combat.* 事件
        _sub = _eventBus.SubscribeAll("combat.", HandleEvent);
    }

    private void HandleEvent(EventKey<object> key, object payload)
    {
        Log.Info($"[GeneralEventHandler] Event: {key.EventId} received");
    }
}
```

## 6. 事件负载类型

### 6.1 定义负载

```csharp
public readonly struct DamagePayload
{
    public int AttackerActorId { get; }
    public int TargetActorId { get; }
    public float DamageValue { get; }
    public DamageType DamageType { get; }

    public DamagePayload(
        int attackerActorId,
        int targetActorId,
        float damageValue,
        DamageType damageType)
    {
        AttackerActorId = attackerActorId;
        TargetActorId = targetActorId;
        DamageValue = damageValue;
        DamageType = damageType;
    }
}
```

### 6.2 负载命名规范

| 前缀 | 用途 | 示例 |
|-----|------|------|
| `*Event` | 事件负载 | `DamageEvent`, `SpawnEvent` |
| `*Payload` | 数据负载 | `DamagePayload`, `TransformPayload` |
| `*Context` | 上下文 | `SkillCastContext`, `TriggerContext` |

## 7. 事件注册

使用 `MobaEventSubscriptionRegistry` 注册事件类型：

```csharp
public void Configure(WorldContainerBuilder builder)
{
    builder.TryRegister<MobaEventSubscriptionRegistry>(WorldLifetime.Singleton, _ =>
    {
        var reg = new MobaEventSubscriptionRegistry();

        // 精确注册
        reg.RegisterExact<AttackInfo>(GameEvents.Combat_AttackCreated);
        reg.RegisterExact<AttackCalcInfo>(GameEvents.Combat_CalcBegin);

        // 前缀注册（通配符）
        reg.RegisterPrefix<SkillCastContext>("skill.");
        reg.RegisterPrefix<BuffEventArgs>("buff.");

        return reg;
    });
}
```

## 8. 最佳实践

### 8.1 事件命名规范

```
{子系统}.{动作}.{对象}

示例：
- combat.attack_created     ✓
- combat.damage_calc        ✓
- skill.cast.start          ✓
- buff.apply                ✓
- unit_die                  ✗ (应使用点号分隔)
```

### 8.2 避免频繁事件

```csharp
// ❌ 错误：每帧发送位置更新
public void OnTick()
{
    Publish("unit.position", new PositionPayload(x, y)); // 性能问题！
}

// ✓ 正确：批量收集，按帧发送
public void OnTick()
{
    _pendingPositions.Add(new PositionPayload(x, y));
}

public void OnFrameEnd()
{
    if (_pendingPositions.Count > 0)
    {
        Publish("unit.position_batch", _pendingPositions);
        _pendingPositions.Clear();
    }
}
```

### 8.3 事件幂等性

```csharp
public void OnDeath(int actorId)
{
    // 确保死亡事件只发送一次
    if (_deadActors.Contains(actorId))
        return;

    _deadActors.Add(actorId);
    Publish("unit.die", new DeathPayload(actorId));
}
```

## 9. 禁止事项

| 禁止 | 说明 | 正确做法 |
|-----|------|---------|
| ❌ 在事件处理器中修改触发事件的数据 | 可能导致无限循环 | 只读事件数据 |
| ❌ 发送过多事件 | 性能问题 | 批量收集，按帧发送 |
| ❌ 事件处理器中执行耗时操作 | 阻塞主循环 | 使用异步或分帧处理 |
| ❌ 事件处理器中触发相同类型事件 | 可能导致递归 | 使用命令模式替代 |

## 10. 新建事件检查清单

创建新事件时，确保：

- [ ] 在 `GameEvents` 类中添加常量
- [ ] 定义事件负载类型（Payload/Context）
- [ ] 使用 `TriggeringIdUtil.GetEventEid()` 获取事件 ID
- [ ] 双重发布（类型安全 + 对象）
- [ ] 在 `MobaEventSubscriptionRegistry` 中注册
- [ ] 添加日志输出

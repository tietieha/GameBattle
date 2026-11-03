# 快照同步规范

本文档定义了 moba.runtime 战斗逻辑世界层的快照（Snapshot）同步标准模式。

## 1. 概述

快照是游戏状态的序列化表示，用于网络同步和回放。快照系统采用**路由器（Router）模式**，聚合多个快照提供者。

## 2. 架构

```
┌─────────────────────────────────────────────────────────┐
│ MobaSnapshotRouter (IWorldStateSnapshotProvider)         │
│                                                          │
│ Aggregates:                                               │
│ ├── MobaEnterGameSnapshotService   (初始游戏状态)         │
│ ├── MobaActorSpawnSnapshotService  (实体生成)             │
│ ├── MobaActorDespawnSnapshotService (实体销毁)            │
│ ├── MobaActorTransformSnapshotService (位置同步)           │
│ ├── MobaProjectileEventSnapshotService (投射物事件)         │
│ ├── MobaAreaEventSnapshotService (区域事件)               │
│ ├── MobaDamageEventSnapshotService (伤害事件)             │
│ └── MobaStateHashSnapshotService (状态哈希)              │
└─────────────────────────────────────────────────────────┘
```

## 3. 快照提供者接口

### 3.1 IWorldStateSnapshotProvider

```csharp
public interface IWorldStateSnapshotProvider
{
    /// <summary>
    /// 尝试获取指定帧的快照
    /// </summary>
    bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot);
}
```

## 4. 快照路由器模板

### 4.1 基本结构

```csharp
[WorldService(typeof(IGameSnapshotRouter))]
[WorldService(typeof(IWorldStateSnapshotProvider))]
public sealed class GameSnapshotRouter : IWorldStateSnapshotProvider
{
    private readonly List<IWorldStateSnapshotProvider> _providers = new();

    public GameSnapshotRouter(
        MobaActorSpawnSnapshotService spawn,
        MobaActorTransformSnapshotService transform,
        /* ... 其他 provider ... */)
    {
        // 按优先级添加
        _providers.Add(spawn);
        _providers.Add(transform);
    }

    public bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot)
    {
        // 按顺序检查每个提供者
        foreach (var p in _providers)
        {
            if (p.TryGetSnapshot(frame, out snapshot))
                return true;
        }
        snapshot = default;
        return false;
    }
}
```

## 5. 单个快照服务模板

### 5.1 服务定义

```csharp
[WorldService(typeof(MobaActorTransformSnapshotService))]
public sealed class MobaActorTransformSnapshotService : IWorldStateSnapshotProvider
{
    private readonly MobaEntityManager _entities;
    private readonly List<ActorTransformData> _pending = new();
    private FrameIndex _lastFrame;

    public MobaActorTransformSnapshotService(MobaEntityManager entities)
    {
        _entities = entities;
    }

    public bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot)
    {
        // 帧去重：避免同一帧生成多个快照
        if (frame.Value == _lastFrame.Value)
        {
            snapshot = default;
            return false;
        }
        _lastFrame = frame;

        // 收集数据
        _pending.Clear();
        CollectTransforms(_pending);

        // 生成快照
        if (_pending.Count == 0)
        {
            snapshot = default;
            return false;
        }

        var payload = new ActorTransformSnapshotPayload(_pending.ToArray());
        snapshot = new WorldStateSnapshot(frame, SnapshotTypes.ActorTransform, payload);
        return true;
    }

    private void CollectTransforms(List<ActorTransformData> results)
    {
        // 遍历实体收集变换数据
    }
}
```

### 5.2 快照负载

```csharp
public readonly struct ActorTransformSnapshotPayload
{
    public readonly ActorTransformData[] Transforms;

    public ActorTransformSnapshotPayload(ActorTransformData[] transforms)
    {
        Transforms = transforms;
    }

    public byte[] Serialize()
    {
        // 序列化逻辑
    }
}
```

## 6. 帧去重

每个快照服务必须实现帧去重：

```csharp
private FrameIndex _lastFrame;

public bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot)
{
    // 跳过同一帧的重复请求
    if (frame.Value == _lastFrame.Value)
    {
        snapshot = default;
        return false;
    }
    _lastFrame = frame;
    // ...
}
```

## 7. 快照序列化

### 7.1 序列化接口

```csharp
public interface ISnapshotCodec<TPayload>
{
    byte[] Encode(TPayload payload);
    TPayload Decode(byte[] data);
}
```

### 7.2 使用示例

```csharp
public class ActorTransformSnapshotCodec : ISnapshotCodec<ActorTransformSnapshotPayload>
{
    public byte[] Encode(ActorTransformSnapshotPayload payload)
    {
        using var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);
        writer.Write(payload.Transforms.Length);
        foreach (var t in payload.Transforms)
        {
            writer.Write(t.ActorId);
            writer.Write(t.X);
            writer.Write(t.Y);
            writer.Write(t.Rotation);
        }
        return stream.ToArray();
    }
}
```

## 8. 事件驱动快照

快照也可以通过事件触发：

```csharp
public void OnActorSpawn(int actorId, float x, float y)
{
    _pending.Add(new ActorSpawnData(actorId, x, y));
}

public bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot)
{
    if (_pending.Count == 0)
    {
        snapshot = default;
        return false;
    }

    var payload = new ActorSpawnPayload(_pending.ToArray());
    _pending.Clear();
    snapshot = new WorldStateSnapshot(frame, SnapshotTypes.ActorSpawn, payload);
    return true;
}
```

## 9. 最佳实践

### 9.1 分离快照类型

| 类型 | 说明 | 示例 |
|-----|------|------|
| 状态快照 | 游戏状态完整副本 | EnterGameSnapshot |
| 增量快照 | 状态变化部分 | Transform, Damage |
| 事件快照 | 触发的事件 | Spawn, Despawn, ProjectileHit |

### 9.2 压缩优化

```csharp
public byte[] Serialize()
{
    // 使用增量编码减少数据量
    // 例如：位置只发送变化量，而非完整值
}
```

### 9.3 快照合并

```csharp
public bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot)
{
    // 如果有多个快照类型，合并为一个
    var transforms = CollectTransforms();
    var damages = CollectDamages();

    if (transforms.Count == 0 && damages.Count == 0)
    {
        snapshot = default;
        return false;
    }

    var payload = new CombinedSnapshotPayload(transforms, damages);
    snapshot = new WorldStateSnapshot(frame, SnapshotTypes.Combined, payload);
    return true;
}
```

## 10. 新建快照服务检查清单

创建新快照服务时，确保：

- [ ] 实现 `IWorldStateSnapshotProvider` 接口
- [ ] 使用 `[WorldService]` 特性注册
- [ ] 实现帧去重（`_lastFrame` 检查）
- [ ] 定义快照负载类型（Payload）
- [ ] 实现 `Serialize()` 方法
- [ ] 在 Router 中注册
- [ ] 添加日志输出

## 11. 快照类型常量

```csharp
public static class SnapshotTypes
{
    public const string EnterGame = "enter_game";
    public const string ActorSpawn = "actor_spawn";
    public const string ActorDespawn = "actor_despawn";
    public const string ActorTransform = "actor_transform";
    public const string ActorDamage = "actor_damage";
    public const string ActorDead = "actor_dead";
    public const string SkillCast = "skill_cast";
    public const string BuffApply = "buff_apply";
    public const string ProjectileHit = "projectile_hit";
    public const string Combined = "combined";
}
```

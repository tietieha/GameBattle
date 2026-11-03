# 服务注册规范

本文档定义了 moba.runtime 战斗逻辑世界层的服务注册标准模式。

## 1. 服务分类

| 类型 | 接口 | 注册方式 | 生命周期 |
|-----|------|---------|---------|
| 核心服务 | `IService` | `[WorldService]` | Scoped/Singleton |
| 快照提供者 | `IWorldStateSnapshotProvider` | `[WorldService]` | Singleton |
| 输入处理 | `IWorldInputSink` | `[WorldService]` | Scoped |
| 事件总线 | `IEventBus` | 系统注入 | Singleton |

## 2. 服务注册模板

### 2.1 基础服务注册

```csharp
using AbilityKit.Ability.World.Services;
using AbilityKit.Ability.World.Services.Attributes;

namespace AbilityKit.Demo.Moba.Services
{
    /// <summary>
    /// 我的服务
    /// </summary>
    [WorldService(typeof(MyService))]
    public sealed class MyService : IService
    {
        private readonly IDependencyA _depA;
        private readonly IDependencyB _depB;

        /// <summary>
        /// 依赖通过构造函数注入
        /// </summary>
        public MyService(IDependencyA depA, IDependencyB depB)
        {
            _depA = depA ?? throw new ArgumentNullException(nameof(depA));
            _depB = depB ?? throw new ArgumentNullException(nameof(depB));
        }
    }
}
```

### 2.2 多接口服务注册

```csharp
/// <summary>
/// 输入协调器同时作为玩法输入入口和 Host 帧同步输入落点。
/// </summary>
[WorldService(typeof(IWorldInputSink))]
[WorldService(typeof(IMobaInputCoordinator))]
[WorldService(typeof(MobaInputCoordinator))]
public sealed class MobaInputCoordinator : IWorldInputSink, IMobaInputCoordinator
{
    // 实现
}
```

### 2.3 非默认服务注册

```csharp
/// <summary>
/// isDefault: false 表示不作为默认接口注册
/// 仅通过具体类型 MyService 访问
/// </summary>
[WorldService(typeof(MyService), isDefault: false)]
public sealed class MyService : IService
{
}
```

## 3. 生命周期

| 生命周期 | 适用场景 | 说明 |
|---------|---------|------|
| `WorldLifetime.Singleton` | 无状态服务、配置服务 | 全局单例 |
| `WorldLifetime.Scoped` | 有状态服务、实体相关服务 | 每个 World 实例一份 |
| `WorldLifetime.Transient` | 每次请求创建新实例 | 很少使用 |

## 4. 服务发现机制

服务通过 `[WorldService]` 特性自动发现和注册：

```csharp
// MobaServicesAutoModule.cs
public void Configure(WorldContainerBuilder builder)
{
    builder.AddModule(new MobaApplicationServicesModule());
    builder.AddModule(new MobaApplicationSystemsServicesModule());
    builder.AddModule(new MobaInfrastructureServicesModule());
}
```

当前按模块拆分扫描范围：

| 模块 | 扫描的命名空间前缀 | 用途 |
| --- | --- | --- |
| `MobaApplicationServicesModule` | `AbilityKit.Demo.Moba.Services` | 业务服务、技能服务、战斗服务、输入服务、快照服务等 |
| `MobaApplicationSystemsServicesModule` | `AbilityKit.Demo.Moba.Systems` | 系统侧辅助服务、PlanAction 模块注册等 |
| `MobaInfrastructureServicesModule` | `AbilityKit.Demo.Moba.Util`, `AbilityKit.Demo.Moba.Util.Generator` | 工具、生成器、基础设施辅助能力 |

新增服务时优先放在 `AbilityKit.Demo.Moba.Services` 命名空间下；只有确实属于系统安装或基础设施工具时，才进入 `Systems` 或 `Util` 范围。

## 5. 服务依赖注入

### 5.1 构造函数注入（推荐）

```csharp
public MyService(IDependencyA depA, IDependencyB depB)
{
    _depA = depA ?? throw new ArgumentNullException(nameof(depA));
    _depB = depB ?? throw new ArgumentNullException(nameof(depB));
}
```

### 5.2 可选依赖

```csharp
public MyService(
    IRequiredService required,
    [OptionalDependency] IOptionalService? optional = null)
{
    _required = required;
    _optional = optional;
}
```

### 5.3 延迟解析

```csharp
public sealed class LazyService : IService, IWorldInitializable
{
    private IWorldResolver? _resolver;

    public void OnInit(IWorldResolver services)
    {
        _resolver = services;
    }

    public void DoSomething()
    {
        if (_resolver?.TryResolve<ISomeService>(out var svc) == true)
        {
            svc.Process();
        }
    }
}
```

## 6. 禁止事项

| 禁止 | 说明 | 正确做法 |
|-----|------|---------|
| ❌ 在服务中使用静态字段 | 导致状态污染，难以测试 | 使用实例字段或 Scoped 服务 |
| ❌ 服务直接实例化其他服务 | 破坏 DI 模式 | 通过构造函数注入 |
| ❌ 服务循环依赖 | 导致初始化失败 | 重构设计，消除循环 |
| ❌ 在构造函数中执行耗时操作 | 延长启动时间 | 使用 `IWorldInitializable.OnInit` |
| ❌ 服务持有 Entitas Entity 直接引用 | 阻止 GC | 使用 ActorId 引用，通过 Registry 查询 |

## 7. 服务组织规范

### 7.1 按功能模块组织目录

```
Services/
├── Core/                 # 核心服务（MobaGamePhaseService）
├── Combat/              # 战斗服务（DamagePipelineService）
├── Skill/              # 技能服务（SkillExecutor）
├── Buff/               # Buff服务（MobaBuffService）
├── Entity/             # 实体管理（MobaEntityManager）
├── Actor/              # Actor管理（MobaActorRegistry）
├── Snapshot/           # 快照服务（MobaSnapshotRouter）
├── Input/              # 输入处理（MobaInputCoordinator）
├── Projectile/        # 投射物服务
├── Spawn/             # 实体生成
├── FrameSync/         # 帧同步
├── Triggering/        # 触发器
└── ContinuousPeriodic/ # 持续周期行为
```

### 7.2 服务命名规范

| 类型 | 命名规范 | 示例 |
|-----|---------|------|
| 服务实现 | `{功能}Service` | `MobaBuffService` |
| 服务接口 | `I{功能}Service` | `IMobaBuffService` |
| 注册常量 | `{功能}Service` | `MobaGamePhaseService` |

## 8. 新建服务检查清单

创建新服务时，确保：

- [ ] 使用 `[WorldService]` 特性注册
- [ ] 实现 `IService` 接口
- [ ] 依赖通过构造函数注入
- [ ] 添加 `throw ArgumentNullException` 校验
- [ ] 实现 `IDisposable.Dispose()` 清理资源
- [ ] 放置在正确的 Services 子目录
- [ ] 遵循命名规范

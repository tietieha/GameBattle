# Bootstrap Flow 规范

本文档定义了 moba.runtime 战斗逻辑世界层的引导流程（Bootstrap Flow）标准模式。

## 1. 概述

Bootstrap Flow 是世界初始化的核心机制，通过 Stage（阶段）模式组织初始化逻辑。

```
MobaWorldBootstrapModule
    └── MobaBootstrapFlow
            └── MobaBootstrapStageRegistry
                    ├── ConfigStage          (配置加载)
                    ├── CoreStateStage      (核心状态)
                    ├── WorldModulesStage   (世界模块)
                    ├── TagsStage           (标签系统)
                    ├── TriggerPlansStage   (触发器计划)
                    ├── TargetingAndSkillsStage (技能目标)
                    ├── PlanTriggeringStage (计划触发)
                    └── WorldInitStage      (世界初始化)
```

## 2. Stage 接口

所有 Stage 必须实现 `MobaBootstrapStageBase`：

```csharp
public abstract class MobaBootstrapStageBase
{
    /// <summary>
    /// Stage 名称（用于日志）
    /// </summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    /// 依赖的其他 Stage 名称
    /// </summary>
    public virtual string[] Dependencies => Array.Empty<string>();

    /// <summary>
    /// 配置阶段 - 添加服务到容器
    /// </summary>
    protected internal virtual void Configure(WorldContainerBuilder builder)
    {
    }

    /// <summary>
    /// 安装阶段 - 安装系统
    /// </summary>
    protected internal virtual void Install(
        Entitas.IContexts contexts,
        Entitas.Systems systems,
        IWorldResolver services)
    {
    }
}
```

## 3. Stage 创建模板

### 3.1 配置型 Stage

```csharp
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Systems.Bootstrap.Flow;

namespace AbilityKit.Demo.Moba.Systems.Bootstrap.Flow.Stages
{
    /// <summary>
    /// 我的配置 Stage
    /// </summary>
    [MobaBootstrapStage]
    public sealed class MyConfigStage : MobaBootstrapStageBase
    {
        public override string Name => "MyConfig";

        protected internal override void Configure(WorldContainerBuilder builder)
        {
            // 1. 注册配置接口
            builder.TryRegister<IMyConfigInterface>(WorldLifetime.Singleton, _ =>
                MobaConfigRegistry.Instance);

            // 2. 注册数据库
            builder.TryRegister<MyConfigDatabase>(WorldLifetime.Singleton, _ =>
            {
                var loader = _.Resolve<ITextAssetLoader>();
                return new MyConfigDatabase(loader);
            });
        }
    }
}
```

### 3.2 安装型 Stage

```csharp
using Entitas;
using AbilityKit.Ability.World.DI;
using AbilityKit.Demo.Moba.Systems.Bootstrap.Flow;

namespace AbilityKit.Demo.Moba.Systems.Bootstrap.Flow.Stages
{
    /// <summary>
    /// 我的安装 Stage
    /// </summary>
    [MobaBootstrapStage]
    public sealed class MyInstallStage : MobaBootstrapStageBase
    {
        public override string Name => "Install.MyFeature";

        protected internal override void Install(
            IContexts contexts,
            ISystems systems,
            IWorldResolver services)
        {
            // 1. 获取依赖服务
            if (!services.TryResolve<IMyService>(out var svc))
            {
                Log.Warning("[MyInstallStage] IMyService not found, skipping");
                return;
            }

            // 2. 初始化
            svc.Initialize();

            // 3. 注册到系统
            Log.Info($"[MyInstallStage] MyFeature initialized");
        }
    }
}
```

## 4. Stage 注册

使用 `[MobaBootstrapStage]` 特性自动注册：

```csharp
[MobaBootstrapStage]
public sealed class MyStage : MobaBootstrapStageBase { }
```

静态初始化器会自动扫描并注册所有 Stage。

## 5. 执行顺序

Stage 按以下顺序执行：

1. **Configure 阶段** - 所有 Stage 的 `Configure()` 方法
2. **Install 阶段** - 所有 Stage 的 `Install()` 方法

每个阶段内按注册顺序执行（通常按类名字母序）。

## 6. Stage 依赖

声明依赖确保正确的初始化顺序：

```csharp
public override string[] Dependencies => new[] { "OtherStageName" };
```

> 注意：当前实现中 `Dependencies` 属性存在但未被使用，Stage 按注册顺序执行。

## 7. 错误处理

每个 Stage 的执行都包裹在 try-catch 中：

```csharp
protected internal void ExecuteConfigure(WorldContainerBuilder builder)
{
    try
    {
        Log.Info($"[MobaBootstrap] Configure stage: {Name}");
        Configure(builder);
        Log.Info($"[MobaBootstrap] Configure stage done: {Name}");
    }
    catch (Exception ex)
    {
        Log.Exception(ex, $"[MobaBootstrap] Configure stage failed: {Name}");
        throw;  // 重新抛出，中断初始化
    }
}
```

## 8. 最佳实践

### 8.1 分离 Configure 和 Install

| 阶段 | 职责 | 示例 |
|-----|------|------|
| `Configure` | 注册服务到 DI 容器 | `builder.TryRegister<T>()` |
| `Install` | 使用已注册的服务初始化 | `services.Resolve<T>().Initialize()` |

### 8.2 检查依赖可用性

```csharp
protected internal override void Install(...)
{
    if (!services.TryResolve<IMyService>(out var svc))
    {
        Log.Warning("[MyStage] IMyService not found, skipping");
        return;  // 可选服务缺失不影响后续 Stage
    }

    svc.Initialize();
}
```

### 8.3 添加日志

```csharp
Log.Info($"[MyStage] Starting...");
Log.Info($"[MyStage] MyService initialized");
Log.Info($"[MyStage] Completed");
```

## 9. 新建 Stage 检查清单

创建新 Stage 时，确保：

- [ ] 继承 `MobaBootstrapStageBase`
- [ ] 使用 `[MobaBootstrapStage]` 特性
- [ ] 重写 `Name` 属性（用于日志）
- [ ] `Configure` 中只注册服务
- [ ] `Install` 中执行初始化
- [ ] 添加日志输出
- [ ] 检查可选依赖
- [ ] 异常处理

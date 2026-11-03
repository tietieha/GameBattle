using AbilityKit.Ability.World.DI;
using AbilityKit.Core.Logging;

namespace AbilityKit.Demo.Moba.Systems.Bootstrap.Flow.Stages
{
    /// <summary>
    /// Bootstrap stage authoring sample.
    /// This type is intentionally not decorated with MobaBootstrapStage.
    /// </summary>
    public sealed class TemplateFeatureStage : MobaBootstrapStageBase
    {
        public override string Name => "TemplateFeature";

        /// <summary>
        /// 配置阶段 - 注册服务到 DI 容器
        /// 此阶段只能注册服务，不能使用服务
        /// </summary>
        protected internal override void Configure(WorldContainerBuilder builder)
        {
            Log.Info($"[{Name}] Configuring services...");

            // 示例 1: 注册单例服务
            // builder.TryRegister<IMyService>(WorldLifetime.Singleton, _ => new MyService());

            // 示例 2: 注册配置服务
            // builder.TryRegister<IMyConfig>(WorldLifetime.Singleton, _ => MyConfigRegistry.Instance);

            // 示例 3: 注册有依赖的服务
            // builder.TryRegister<IMyComplexService>(WorldLifetime.Singleton, r =>
            // {
            //     var depA = r.Resolve<IDependencyA>();
            //     var depB = r.Resolve<IDependencyB>();
            //     return new MyComplexService(depA, depB);
            // });

            Log.Info($"[{Name}] Services configured");
        }

        /// <summary>
        /// 安装阶段 - 使用已注册的服务初始化
        /// 此阶段可以使用服务进行初始化
        /// </summary>
        protected internal override void Install(
            Entitas.IContexts contexts,
            Entitas.Systems systems,
            IWorldResolver services)
        {
            Log.Info($"[{Name}] Installing feature...");

            // 示例 1: 检查并使用服务
            // if (services.TryResolve<IMyService>(out var svc))
            // {
            //     svc.Initialize();
            //     Log.Info($"[{Name}] MyService initialized");
            // }
            // else
            // {
            //     Log.Warning($"[{Name}] MyService not found, skipping");
            // }

            // 示例 2: 获取多个服务协同工作
            // if (services.TryResolve<IMyService>(out var svcA) &&
            //     services.TryResolve<IOtherService>(out var svcB))
            // {
            //     svcA.SetPartner(svcB);
            // }

            Log.Info($"[{Name}] Feature installed");
        }
    }
}

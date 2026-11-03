using System;
using AbilityKit.Ability.World.DI;

namespace AbilityKit.Ability.Share.ECS.Entitas
{
    /// <summary>
    /// Entitas ECS 世界模块
    /// 注意：此模块在 Configure 阶段注册服务，但不立即解析 IContexts。
    /// 工厂方法使用延迟解析，在第一次解析时才创建依赖 IContexts 的服务。
    /// </summary>
    public sealed class EntitasEcsWorldModule : IWorldModule
    {
        public void Configure(WorldContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            // 使用延迟解析：工厂方法在解析时才调用 Resolve<IContexts>()
            // 这样即使 IContexts 还没有创建，模块也能正常注册
            builder.TryRegister<EntitasActorIdLookup>(WorldLifetime.Scoped, s =>
            {
                // 延迟解析 IContexts - 只在第一次解析时才调用
                var contexts = s.Resolve<global::Entitas.IContexts>() as global::Contexts;
                if (contexts == null) throw new InvalidOperationException("[EntitasEcsWorldModule] Expected Entitas IContexts to be generated Contexts instance.");
                return new EntitasActorIdLookup(contexts.actor);
            });

            builder.TryRegisterType<IUnitResolver, EntitasUnitResolver>(WorldLifetime.Scoped);

            builder.TryRegister<IEcsWorld>(WorldLifetime.Scoped, s =>
            {
                // 延迟解析 - 只在第一次解析时才创建依赖
                var lookup = s.Resolve<EntitasActorIdLookup>();
                var units = s.Resolve<IUnitResolver>();
                return new EntitasEcsWorld(s, lookup, units);
            });
        }
    }
}

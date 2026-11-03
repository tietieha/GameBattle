using System;

namespace AbilityKit.Ability.World.DI
{
    /// <summary>
    /// 在创建 scope 时把外部构造的实例「播种」进该 scope 的协作接口。
    ///
    /// 语义（见 MobaFlowSpec.md 播种机制）：
    /// - 播种用于注入<b>跨阶段输入</b>（如 per-battle 的 bootstrapper / gateway 工厂），
    ///   解决「容器在构造期一次建好、但 per-scope 数据此时尚不存在」的矛盾。
    /// - 播种实例的生命周期归<b>调用方</b>：<c>scope.Dispose()</c> 不会连带释放它。
    /// - 播种会覆盖容器内同类型的 scoped 工厂（解析时播种实例优先命中）。
    /// </summary>
    public interface IWorldScopeSeeder
    {
        /// <summary>把 <paramref name="instance"/> 播种为服务类型 <paramref name="serviceType"/> 的解析结果。</summary>
        IWorldScopeSeeder Seed(Type serviceType, object instance);

        /// <summary>把 <paramref name="instance"/> 播种为服务类型 <typeparamref name="TService"/> 的解析结果。</summary>
        IWorldScopeSeeder Seed<TService>(TService instance);
    }
}

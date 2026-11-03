using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.World.DI
{
    /// <summary>
    /// 测试辅助：在不启动完整 <see cref="WorldContainer"/> 的情况下，向一个已创建的实例
    /// 注入其 <c>[WorldInject]</c> 字段/属性（包含 required:false 的可选依赖）。
    ///
    /// 适用于以 <c>[WorldInject]</c> 私有字段注入为主、构造函数参数很多而不便改造成构造注入的服务：
    /// 直接 <c>new</c> 出实例（或通过无参/简单构造函数），再用本工具把 mock 推入，
    /// 走的是与运行时完全相同的注入路径，避免单元测试退化成必须起整个容器的集成测试。
    ///
    /// 仅用于测试代码。生产代码应继续通过容器解析。
    /// </summary>
    public static class WorldTestInjector
    {
        /// <summary>
        /// 使用一个已有的 <see cref="IWorldResolver"/>（例如 mock resolver）注入实例上的所有 [WorldInject] 成员。
        /// </summary>
        public static T Inject<T>(T instance, IWorldResolver resolver) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            WorldActivator.InjectMembersInto(instance, resolver);
            return instance;
        }

        /// <summary>
        /// 使用一组手工提供的 (serviceType -> instance) 依赖注入实例上的所有 [WorldInject] 成员。
        /// 未在字典中提供的可选依赖（required:false）会被跳过；缺失的必选依赖会抛异常。
        /// </summary>
        public static T Inject<T>(T instance, IReadOnlyDictionary<Type, object> dependencies) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (dependencies == null) throw new ArgumentNullException(nameof(dependencies));

            return Inject(instance, new DictionaryResolver(dependencies));
        }

        /// <summary>
        /// 创建一个可链式登记 mock 依赖的构建器，登记完成后对目标实例执行注入。
        /// 用法：
        /// <code>
        /// WorldTestInjector.For(new MobaSummonService())
        ///     .With(mockAllocator)
        ///     .With&lt;MobaActorRegistry&gt;(mockRegistry)
        ///     .Build();
        /// </code>
        /// </summary>
        public static Builder<T> For<T>(T instance) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            return new Builder<T>(instance);
        }

        public sealed class Builder<T> where T : class
        {
            private readonly T _instance;
            private readonly Dictionary<Type, object> _deps = new Dictionary<Type, object>();

            internal Builder(T instance)
            {
                _instance = instance;
            }

            /// <summary>登记依赖，服务类型显式指定。</summary>
            public Builder<T> With<TService>(TService dependency)
            {
                _deps[typeof(TService)] = dependency;
                return this;
            }

            /// <summary>登记依赖，服务类型由实例的运行时类型推断（适合具体类作为服务类型登记）。</summary>
            public Builder<T> With(object dependency)
            {
                if (dependency == null) throw new ArgumentNullException(nameof(dependency));
                _deps[dependency.GetType()] = dependency;
                return this;
            }

            /// <summary>登记依赖，显式给出服务类型键。</summary>
            public Builder<T> With(Type serviceType, object dependency)
            {
                if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
                _deps[serviceType] = dependency;
                return this;
            }

            /// <summary>执行注入并返回目标实例。</summary>
            public T Build()
            {
                return Inject(_instance, new DictionaryResolver(_deps));
            }
        }

        private sealed class DictionaryResolver : IWorldResolver
        {
            private readonly IReadOnlyDictionary<Type, object> _map;

            public DictionaryResolver(IReadOnlyDictionary<Type, object> map)
            {
                _map = map ?? throw new ArgumentNullException(nameof(map));
            }

            public object Resolve(Type serviceType)
            {
                if (TryResolve(serviceType, out var value)) return value;
                throw new InvalidOperationException($"[WorldTestInjector] Service not provided: {serviceType?.FullName}");
            }

            public T Resolve<T>()
            {
                return (T)Resolve(typeof(T));
            }

            public bool TryResolve(Type serviceType, out object instance)
            {
                if (serviceType == null)
                {
                    instance = null;
                    return false;
                }

                return _map.TryGetValue(serviceType, out instance) && instance != null;
            }

            public bool TryResolve<T>(out T instance)
            {
                if (TryResolve(typeof(T), out var obj) && obj is T t)
                {
                    instance = t;
                    return true;
                }

                instance = default;
                return false;
            }
        }
    }
}

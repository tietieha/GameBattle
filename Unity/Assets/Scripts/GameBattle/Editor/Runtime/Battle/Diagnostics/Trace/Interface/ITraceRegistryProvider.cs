using System;
using System.Collections.Generic;
using AbilityKit.Trace;

namespace AbilityKit.Trace.Editor.Windows
{
    /// <summary>
    /// 溯源注册表提供者接口
    /// 用于在编辑器中获取 TraceTreeRegistry 实例
    /// 业务层可以实现此接口来自定义如何获取注册表
    /// </summary>
    public interface ITraceRegistryProvider
    {
        /// <summary>
        /// 获取所有可用的溯源注册表
        /// </summary>
        IEnumerable<TraceTreeRegistryBase> GetRegistries();
    }

    /// <summary>
    /// 默认的溯源注册表提供者
    /// 优先使用运行时注册目录，未注册时兼容反射查找 Instance/Singleton
    /// </summary>
    public sealed class DefaultTraceRegistryProvider : ITraceRegistryProvider
    {
        private static DefaultTraceRegistryProvider _instance;

        public static DefaultTraceRegistryProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DefaultTraceRegistryProvider();
                }
                return _instance;
            }
        }

        public IEnumerable<TraceTreeRegistryBase> GetRegistries()
        {
            var result = new List<TraceTreeRegistryBase>();
            AddUnique(result, TraceRegistryDirectory.Registries);
            if (result.Count > 0)
                return result;

            var baseType = typeof(TraceTreeRegistryBase);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (baseType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                        {
                            // 尝试获取 Instance 属性
                            var instanceProp = type.GetProperty("Instance",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                            if (instanceProp != null)
                            {
                                var value = instanceProp.GetValue(null);
                                if (value is TraceTreeRegistryBase registry)
                                {
                                    AddUnique(result, registry);
                                    continue;
                                }
                            }

                            // 尝试获取 Singleton 属性
                            var singletonProp = type.GetProperty("Singleton",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                            if (singletonProp != null)
                            {
                                var value = singletonProp.GetValue(null);
                                if (value is TraceTreeRegistryBase registry)
                                {
                                    AddUnique(result, registry);
                                }
                            }
                        }
                    }
                }
                catch (System.Exception)
                {
                    // 忽略无法加载的程序集
                }
            }

            return result;
        }

        private static void AddUnique(List<TraceTreeRegistryBase> result, IEnumerable<TraceTreeRegistryBase> registries)
        {
            if (registries == null) return;
            foreach (var registry in registries)
                AddUnique(result, registry);
        }

        private static void AddUnique(List<TraceTreeRegistryBase> result, TraceTreeRegistryBase registry)
        {
            if (registry != null && !result.Contains(registry))
                result.Add(registry);
        }

        private DefaultTraceRegistryProvider() { }
    }
}

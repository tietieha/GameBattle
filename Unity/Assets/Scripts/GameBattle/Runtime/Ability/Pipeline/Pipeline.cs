using System;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线运行时访问器。
    /// 提供统一的访问入口。
    /// </summary>
    public static class Pipeline
    {
        private static PipelineRuntime? _defaultRuntime;
        private static bool _isInitialized;

        /// <summary>
        /// 设置注册表（编辑器初始化使用）。
        /// </summary>
        public static void SetRegistry(IPipelineRegistry registry)
        {
            DefaultRuntime.SetRegistry(registry);
        }

        /// <summary>
        /// 设置追踪记录器（编辑器初始化使用）。
        /// </summary>
        public static void SetTraceRecorder(IPipelineTraceRecorder recorder)
        {
            DefaultRuntime.SetTraceRecorder(recorder);
        }

        /// <summary>
        /// 默认管线运行时上下文。
        /// </summary>
        public static PipelineRuntime DefaultRuntime
        {
            get
            {
                EnsureInitialized();
                return _defaultRuntime!;
            }
        }

        /// <summary>
        /// 管线注册表。
        /// </summary>
        public static IPipelineRegistry Registry => DefaultRuntime.Registry;

        /// <summary>
        /// 追踪记录器。
        /// </summary>
        public static IPipelineTraceRecorder TraceRecorder => DefaultRuntime.TraceRecorder;

        /// <summary>
        /// 是否启用调试追踪。
        /// </summary>
        public static bool IsDebugEnabled => DefaultRuntime.IsDebugEnabled;

        /// <summary>
        /// 初始化管线系统（运行时基础版本）。
        /// </summary>
        private static void Initialize()
        {
            if (_isInitialized) return;

            _defaultRuntime = new PipelineRuntime();
            _defaultRuntime.Initialize();
            _isInitialized = true;
        }

        /// <summary>
        /// 关闭管线系统。
        /// </summary>
        public static void Shutdown()
        {
            _defaultRuntime?.Shutdown();
            _defaultRuntime = null;
            _isInitialized = false;
        }

        /// <summary>
        /// 记录追踪数据。
        /// </summary>
        public static void RecordTrace(IPipelineLifeOwner owner, PipelineTraceData data)
        {
            DefaultRuntime.RecordTrace(owner, data);
        }

        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }
    }
}

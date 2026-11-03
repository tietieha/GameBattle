using System;
using AbilityKit.Pipeline.Pooling;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 可实例化的管线运行时上下文。
    /// 一个运行时上下文持有自己的生命周期注册表和追踪记录器。
    /// </summary>
    public sealed class PipelineRuntime
    {
        /// <summary>
        /// 创建管线运行时上下文。
        /// </summary>
        public PipelineRuntime(IPipelineRegistry? registry = null, IPipelineTraceRecorder? traceRecorder = null)
        {
            Registry = registry ?? PipelineRegistry.Instance;
            TraceRecorder = traceRecorder ?? NoOpPipelineTraceRecorder.Instance;
        }

        /// <summary>
        /// 生命周期注册表。
        /// </summary>
        public IPipelineRegistry Registry { get; private set; }

        /// <summary>
        /// 追踪记录器。
        /// </summary>
        public IPipelineTraceRecorder TraceRecorder { get; private set; }

        /// <summary>
        /// 是否启用调试追踪。
        /// </summary>
        public bool IsDebugEnabled => TraceRecorder.IsEnabled;

        /// <summary>
        /// 初始化运行时上下文。
        /// </summary>
        public void Initialize()
        {
            PipelinePools.RegisterDefaultConfig();
            if (Registry is PipelineRegistry runtimeRegistry)
            {
                runtimeRegistry.Initialize();
            }
        }

        /// <summary>
        /// 设置生命周期注册表。
        /// </summary>
        public void SetRegistry(IPipelineRegistry registry)
        {
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// 设置追踪记录器。
        /// </summary>
        public void SetTraceRecorder(IPipelineTraceRecorder recorder)
        {
            TraceRecorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        }

        /// <summary>
        /// 记录追踪数据。
        /// </summary>
        public void RecordTrace(IPipelineLifeOwner owner, PipelineTraceData data)
        {
            if (owner == null) return;

            if (TraceRecorder.IsEnabled)
            {
                TraceRecorder.Record(owner, data);
            }

            PipelineDebugHooks.NotifyTrace(owner, data);
        }

        /// <summary>
        /// 关闭运行时上下文。
        /// </summary>
        public void Shutdown()
        {
            if (Registry is PipelineRegistry runtimeRegistry)
            {
                runtimeRegistry.Shutdown();
            }
        }
    }
}

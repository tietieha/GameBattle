using AbilityKit.Triggering.Runtime;

namespace AbilityKit.Triggering.Runtime.Plan
{
    /// <summary>
    /// TriggerPlan 行为树主线执行节点。
    /// 用于承载配置化 TriggerPlan 的 Sequence、Selector、If、Repeat、ActionCall 等正式执行结构。
    /// 与 Runtime/Executable 的旧 DSL/示例体系分离，后者仅作为兼容和迁移参考。
    /// </summary>
    public interface ITriggerPlanExecutable
    {
        string Name { get; }
        ETriggerPlanExecutableKind Kind { get; }
        float Weight { get; }

        TriggerPlanExecutionResult Execute<TCtx>(object args, in ExecCtx<TCtx> ctx)
            where TCtx : class;
    }
}

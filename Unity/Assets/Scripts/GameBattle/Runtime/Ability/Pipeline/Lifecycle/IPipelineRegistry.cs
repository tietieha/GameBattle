using System.Collections.Generic;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线生命周期注册表接口。
    /// 负责管理活跃的管线运行实例。
    /// </summary>
    public interface IPipelineRegistry
    {
        /// <summary>
        /// 注册一个管线运行实例。
        /// </summary>
        void Register(IPipelineLifeOwner owner);

        /// <summary>
        /// 注销一个管线运行实例。
        /// </summary>
        void Unregister(IPipelineLifeOwner owner);

        /// <summary>
        /// 获取所有活跃的管线运行实例。
        /// </summary>
        IReadOnlyList<IPipelineLifeOwner> GetActiveOwners();

        /// <summary>
        /// 活跃实例数量。
        /// </summary>
        int ActiveCount { get; }

        /// <summary>
        /// 中断所有活跃管线。
        /// </summary>
        void InterruptAll();

        /// <summary>
        /// 按阶段 ID 过滤活跃管线。该方法会返回新的结果列表；高频路径建议使用填充指定阶段拥有者列表方法或租借指定阶段拥有者列表方法。
        /// </summary>
        IReadOnlyList<IPipelineLifeOwner> GetOwnersByPhase(AbilityPipelinePhaseId phaseId);

        /// <summary>
        /// 将指定阶段 ID 的活跃管线追加到外部结果列表，避免查询过程分配临时列表。
        /// </summary>
        int FillOwnersByPhase(AbilityPipelinePhaseId phaseId, IList<IPipelineLifeOwner> results);

        /// <summary>
        /// 从对象池租借列表并填充指定阶段 ID 的活跃管线；使用完成后必须释放返回租约。
        /// </summary>
        PipelineRegistryOwnerListLease RentOwnersByPhase(AbilityPipelinePhaseId phaseId);

        /// <summary>
        /// 按状态过滤活跃管线。该方法会返回新的结果列表；高频路径建议使用填充指定状态拥有者列表方法或租借指定状态拥有者列表方法。
        /// </summary>
        IReadOnlyList<IPipelineLifeOwner> GetOwnersByState(EAbilityPipelineState state);

        /// <summary>
        /// 将指定状态的活跃管线追加到外部结果列表，避免查询过程分配临时列表。
        /// </summary>
        int FillOwnersByState(EAbilityPipelineState state, IList<IPipelineLifeOwner> results);

        /// <summary>
        /// 从对象池租借列表并填充指定状态的活跃管线；使用完成后必须释放返回租约。
        /// </summary>
        PipelineRegistryOwnerListLease RentOwnersByState(EAbilityPipelineState state);
    }

    /// <summary>
    /// 管线生命周期注册表事件。
    /// </summary>
    public static class PipelineRegistryEvents
    {
        /// <summary>
        /// 当注册表发生变化时触发。
        /// </summary>
        public static System.Action? OnChanged;

        /// <summary>
        /// 当管线运行开始时触发。
        /// </summary>
        public static System.Action<IPipelineLifeOwner>? OnRunStarted;

        /// <summary>
        /// 当管线运行结束时触发。
        /// </summary>
        public static System.Action<IPipelineLifeOwner, EAbilityPipelineState>? OnRunEnded;

        /// <summary>
        /// 当全局中断时触发。
        /// </summary>
        public static System.Action? OnGlobalInterrupt;
    }
}

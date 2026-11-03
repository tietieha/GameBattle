using System;
using System.Collections.Generic;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线阶段接口。阶段同时支持瞬时完成和跨帧更新两种执行模式。
    /// </summary>
    /// <typeparam name="TCtx">管线上下文类型。</typeparam>
    public interface IAbilityPipelinePhase<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        /// <summary>
        /// 阶段 ID，用于调试、追踪和运行时查询。
        /// </summary>
        AbilityPipelinePhaseId PhaseId { get; }

        /// <summary>
        /// 阶段是否已经完成。瞬时阶段应在执行入口内完成，持续阶段可在更新入口中完成。
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// 是否为复合阶段。
        /// </summary>
        bool IsComposite { get; }

        /// <summary>
        /// 子阶段集合。非复合阶段通常返回 null 或空集合。
        /// </summary>
        IReadOnlyList<IAbilityPipelinePhase<TCtx>> SubPhases { get; }

        /// <summary>
        /// 判断阶段在当前上下文下是否应该执行。
        /// </summary>
        bool ShouldExecute(TCtx context);

        /// <summary>
        /// 执行阶段入口。实现应在此初始化运行态并执行首帧逻辑。
        /// </summary>
        void Execute(TCtx context);

        /// <summary>
        /// 更新阶段。仅持续阶段或复合阶段通常需要在这里推进运行态。
        /// </summary>
        void OnUpdate(TCtx context, float deltaTime);

        /// <summary>
        /// 重置阶段运行态，以便下一次管线运行复用该阶段配置。
        /// </summary>
        void Reset();

        /// <summary>
        /// 处理阶段执行中的异常。
        /// </summary>
        void HandleError(TCtx context, Exception exception);
    }
}

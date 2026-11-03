using System;
using System.Collections.Generic;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 能力管线实例接口。管线通常持有一组配置态阶段，调用启动方法后创建一次运行实例。
    /// </summary>
    /// <typeparam name="TCtx">管线上下文类型。</typeparam>
    public interface IAbilityPipeline<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        /// <summary>
        /// 管线事件集合。事件订阅归管线实例所有，重置方法不会自动清除订阅。
        /// </summary>
        AbilityPipelineEvents<TCtx> Events { get; }

        /// <summary>
        /// 启动一次管线运行。返回的运行对象代表一次独立执行，进入终态后不应再次复用。
        /// </summary>
        /// <param name="config">本次运行使用的配置。</param>
        /// <param name="context">本次运行使用的上下文；其生命周期由具体管线实现负责释放。</param>
        /// <returns>一次性管线运行对象。</returns>
        IAbilityPipelineRun<TCtx> Start(IAbilityPipelineConfig config, TCtx context);

        /// <summary>
        /// 将阶段追加到管线末尾。阶段实例通常是配置态对象，不建议在多个并发运行间共享可变运行态。
        /// </summary>
        void AddPhase(IAbilityPipelinePhase<TCtx> phase);

        /// <summary>
        /// 将阶段插入到指定索引位置。
        /// </summary>
        void InsertPhase(int index, IAbilityPipelinePhase<TCtx> phase);

        /// <summary>
        /// 按阶段 ID 移除第一个匹配阶段。
        /// </summary>
        void RemovePhase(AbilityPipelinePhaseId phaseId);

        /// <summary>
        /// 重置管线内所有阶段运行态，但不会清除阶段列表或事件订阅。
        /// </summary>
        void Reset();
    }
}

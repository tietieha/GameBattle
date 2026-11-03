using System;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 动作阶段：执行一个同步回调后立即完成。
    /// </summary>
    /// <typeparam name="TCtx">管线上下文类型。</typeparam>
    public sealed class AbilityActionPhase<TCtx> : AbilityInstantPhaseBase<TCtx>, IAbilityPipelinePhaseInstanceFactory<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        private readonly Action<TCtx> _action;

        /// <summary>
        /// 创建动作阶段。
        /// </summary>
        public AbilityActionPhase(Action<TCtx> action) : this(new AbilityPipelinePhaseId("Action"), action)
        {
        }

        /// <summary>
        /// 使用指定阶段 ID 创建动作阶段。
        /// </summary>
        public AbilityActionPhase(AbilityPipelinePhaseId phaseId, Action<TCtx> action) : base(phaseId)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <inheritdoc />
        protected override void OnInstantExecute(TCtx context)
        {
            _action.Invoke(context);
        }

        /// <inheritdoc />
        public IAbilityPipelinePhase<TCtx> CreateRunPhase()
        {
            return new AbilityActionPhase<TCtx>(PhaseId, _action);
        }

        /// <summary>
        /// 创建动作阶段。
        /// </summary>
        public static AbilityActionPhase<TCtx> Create(Action<TCtx> action)
        {
            return new AbilityActionPhase<TCtx>(action);
        }
    }
}

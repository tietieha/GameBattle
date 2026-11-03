using System;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 条件等待阶段：持续等待谓词满足，可选超时后完成或保持等待。
    /// </summary>
    /// <typeparam name="TCtx">管线上下文类型。</typeparam>
    public sealed class AbilityWaitUntilPhase<TCtx> : AbilityDurationalPhaseBase<TCtx>, IAbilityPipelinePhaseInstanceFactory<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        private readonly Func<TCtx, bool> _predicate;
        private readonly bool _completeOnTimeout;

        /// <summary>
        /// 创建条件等待阶段。
        /// </summary>
        public AbilityWaitUntilPhase(Func<TCtx, bool> predicate, float timeout = -1f, bool completeOnTimeout = true)
            : this(new AbilityPipelinePhaseId("WaitUntil"), predicate, timeout, completeOnTimeout)
        {
        }

        /// <summary>
        /// 使用指定阶段 ID 创建条件等待阶段。
        /// </summary>
        public AbilityWaitUntilPhase(AbilityPipelinePhaseId phaseId, Func<TCtx, bool> predicate, float timeout = -1f, bool completeOnTimeout = true)
            : base(phaseId)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            Duration = timeout;
            _completeOnTimeout = completeOnTimeout;
        }

        /// <inheritdoc />
        protected override void OnExecute(TCtx context)
        {
            if (_predicate.Invoke(context))
            {
                Complete(context);
            }
        }

        /// <inheritdoc />
        protected override void OnTick(TCtx context, float deltaTime)
        {
            if (_predicate.Invoke(context))
            {
                Complete(context);
            }
        }

        /// <inheritdoc />
        public override void OnUpdate(TCtx context, float deltaTime)
        {
            if (IsComplete || context.IsPaused)
                return;

            if (_predicate.Invoke(context))
            {
                Complete(context);
                return;
            }

            if (Duration < 0f)
                return;

            base.OnUpdate(context, deltaTime);
            if (IsComplete && !_completeOnTimeout)
            {
                Reset();
            }
        }

        /// <inheritdoc />
        public IAbilityPipelinePhase<TCtx> CreateRunPhase()
        {
            return new AbilityWaitUntilPhase<TCtx>(PhaseId, _predicate, Duration, _completeOnTimeout);
        }

        /// <summary>
        /// 创建条件等待阶段。
        /// </summary>
        public static AbilityWaitUntilPhase<TCtx> Create(Func<TCtx, bool> predicate, float timeout = -1f, bool completeOnTimeout = true)
        {
            return new AbilityWaitUntilPhase<TCtx>(predicate, timeout, completeOnTimeout);
        }
    }
}

using System;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线阶段图的正式构建入口。
    /// </summary>
    public static class PipelineGraph
    {
        /// <summary>
        /// 创建顺序复合阶段，并按传入顺序追加子阶段。
        /// </summary>
        public static AbilitySequencePhase<TCtx> Sequence<TCtx>(params IAbilityPipelinePhase<TCtx>[] phases)
            where TCtx : IAbilityPipelineContext
        {
            var sequence = new AbilitySequencePhase<TCtx>();
            AddSubPhases(sequence, phases);
            return sequence;
        }

        /// <summary>
        /// 创建并行复合阶段，并追加传入的子阶段。
        /// </summary>
        public static AbilityParallelPhase<TCtx> Parallel<TCtx>(params IAbilityPipelinePhase<TCtx>[] phases)
            where TCtx : IAbilityPipelineContext
        {
            var parallel = new AbilityParallelPhase<TCtx>(new AbilityPipelinePhaseId("Parallel"));
            AddSubPhases(parallel, phases);
            return parallel;
        }

        /// <summary>
        /// 创建条件阶段，包含一个必选分支和一个可选回退分支。
        /// </summary>
        public static AbilityConditionalPhase<TCtx> Conditional<TCtx>(IAbilityConditionNode condition, IAbilityPipelinePhase<TCtx> whenTrue, IAbilityPipelinePhase<TCtx>? whenFalse = null)
            where TCtx : IAbilityPipelineContext
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (whenTrue == null) throw new ArgumentNullException(nameof(whenTrue));

            var conditional = new AbilityConditionalPhase<TCtx>();
            conditional.AddBranch(condition, whenTrue);
            if (whenFalse != null)
            {
                conditional.NoConditionBehavior = ENoConditionBehavior.Complete;
                conditional.AddBranch(AlwaysConditionNode.Instance, whenFalse);
            }

            return conditional;
        }

        /// <summary>
        /// 创建围绕指定子阶段执行的重复阶段。
        /// </summary>
        public static AbilityRepeatPhase<TCtx> Repeat<TCtx>(IAbilityPipelinePhase<TCtx> phase, int repeatCount = -1, float interval = 0f)
            where TCtx : IAbilityPipelineContext
        {
            if (phase == null) throw new ArgumentNullException(nameof(phase));

            var repeat = new AbilityRepeatPhase<TCtx>(repeatCount);
            repeat.RepeatInterval = interval;
            repeat.SetRepeatPhase(phase);
            return repeat;
        }

        /// <summary>
        /// 创建围绕动作回调执行的重复阶段。
        /// </summary>
        public static AbilityRepeatPhase<TCtx> Repeat<TCtx>(Action<TCtx, int> action, int repeatCount = -1, float interval = 0f)
            where TCtx : IAbilityPipelineContext
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return AbilityRepeatPhase<TCtx>.Create(action, repeatCount, interval);
        }

        /// <summary>
        /// 创建动作阶段。
        /// </summary>
        public static AbilityActionPhase<TCtx> Action<TCtx>(Action<TCtx> action)
            where TCtx : IAbilityPipelineContext
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return AbilityActionPhase<TCtx>.Create(action);
        }

        /// <summary>
        /// 创建延迟阶段。
        /// </summary>
        public static AbilityDelayPhase<TCtx> Delay<TCtx>(float seconds)
            where TCtx : IAbilityPipelineContext
        {
            return AbilityDelayPhase<TCtx>.Create(seconds);
        }

        /// <summary>
        /// 创建条件等待阶段。
        /// </summary>
        public static AbilityWaitUntilPhase<TCtx> WaitUntil<TCtx>(Func<TCtx, bool> predicate, float timeout = -1f, bool completeOnTimeout = true)
            where TCtx : IAbilityPipelineContext
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return AbilityWaitUntilPhase<TCtx>.Create(predicate, timeout, completeOnTimeout);
        }

        /// <summary>
        /// 创建条件门控阶段。
        /// </summary>
        public static AbilityGatePhase<TCtx> Gate<TCtx>(IAbilityConditionNode condition)
            where TCtx : IAbilityPipelineContext
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            return AbilityGatePhase<TCtx>.Create(condition);
        }

        private static void AddSubPhases<TCtx>(AbilityCompositePhase<TCtx> composite, IAbilityPipelinePhase<TCtx>[] phases)
            where TCtx : IAbilityPipelineContext
        {
            if (phases == null) return;

            for (int i = 0; i < phases.Length; i++)
            {
                if (phases[i] == null) throw new ArgumentNullException(nameof(phases));
                composite.AddSubPhase(phases[i]);
            }
        }

        private sealed class AlwaysConditionNode : IAbilityConditionNode
        {
            public static readonly AlwaysConditionNode Instance = new AlwaysConditionNode();

            public EConditionCheckStrategy CheckStrategy => EConditionCheckStrategy.OnEnter;

            public bool Evaluate(IAbilityPipelineContext context)
            {
                return true;
            }
        }
    }
}

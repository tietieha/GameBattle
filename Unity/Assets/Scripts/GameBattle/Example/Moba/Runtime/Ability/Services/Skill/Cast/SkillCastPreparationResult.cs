using System.Collections.Generic;
using AbilityKit.Ability;
using AbilityKit.Pipeline;

namespace AbilityKit.Demo.Moba.Services
{
    internal readonly struct SkillCastPreparationResult
    {
        private SkillCastPreparationResult(
            bool success,
            string failReason,
            in MobaSkillCastFailure failure,
            in SkillCastRequest request,
            SkillCastContext context,
            MobaSkillCastRuntimeService runtimes,
            IAbilityPipelineConfig preCastConfig,
            IReadOnlyList<IAbilityPipelinePhase<SkillPipelineContext>> preCastPhases,
            IAbilityPipelineConfig castConfig,
            IReadOnlyList<IAbilityPipelinePhase<SkillPipelineContext>> castPhases)
        {
            Success = success;
            FailReason = failReason;
            Failure = failure;
            Request = request;
            Context = context;
            Runtimes = runtimes;
            PreCastConfig = preCastConfig;
            PreCastPhases = preCastPhases;
            CastConfig = castConfig;
            CastPhases = castPhases;
        }

        public bool Success { get; }
        public string FailReason { get; }
        public MobaSkillCastFailure Failure { get; }
        public SkillCastRequest Request { get; }
        public SkillCastContext Context { get; }
        public MobaSkillCastRuntimeService Runtimes { get; }
        public IAbilityPipelineConfig PreCastConfig { get; }
        public IReadOnlyList<IAbilityPipelinePhase<SkillPipelineContext>> PreCastPhases { get; }
        public IAbilityPipelineConfig CastConfig { get; }
        public IReadOnlyList<IAbilityPipelinePhase<SkillPipelineContext>> CastPhases { get; }

        public static SkillCastPreparationResult Failed(string failReason)
        {
            return Failed("skill.cast.prepareFailed", failReason);
        }

        public static SkillCastPreparationResult Failed(string code, string failReason)
        {
            var failure = new MobaSkillCastFailure("Preparation", null, code, failReason);
            return new SkillCastPreparationResult(false, failReason, in failure, default, null, null, null, null, null, null);
        }

        public static SkillCastPreparationResult Ready(
            in SkillCastRequest request,
            SkillCastContext context,
            MobaSkillCastRuntimeService runtimes,
            IAbilityPipelineConfig preCastConfig,
            IReadOnlyList<IAbilityPipelinePhase<SkillPipelineContext>> preCastPhases,
            IAbilityPipelineConfig castConfig,
            IReadOnlyList<IAbilityPipelinePhase<SkillPipelineContext>> castPhases)
        {
            return new SkillCastPreparationResult(true, null, in MobaSkillCastFailure.None, in request, context, runtimes, preCastConfig, preCastPhases, castConfig, castPhases);
        }
    }
}

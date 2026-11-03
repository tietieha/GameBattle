using System;
using System.Collections.Generic;
using AbilityKit.Demo.Moba.Share.Config;

namespace AbilityKit.Demo.Moba.Config.BattleDemo.MO
{
    public sealed class SkillFlowMO
    {
        public int Id { get; }
        public string Name { get; }
        public int PipelineContinuousTagTemplateId { get; }
        public IReadOnlyList<SkillPhaseDTO> Phases { get; }

        public SkillFlowMO(SkillFlowDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            Id = dto.Id;
            Name = dto.Name;
            PipelineContinuousTagTemplateId = dto.PipelineContinuousTagTemplateId;
            Phases = dto.Phases ?? Array.Empty<SkillPhaseDTO>();
        }
    }
}

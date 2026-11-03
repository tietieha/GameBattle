using System;
using System.Collections.Generic;
using AbilityKit.Demo.Moba.Share.Config;

namespace AbilityKit.Demo.Moba.Config.BattleDemo.MO
{
    public sealed class PassiveSkillMO
    {
        public int Id { get; }
        public string Name { get; }
        public int CooldownMs { get; }
        public IReadOnlyList<int> TriggerIds { get; }
        public IReadOnlyList<int> ContinuousProcessIds { get; }

        public PassiveSkillMO(PassiveSkillDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            Id = dto.Id;
            Name = dto.Name;
            CooldownMs = dto.CooldownMs;
            TriggerIds = dto.TriggerIds ?? Array.Empty<int>();
            ContinuousProcessIds = dto.ContinuousProcessIds ?? Array.Empty<int>();
        }
    }
}

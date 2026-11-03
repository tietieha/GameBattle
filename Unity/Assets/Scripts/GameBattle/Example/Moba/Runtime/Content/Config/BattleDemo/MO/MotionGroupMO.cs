using System;
using AbilityKit.Combat.MotionSystem.Core;
using AbilityKit.Demo.Moba.Share.Config;

namespace AbilityKit.Demo.Moba.Config.BattleDemo.MO
{
    public sealed class MotionGroupMO
    {
        public int Id { get; }
        public string Key { get; }
        public string Name { get; }
        public int DefaultPriority { get; }
        public MotionStacking Stacking { get; }
        public int[] SuppressedGroupIds { get; }

        public MotionGroupMO(MotionGroupDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            Id = dto.Id;
            Key = dto.Key;
            Name = dto.Name;
            DefaultPriority = dto.DefaultPriority;
            Stacking = (MotionStacking)dto.Stacking;
            SuppressedGroupIds = dto.SuppressedGroupIds ?? Array.Empty<int>();
        }
    }
}

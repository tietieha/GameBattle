using AbilityKit.Core.Mathematics;
using AbilityKit.Core.Eventing;
using AbilityKit.Demo.Moba.Services.Buffs.Triggering;
using AbilityKit.Demo.Moba.Services;

namespace AbilityKit.Demo.Moba
{
    using AbilityKit.Demo.Moba;

    public static class MobaEventIds
    {
        public static class Common
        {
            public const string DamageApplied = "common.damage_applied";
        }

        public static class Ability
        {
            public const string SkillCastStarted = "ability.skill_cast_started";
            public const string SkillStage = "ability.skill_stage";
            public const string SkillCastEnded = "ability.skill_cast_ended";
            public const string SkillHitConfirmed = "ability.skill_hit_confirmed";
            public const string SkillCastInterrupted = "ability.skill_cast_interrupted";
            public const string SkillStageChanged = "ability.skill_stage_changed";
        }

        public static class Gameplay
        {
            public const string Entered = "gameplay.entered";
            public const string Exited = "gameplay.exited";
        }

        public static class Buff
        {
            public const string Applied = MobaBuffTriggering.Events.Apply;
            public const string Stacked = MobaBuffTriggering.Events.Stack;
            public const string Refreshed = MobaBuffTriggering.Events.Refresh;
            public const string Removed = MobaBuffTriggering.Events.Remove;
            public const string Ticked = MobaBuffTriggering.Events.Tick;
            public const string Ended = MobaBuffTriggering.Events.End;
        }
    }
}

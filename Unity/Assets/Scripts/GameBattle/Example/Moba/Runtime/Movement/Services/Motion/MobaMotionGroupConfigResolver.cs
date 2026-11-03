using AbilityKit.Ability.World.DI;
using AbilityKit.Combat.MotionSystem.Core;
using AbilityKit.Demo.Moba.Config.BattleDemo.MO;
using AbilityKit.Demo.Moba.Config.Core;

namespace AbilityKit.Demo.Moba.Services.Motion
{
    public readonly struct MobaMotionGroupSettings
    {
        public readonly int GroupId;
        public readonly int Priority;
        public readonly MotionStacking Stacking;

        public MobaMotionGroupSettings(int groupId, int priority, MotionStacking stacking)
        {
            GroupId = groupId;
            Priority = priority;
            Stacking = stacking;
        }
    }

    public static class MobaMotionGroupConfigResolver
    {
        public static MobaMotionGroupSettings Resolve(
            IWorldResolver services,
            int requestedGroupId,
            int fallbackGroupId,
            int requestedPriority,
            int fallbackPriority)
        {
            var groupId = requestedGroupId > 0 ? requestedGroupId : fallbackGroupId;
            if (TryGetGroup(services, groupId, out var group) && group != null)
            {
                var priority = requestedPriority > 0 ? requestedPriority : group.DefaultPriority;
                return new MobaMotionGroupSettings(group.Id, priority, group.Stacking);
            }

            return ResolveFallback(groupId, requestedPriority > 0 ? requestedPriority : fallbackPriority);
        }

        public static MotionPipelinePolicy CreatePolicy(IWorldResolver services)
        {
            var policy = new MotionPipelinePolicy();
            var hasConfiguredGroups = false;

            if (services != null && services.TryResolve<MobaConfigDatabase>(out var configs) && configs != null)
            {
                foreach (var group in configs.GetAllMotionGroups())
                {
                    if (group == null) continue;
                    hasConfiguredGroups = true;
                    if (group.SuppressedGroupIds != null && group.SuppressedGroupIds.Length > 0)
                    {
                        policy.SetSuppressedGroups(group.Id, group.SuppressedGroupIds);
                    }
                }
            }

            return hasConfiguredGroups ? policy : MotionPipelinePolicy.CreateDefault();
        }

        private static bool TryGetGroup(IWorldResolver services, int groupId, out MotionGroupMO group)
        {
            group = null;
            return services != null
                   && groupId > 0
                   && services.TryResolve<MobaConfigDatabase>(out var configs)
                   && configs != null
                   && configs.TryGetMotionGroup(groupId, out group);
        }

        private static MobaMotionGroupSettings ResolveFallback(int groupId, int priority)
        {
            var stacking = groupId == MotionGroups.Locomotion || groupId == MotionGroups.PassiveDisplacement
                ? MotionStacking.Additive
                : MotionStacking.OverrideLowerPriority;
            return new MobaMotionGroupSettings(groupId, priority, stacking);
        }
    }
}

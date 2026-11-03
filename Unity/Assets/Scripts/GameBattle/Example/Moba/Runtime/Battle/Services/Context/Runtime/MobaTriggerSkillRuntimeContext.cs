namespace AbilityKit.Demo.Moba.Services
{
    public interface IMobaTriggerRuntimeContext<TRuntime>
    {
        bool TryGetRuntime(out TRuntime runtime);
    }

    public interface IMobaTriggerSkillRuntimeContext
    {
        bool TryGetSkillRuntimeHandle(out MobaSkillCastRuntimeHandle handle);
    }

    public static class MobaTriggerSkillRuntimeContextExtensions
    {
        public static bool TryGetSkillRuntimeBlackboard(this IMobaTriggerSkillRuntimeContext context, MobaSkillCastRuntimeService runtimes, out MobaSkillRuntimeBlackboard blackboard)
        {
            blackboard = null;
            if (context == null || runtimes == null) return false;
            if (!context.TryGetSkillRuntimeHandle(out var handle)) return false;
            return runtimes.TryGetBlackboard(in handle, out blackboard);
        }

        public static bool TryMarkDamagedTarget(this IMobaTriggerSkillRuntimeContext context, MobaSkillCastRuntimeService runtimes, int actorId)
        {
            return context.TryGetSkillRuntimeBlackboard(runtimes, out var blackboard) && blackboard.AddActorId(in MobaSkillRuntimeBlackboardKeys.DamagedTargets, actorId);
        }

        public static bool HasDamagedTarget(this IMobaTriggerSkillRuntimeContext context, MobaSkillCastRuntimeService runtimes, int actorId)
        {
            return context.TryGetSkillRuntimeBlackboard(runtimes, out var blackboard) && blackboard.ContainsActorId(in MobaSkillRuntimeBlackboardKeys.DamagedTargets, actorId);
        }

        public static int AddSkillRuntimeHitCount(this IMobaTriggerSkillRuntimeContext context, MobaSkillCastRuntimeService runtimes, int delta = 1)
        {
            return context.TryGetSkillRuntimeBlackboard(runtimes, out var blackboard) ? blackboard.AddInt(in MobaSkillRuntimeBlackboardKeys.HitCount, delta) : 0;
        }

        public static bool TryAddLoopGuard(this IMobaTriggerSkillRuntimeContext context, MobaSkillCastRuntimeService runtimes, long contextId)
        {
            return context.TryGetSkillRuntimeBlackboard(runtimes, out var blackboard) && blackboard.AddContextId(in MobaSkillRuntimeBlackboardKeys.LoopGuards, contextId);
        }

        public static bool HasLoopGuard(this IMobaTriggerSkillRuntimeContext context, MobaSkillCastRuntimeService runtimes, long contextId)
        {
            return context.TryGetSkillRuntimeBlackboard(runtimes, out var blackboard) && blackboard.ContainsContextId(in MobaSkillRuntimeBlackboardKeys.LoopGuards, contextId);
        }
    }
}

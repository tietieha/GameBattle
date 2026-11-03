using AbilityKit.Ability.Flow;
using AbilityKit.Ability.Flow.Blocks;
using AbilityKit.Ability.Flow.Pooling;

namespace AbilityKit.FlowExamples
{
    public static class BasicSessionExample
    {
        public static FlowStatus RunOnce(float deltaTime)
        {
            FlowPools.RegisterDefaultConfig();

            using var session = new FlowSession();

            var root = new DoNode(
                onEnter: _ => { },
                onTick: (_, __) => FlowStatus.Succeeded,
                onExit: _ => { },
                onInterrupt: _ => { }
            );

            session.Start(root);
            return session.Step(deltaTime);
        }
    }
}

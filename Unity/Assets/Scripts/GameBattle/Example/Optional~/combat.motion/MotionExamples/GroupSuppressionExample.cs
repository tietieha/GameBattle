using AbilityKit.Combat.MotionSystem.Core;
using AbilityKit.Combat.MotionSystem.Generic;
using AbilityKit.Core.Mathematics;

namespace AbilityKit.Combat.MotionSystem.Examples
{
    public static class GroupSuppressionExample
    {
        public static void Example_ControlSuppressLocomotion(int id)
        {
            var pipeline = MotionPipelinePool.Rent();
            var locomotion = LocomotionMotionSource.Rent(speed: 4.5f, space: MotionInputSpace.Local, priority: 0);
            var knockback = FixedDeltaMotionSource.Rent(
                deltaPerSecond: new Vec3(0f, 0f, -6f),
                duration: 0.3f,
                priority: 100,
                groupId: MotionGroups.Control,
                stacking: MotionStacking.OverrideLowerPriority);

            try
            {
                pipeline.Policy = MotionPipelinePolicy.CreateDefault();

                locomotion.SetInput(0f, 1f);
                pipeline.AddSource(locomotion);

                // Control source: for 0.3s, push backward and suppress locomotion by policy.
                pipeline.AddSource(knockback);

                var state = new MotionState(new Vec3(0f, 0f, 0f))
                {
                    Forward = new Vec3(0f, 0f, 1f),
                };

                var output = new MotionOutput();

                // During knockback active window, locomotion will be suppressed.
                pipeline.Tick(id, ref state, 0.1f, ref output);
            }
            finally
            {
                pipeline.RemoveSource(knockback);
                pipeline.RemoveSource(locomotion);
                FixedDeltaMotionSource.Release(knockback);
                LocomotionMotionSource.Release(locomotion);
                MotionPipelinePool.Release(pipeline);
            }
        }
    }
}

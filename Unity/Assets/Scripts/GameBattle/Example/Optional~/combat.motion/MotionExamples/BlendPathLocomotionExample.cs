using AbilityKit.Combat.MotionSystem.Core;
using AbilityKit.Combat.MotionSystem.Generic;
using AbilityKit.Combat.MotionSystem.Trajectory;
using AbilityKit.Core.Mathematics;

namespace AbilityKit.Combat.MotionSystem.Examples
{
    public static class BlendPathLocomotionExample
    {
        public static void Example_PathWithInputSteering(int id)
        {
            var points = new[]
            {
                new Vec3(0f, 0f, 0f),
                new Vec3(5f, 0f, 0f),
                new Vec3(10f, 0f, 5f),
            };

            var traj = new WaypointTrajectory3D(points, speed: 4f);

            var pipeline = MotionPipelinePool.Rent();
            var path = TrajectoryMotionSource.Rent(traj, priority: 10, groupId: MotionGroups.Path, stacking: MotionStacking.Additive);
            var locomotion = LocomotionMotionSource.Rent(speed: 4.5f, space: MotionInputSpace.Local, priority: 0);
            var steering = ScaledMotionSource.Rent(locomotion, scale: 0.3f);

            try
            {
                // Path contributes as Path group but additive, so it can be blended with locomotion.
                pipeline.AddSource(path);

                locomotion.SetInput(1f, 0f);
                // Input steering weight: 30%.
                pipeline.AddSource(steering);

                var state = new MotionState(points[0]);
                state.Forward = new Vec3(0f, 0f, 1f);
                var output = new MotionOutput();

                pipeline.Tick(id, ref state, 0.1f, ref output);
            }
            finally
            {
                pipeline.RemoveSource(steering);
                pipeline.RemoveSource(path);
                ScaledMotionSource.Release(steering);
                LocomotionMotionSource.Release(locomotion);
                TrajectoryMotionSource.Release(path);
                MotionPipelinePool.Release(pipeline);
            }
        }
    }
}

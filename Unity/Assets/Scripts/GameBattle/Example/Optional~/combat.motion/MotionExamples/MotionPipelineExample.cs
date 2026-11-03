using AbilityKit.Combat.MotionSystem.Core;
using AbilityKit.Combat.MotionSystem.Trajectory;
using AbilityKit.Core.Mathematics;

namespace AbilityKit.Combat.MotionSystem.Examples
{
    public static class MotionPipelineExample
    {
        public static void Example_LinearTrajectoryFixedStep(int id)
        {
            var start = new Vec3(0f, 0f, 0f);
            var end = new Vec3(10f, 0f, 0f);

            var traj = new LinearTrajectory3D(start, end, duration: 1.0f);

            var pipeline = MotionPipelinePool.Rent();
            var locomotion = LocomotionMotionSource.Rent(speed: 4.5f, space: MotionInputSpace.Local, priority: 0);
            var dash = TrajectoryMotionSource.Rent(traj);

            try
            {
                locomotion.SetInput(0f, 1f);

                pipeline.AddSource(locomotion);
                pipeline.AddSource(dash);

                var state = new MotionState(start);
                state.Forward = new Vec3(0f, 0f, 1f);
                var output = new MotionOutput();

                var runner = new FixedStepRunner(0.02f);
                var steps = runner.Accumulate(0.33f);
                for (int i = 0; i < steps; i++)
                {
                    pipeline.Tick(id, ref state, runner.ConsumeOneStep(), ref output);
                }
            }
            finally
            {
                pipeline.RemoveSource(dash);
                pipeline.RemoveSource(locomotion);
                TrajectoryMotionSource.Release(dash);
                LocomotionMotionSource.Release(locomotion);
                MotionPipelinePool.Release(pipeline);
            }
        }
    }
}

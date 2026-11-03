using System;
using AbilityKit.Combat.MotionSystem.Constraints;
using AbilityKit.Combat.MotionSystem.Core;

namespace AbilityKit.Combat.MotionSystem.Collision
{
    public interface IMotionSolverDiagnostics
    {
        void OnConstraintsProviderException(int moverId, in MotionState state, in MotionOutput input, float dt, Exception exception);

        void OnEndOverlapResolved(int moverId, in MotionState state, in MotionCollisionConstraints constraints, MotionEndOverlapPolicy policy, bool resolved);
    }
}

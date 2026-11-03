using AbilityKit.Combat.MotionSystem.Core;
using AbilityKit.Combat.MotionSystem.Collision;

namespace AbilityKit.Combat.MotionSystem.Events
{
    public interface IMotionEventSink
    {
        void OnHit(int id, in MotionState state, in MotionHit hit);
        void OnArrive(int id, in MotionState state);
        void OnExpired(int id, in MotionState state);
    }
}

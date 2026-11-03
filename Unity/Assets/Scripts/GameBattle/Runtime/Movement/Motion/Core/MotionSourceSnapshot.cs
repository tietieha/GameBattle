using AbilityKit.Core.Mathematics;

namespace AbilityKit.Combat.MotionSystem.Core
{
    public struct MotionSourceSnapshot
    {
        public int GroupId;
        public int Priority;
        public MotionStacking Stacking;
        public bool IsActive;
        public float Time;
        public float TimeLeft;
        public int Index;
        public Vec3 Vector0;
        public Vec3 Vector1;
        public float Float0;
        public float Float1;
    }
}

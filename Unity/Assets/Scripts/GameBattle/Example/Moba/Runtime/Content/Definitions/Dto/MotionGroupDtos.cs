using System;

namespace AbilityKit.Demo.Moba.Share.Config
{
    [Serializable]
    public sealed class MotionGroupDTO
    {
        public int Id;
        public string Key;
        public string Name;
        public int DefaultPriority;
        public int Stacking;
        public int[] SuppressedGroupIds;
    }
}

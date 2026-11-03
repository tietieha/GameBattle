using System;

namespace AbilityKit.Demo.Moba.Share.Config
{
    [Serializable]
    public sealed class GameplayDTO
    {
        public int Id;
        public string Name;
        public int[] TriggerIds;
        public int DefaultDurationMs;
        public int WinPolicy;
        public int[] Tags;
    }
}

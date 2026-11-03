using UnityEngine;
using UnityTime = UnityEngine.Time;

namespace AbilityKit.Game.Battle.Shared.Time
{
    /// <summary>
    /// 战斗表现层统一时间源，隔离 UnityEngine.Time 的直接依赖，便于测试、回放和远端驱动。
    /// </summary>
    public interface IBattleViewTimeSource
    {
        float TimeSeconds { get; }
        int FrameCount { get; }
    }

    public sealed class UnityBattleViewTimeSource : IBattleViewTimeSource
    {
        public static readonly UnityBattleViewTimeSource Shared = new UnityBattleViewTimeSource();

        public float TimeSeconds => UnityTime.time;

        public int FrameCount => UnityTime.frameCount;
    }

    public sealed class ManualBattleViewTimeSource : IBattleViewTimeSource
    {
        public float TimeSeconds { get; set; }

        public int FrameCount { get; set; }
    }
}

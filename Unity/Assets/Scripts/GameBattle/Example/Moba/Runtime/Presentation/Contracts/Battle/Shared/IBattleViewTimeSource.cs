namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.Shared
{
    public interface IBattleViewTimeSource
    {
        float TimeSeconds { get; }
        int FrameCount { get; }
    }

    public sealed class ManualBattleViewTimeSource : IBattleViewTimeSource
    {
        public float TimeSeconds { get; set; }

        public int FrameCount { get; set; }
    }
}

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public static class BattleViewSampleTimeResolver
    {
        public static double Resolve(double lastFrame, int tickRate)
        {
            if (tickRate <= 0) tickRate = 30;
            return lastFrame / tickRate;
        }
    }
}

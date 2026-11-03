namespace AbilityKit.Game.Flow
{
    public sealed class BootPhase : IGamePhase
    {
        public void Enter(in GamePhaseContext ctx)
        {
            var featureInstaller = ctx.Entry.Get<IGameFlowFeatureInstaller>();
            featureInstaller.AttachBootFeatures();
        }

        public void Exit(in GamePhaseContext ctx)
        {
        }

        public void Tick(in GamePhaseContext ctx, float deltaTime)
        {
        }
    }
}

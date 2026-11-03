namespace AbilityKit.Demo.Moba.Worlds.Blueprints
{
    public sealed class MobaLobbyWorldBlueprint : MobaLogicWorldBlueprintBase
    {
        public const string Type = "lobby";

        public override string WorldType => Type;

        protected override MobaLogicWorldProfile Profile => MobaLogicWorldProfile.Lobby;

        protected override MobaLogicWorldFeatures Features => MobaLogicWorldFeatures.EntitasContexts | MobaLogicWorldFeatures.Config;
    }
}

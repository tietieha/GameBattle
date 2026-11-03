namespace AbilityKit.Game.Flow
{
    public sealed partial class BattleSessionFeature
    {
        private void DisposeNetworkIoDispatcher()
        {
            _handles.Dispatchers.DisposeNetworkIoDispatcher();
        }
    }
}

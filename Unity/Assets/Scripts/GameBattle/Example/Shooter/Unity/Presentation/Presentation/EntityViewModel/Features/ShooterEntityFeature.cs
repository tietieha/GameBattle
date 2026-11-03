namespace AbilityKit.Demo.Shooter.View.EntityViewModel
{
    public sealed class ShooterEntityFeature
    {
        private ShooterEntityLookup _lookup;
        private ShooterEntityFactory _factory;
        private IShooterEntityQuery _query;

        public ShooterEntityLookup Lookup => _lookup;
        public ShooterEntityFactory Factory => _factory;
        public IShooterEntityQuery Query => _query;

        public void Initialize()
        {
            _lookup = new ShooterEntityLookup();
            _factory = new ShooterEntityFactory(_lookup);
            _query = new ShooterEntityQuery(_lookup);
        }

        public void Shutdown()
        {
            _lookup?.Clear();
            _lookup = null;
            _factory = null;
            _query = null;
        }
    }
}
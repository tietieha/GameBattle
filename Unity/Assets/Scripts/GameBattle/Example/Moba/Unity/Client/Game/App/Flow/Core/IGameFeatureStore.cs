namespace AbilityKit.Game.Flow
{
    public interface IGameFeatureStore
    {
        bool TryGet<T>(out T component) where T : class;
        void Set<T>(T component) where T : class;
        void Remove<T>() where T : class;
        void Remove(System.Type componentType);
    }
}

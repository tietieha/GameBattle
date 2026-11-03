namespace AbilityKit.Core.Pooling
{
    internal static class PoolableExtensions
    {
        public static void TryOnPoolGet<T>(this T element) where T : class
        {
            if (element is IPoolable poolable)
            {
                poolable.OnPoolGet();
            }
        }

        public static void TryOnPoolRelease<T>(this T element) where T : class
        {
            if (element is IPoolable poolable)
            {
                poolable.OnPoolRelease();
            }
        }

        public static void TryOnPoolDestroy<T>(this T element) where T : class
        {
            if (element is IPoolable poolable)
            {
                poolable.OnPoolDestroy();
            }
        }
    }
}

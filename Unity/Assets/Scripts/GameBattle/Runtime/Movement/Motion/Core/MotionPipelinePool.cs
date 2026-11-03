using System.Collections.Generic;
using AbilityKit.Core.Pooling;

namespace AbilityKit.Combat.MotionSystem.Core
{
    public static class MotionPipelinePool
    {
        private static readonly ObjectPool<List<IMotionSource>> SourceListPool = Pools.GetPool(
            createFunc: () => new List<IMotionSource>(4),
            onRelease: l => l.Clear(),
            defaultCapacity: 64,
            maxSize: 2048,
            collectionCheck: false);

        private static readonly ObjectPool<Dictionary<int, int>> BestIndexDictionaryPool = Pools.GetPool(
            createFunc: () => new Dictionary<int, int>(8),
            onRelease: d => d.Clear(),
            defaultCapacity: 64,
            maxSize: 2048,
            collectionCheck: false);

        private static readonly ObjectPool<List<int>> IntListPool = Pools.GetPool(
            createFunc: () => new List<int>(8),
            onRelease: l => l.Clear(),
            defaultCapacity: 64,
            maxSize: 2048,
            collectionCheck: false);

        private static readonly ObjectPool<MotionPipeline> Pool = Pools.GetPool(
            createFunc: () => new MotionPipeline(),
            onRelease: p => p.Reset(),
            defaultCapacity: 32,
            maxSize: 1024,
            collectionCheck: false);

        public static MotionPipeline Rent()
        {
            return Pool.Get();
        }

        public static void Release(MotionPipeline pipeline)
        {
            if (pipeline == null) return;
            Pool.Release(pipeline);
        }

        internal static List<IMotionSource> RentSourceList()
        {
            return SourceListPool.Get();
        }

        internal static void ReleaseSourceList(List<IMotionSource> list)
        {
            if (list == null) return;
            SourceListPool.Release(list);
        }

        internal static Dictionary<int, int> RentBestIndexDictionary()
        {
            return BestIndexDictionaryPool.Get();
        }

        internal static void ReleaseBestIndexDictionary(Dictionary<int, int> dictionary)
        {
            if (dictionary == null) return;
            BestIndexDictionaryPool.Release(dictionary);
        }

        internal static List<int> RentIntList()
        {
            return IntListPool.Get();
        }

        internal static void ReleaseIntList(List<int> list)
        {
            if (list == null) return;
            IntListPool.Release(list);
        }
    }
}

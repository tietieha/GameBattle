using System.Collections.Generic;
using AbilityKit.Buffs;
using AbilityKit.Demo.Moba.Components;
using AbilityKit.Core.Pooling;

namespace AbilityKit.Demo.Moba.Services.Buffs.Core
{
    /// <summary>Entitas storage and pool adapter; indexed instance operations are owned by Runtime.</summary>
    internal sealed class BuffRepository
    {
        private static readonly ObjectPool<List<BuffRuntime>> s_runtimeListPool = Pools.GetPool(
            createFunc: () => new List<BuffRuntime>(8),
            onRelease: list => list.Clear(),
            defaultCapacity: 32, maxSize: 256, collectionCheck: false);

        private static readonly ObjectPool<BuffRuntime> s_runtimePool = Pools.GetPool(
            createFunc: () => new BuffRuntime(),
            onRelease: runtime => new BuffRuntimeView(runtime).ClearRuntimeBindings(),
            defaultCapacity: 64, maxSize: 2048, collectionCheck: false);

        public List<BuffRuntime> GetOrCreateList(global::ActorEntity target)
        {
            var list = target != null && target.hasBuffs ? target.buffs.Active : null;
            if (list != null) return list;
            list = s_runtimeListPool.Get();
            target?.ReplaceBuffs(list);
            return list;
        }

        public static BuffRuntime RentRuntime() => s_runtimePool.Get();
        public static void ReleaseRuntime(BuffRuntime runtime)
        {
            if (runtime != null) s_runtimePool.Release(runtime);
        }

        public static void ReleaseList(global::ActorEntity target)
        {
            if (target == null || !target.hasBuffs || target.buffs.Active == null) return;
            var list = target.buffs.Active;
            BuffStorage<BuffRuntime>.Forget(list);
            s_runtimeListPool.Release(list);
            target.ReplaceBuffs(null);
        }

        public static int FindExistingBuffIndex(List<BuffRuntime> list, in BuffRuntimeKey key)
        {
            var sharedKey = key.SharedKey;
            return BuffStorage<BuffRuntime>.FindExistingBuffIndex(list, in sharedKey);
        }

        public static bool TryGetRuntime(List<BuffRuntime> list, in BuffRuntimeKey key, out BuffRuntime runtime, out int index)
        {
            var sharedKey = key.SharedKey;
            return BuffStorage<BuffRuntime>.TryGetRuntime(list, in sharedKey, out runtime, out index);
        }

        public static void RegisterRuntime(List<BuffRuntime> list, BuffRuntime runtime) =>
            BuffStorage<BuffRuntime>.RegisterRuntime(list, runtime);
        public static void MarkDirty(List<BuffRuntime> list) => BuffStorage<BuffRuntime>.MarkDirty(list);
        public static bool RemoveAt(List<BuffRuntime> list, int index, BuffRuntime expectedRuntime) =>
            BuffStorage<BuffRuntime>.RemoveAt(list, index, expectedRuntime);
        public static bool ReplaceAt(List<BuffRuntime> list, int index, BuffRuntime expectedRuntime, BuffRuntime replacement) =>
            BuffStorage<BuffRuntime>.ReplaceAt(list, index, expectedRuntime, replacement);
    }
}

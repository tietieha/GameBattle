using AbilityKit.Game.Flow.Battle.View;
using UnityEngine;
using EC = AbilityKit.World.ECS;

namespace AbilityKit.Game.Flow
{
    internal sealed class ViewSeekableRegistry
    {
        public void RegisterAll(IViewFeatureRuntime runtime)
        {
            if (runtime?.Timeline == null || runtime.Binder == null) return;

            runtime.Timeline.Clear();
            runtime.Binder.ForEachShellGameObject((actorId, entityId, go) => RegisterOnGameObject(runtime, go));
            runtime.LastAlignedFrame = int.MinValue;
        }

        public void RegisterForEntity(IViewFeatureRuntime runtime, EC.IEntityId id)
        {
            if (runtime?.Timeline == null || runtime.Binder == null) return;
            if (!runtime.Binder.TryGetShellGameObject(id, out var go)) return;

            RegisterOnGameObject(runtime, go);
            runtime.LastAlignedFrame = int.MinValue;
        }

        private void RegisterOnGameObject(IViewFeatureRuntime runtime, GameObject go)
        {
            if (runtime?.Timeline == null) return;
            if (go == null) return;

            var monos = go.GetComponentsInChildren<MonoBehaviour>(true);
            if (monos == null || monos.Length == 0) return;

            for (int i = 0; i < monos.Length; i++)
            {
                if (monos[i] is IFrameSeekableView seekable)
                {
                    runtime.Timeline.Register(seekable);
                }
            }
        }
    }
}

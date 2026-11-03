using UnityEngine;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleViewShellHandleBinder
    {
        private readonly IMonoViewHandleRegistry _registry;

        public BattleViewShellHandleBinder(IMonoViewHandleRegistry registry)
        {
            _registry = registry;
        }

        public void Bind(BattleViewHandle handle, GameObject go, int actorId)
        {
            if (handle == null) return;
            if (go == null) return;

            var viewHandle = go.GetComponent<MonoViewHandle>();
            if (viewHandle == null) viewHandle = go.AddComponent<MonoViewHandle>();
            viewHandle.ActorId = actorId;
            viewHandle.Registry = _registry;
            handle.ViewHandle = viewHandle;
        }

        public void Unbind(BattleViewHandle handle)
        {
            if (handle?.ViewHandle == null) return;

            handle.ViewHandle.Registry = null;
            handle.ViewHandle = null;
        }
    }
}

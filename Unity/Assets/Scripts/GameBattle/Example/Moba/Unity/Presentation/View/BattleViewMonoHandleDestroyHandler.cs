using System;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleViewMonoHandleDestroyHandler
    {
        private readonly BattleViewHandleStore _handles;

        public BattleViewMonoHandleDestroyHandler(BattleViewHandleStore handles)
        {
            _handles = handles ?? throw new ArgumentNullException(nameof(handles));
        }

        public void OnDestroyed(MonoViewHandle handle)
        {
            if (handle == null) return;
            if (handle.ActorId <= 0) return;
            if (!_handles.TryGetByActorId(handle.ActorId, out _, out var viewHandle)) return;

            if (!ReferenceEquals(viewHandle.ViewHandle, handle)) return;

            viewHandle.GameObject = null;
            viewHandle.ViewHandle = null;
        }
    }
}

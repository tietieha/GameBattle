using System.Collections.Generic;

namespace AbilityKit.Demo.Shooter.View.EntityViewModel
{
    public sealed class ShooterEntityLookup
    {
        private readonly Dictionary<uint, object> _entityIdToData = new Dictionary<uint, object>();

        public int Count => _entityIdToData.Count;

        public void Bind(uint entityId, object data)
        {
            _entityIdToData[entityId] = data;
        }

        public bool TryResolve(uint entityId, out object data)
        {
            return _entityIdToData.TryGetValue(entityId, out data);
        }

        public bool Unbind(uint entityId)
        {
            return _entityIdToData.Remove(entityId);
        }

        public void Clear()
        {
            _entityIdToData.Clear();
        }
    }
}
using System;
using System.Collections.Generic;

namespace AbilityKit.Game.View.Flow
{
    public sealed class PhaseSwitchFlowCatalog
    {
        private readonly HashSet<string> _ids;

        public PhaseSwitchFlowCatalog(int initialCapacity = 8)
        {
            _ids = new HashSet<string>(StringComparer.Ordinal);
        }

        public int Count => _ids.Count;

        public PhaseSwitchFlowCatalog Add(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Switch flow id is required.", nameof(id));

            _ids.Add(id);
            return this;
        }

        public bool Contains(string? id)
        {
            return !string.IsNullOrEmpty(id) && _ids.Contains(id);
        }
    }
}

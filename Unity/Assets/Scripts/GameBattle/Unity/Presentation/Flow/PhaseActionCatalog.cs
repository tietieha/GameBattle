using System;
using System.Collections.Generic;

namespace AbilityKit.Game.View.Flow
{
    public sealed class PhaseActionCatalog
    {
        private readonly HashSet<string> _ids;

        public PhaseActionCatalog(int initialCapacity = 8)
        {
            _ids = new HashSet<string>(StringComparer.Ordinal);
        }

        public int Count => _ids.Count;

        public PhaseActionCatalog Add(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Action id is required.", nameof(id));

            _ids.Add(id);
            return this;
        }

        public bool Contains(string? id)
        {
            return !string.IsNullOrEmpty(id) && _ids.Contains(id);
        }
    }
}

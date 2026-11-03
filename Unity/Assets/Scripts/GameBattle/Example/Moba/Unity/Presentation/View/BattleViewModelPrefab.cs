using UnityEngine;

namespace AbilityKit.Game.Flow
{
    internal readonly struct BattleViewModelPrefab
    {
        public BattleViewModelPrefab(GameObject prefab, float scale)
        {
            Prefab = prefab;
            Scale = scale;
        }

        public GameObject Prefab { get; }
        public float Scale { get; }
        public bool HasPrefab => Prefab != null;
    }
}

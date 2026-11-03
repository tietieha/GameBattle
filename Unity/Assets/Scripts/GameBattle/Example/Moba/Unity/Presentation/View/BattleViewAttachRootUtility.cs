using UnityEngine;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleViewAttachRootUtility
    {
        public void Ensure(GameObject go)
        {
            if (go == null) return;
            if (go.transform.Find("AttachRoot") != null) return;

            var attachRoot = new GameObject("AttachRoot");
            attachRoot.transform.SetParent(go.transform, worldPositionStays: false);
            attachRoot.transform.localPosition = Vector3.zero;
        }
    }
}

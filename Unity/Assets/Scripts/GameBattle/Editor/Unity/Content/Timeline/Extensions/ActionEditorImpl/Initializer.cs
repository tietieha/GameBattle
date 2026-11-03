using NBC.ActionEditor;
using UnityEditor;

namespace ActionEditorExample.Editor
{
    public class Initializer
    {
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            App.OnInitialize -= ActionSequencerInit;

            App.OnOpenAsset -= OpenAssetData;
            App.OnOpenAsset += OpenAssetData;

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void ActionSequencerInit()
        {
            // Opening the timeline editor must not replace the user's active scene.
        }

        static void OnEditorUpdate()
        {
        }

        private static void OpenAssetData(Asset asset)
        {
            //打开资源做的处理
        }

    }
}

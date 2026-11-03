using AbilityKit.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AbilityKit.Core.Unity
{
    internal static class RuntimeEnvironmentBootstrap
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            RuntimeEnvironment.PersistentDataDirectoryProvider = () =>
            {
                var directory = Application.persistentDataPath;
                return string.IsNullOrEmpty(directory) ? Application.dataPath : directory;
            };
            RuntimeEnvironment.WarningHandler = message => Debug.LogWarning(message);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializePipeline()
        {
            _ = AbilityKit.Pipeline.Pipeline.DefaultRuntime;
        }
    }
}

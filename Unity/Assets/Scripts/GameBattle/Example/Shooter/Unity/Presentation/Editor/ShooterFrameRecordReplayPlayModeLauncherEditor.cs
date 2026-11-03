#nullable enable

using AbilityKit.Demo.Shooter.View.PlayMode;
using UnityEditor;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.View.Editor
{
    [CustomEditor(typeof(ShooterFrameRecordReplayPlayModeLauncher))]
    public sealed class ShooterFrameRecordReplayPlayModeLauncherEditor : UnityEditor.Editor
    {
        private SerializedProperty? _profile;
        private SerializedProperty? _profileCatalog;
        private SerializedProperty? _replayPathOverride;
        private SerializedProperty? _startOnEnable;
        private SerializedProperty? _stopOnDisable;
        private SerializedProperty? _isRunning;
        private SerializedProperty? _stepCount;
        private SerializedProperty? _renderCount;
        private SerializedProperty? _replayFrameCursor;
        private SerializedProperty? _replayInputFrameCount;
        private SerializedProperty? _selectedProfileName;
        private SerializedProperty? _resolvedReplayPath;
        private SerializedProperty? _lastError;

        private void OnEnable()
        {
            _profile = serializedObject.FindProperty("profile");
            _profileCatalog = serializedObject.FindProperty("profileCatalog");
            _replayPathOverride = serializedObject.FindProperty("replayPathOverride");
            _startOnEnable = serializedObject.FindProperty("startOnEnable");
            _stopOnDisable = serializedObject.FindProperty("stopOnDisable");
            _isRunning = serializedObject.FindProperty("isRunning");
            _stepCount = serializedObject.FindProperty("stepCount");
            _renderCount = serializedObject.FindProperty("renderCount");
            _replayFrameCursor = serializedObject.FindProperty("replayFrameCursor");
            _replayInputFrameCount = serializedObject.FindProperty("replayInputFrameCount");
            _selectedProfileName = serializedObject.FindProperty("selectedProfileName");
            _resolvedReplayPath = serializedObject.FindProperty("resolvedReplayPath");
            _lastError = serializedObject.FindProperty("lastError");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ShooterPlayModeProfileInspectorGui.DrawCatalogControls(Required(_profile), Required(_profileCatalog));

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Replay Override", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(Required(_replayPathOverride));
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(Required(_startOnEnable));
                EditorGUILayout.PropertyField(Required(_stopOnDisable));

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = Application.isPlaying;
                    if (GUILayout.Button("Start Replay"))
                    {
                        ((ShooterFrameRecordReplayPlayModeLauncher)target).StartReplay();
                    }

                    if (GUILayout.Button("Stop Replay"))
                    {
                        ((ShooterFrameRecordReplayPlayModeLauncher)target).StopReplay();
                    }

                    GUI.enabled = true;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Previous Template"))
                    {
                        ((ShooterFrameRecordReplayPlayModeLauncher)target).UsePreviousReplayTemplate();
                    }

                    if (GUILayout.Button("Next Template"))
                    {
                        ((ShooterFrameRecordReplayPlayModeLauncher)target).UseNextReplayTemplate();
                    }
                }

                if (GUILayout.Button("Rebuild Replay Views"))
                {
                    ((ShooterFrameRecordReplayPlayModeLauncher)target).RebuildViews();
                }
            }

            ShooterPlayModeProfileInspectorGui.DrawStatus(
                Required(_isRunning),
                Required(_stepCount),
                Required(_renderCount),
                Required(_replayFrameCursor),
                Required(_replayInputFrameCount),
                Required(_selectedProfileName),
                Required(_resolvedReplayPath),
                Required(_lastError));

            serializedObject.ApplyModifiedProperties();
        }

        private static SerializedProperty Required(SerializedProperty? property)
        {
            return property ?? throw new MissingReferenceException("Shooter replay launcher inspector binding is missing.");
        }
    }
}

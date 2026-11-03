#nullable enable

using AbilityKit.Demo.Shooter.View.PlayMode;
using UnityEditor;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.View.Editor
{
    [CustomEditor(typeof(ShooterRemoteStateSyncPlayModeLauncher))]
    public sealed class ShooterRemoteStateSyncPlayModeLauncherEditor : UnityEditor.Editor
    {
        private SerializedProperty? _profile;
        private SerializedProperty? _profileCatalog;
        private SerializedProperty? _overrideLaunchMode;
        private SerializedProperty? _launchModeOverride;
        private SerializedProperty? _sessionTokenOverride;
        private SerializedProperty? _roomIdOverride;
        private SerializedProperty? _startOnEnable;
        private SerializedProperty? _stopOnDisable;
        private SerializedProperty? _isRunning;
        private SerializedProperty? _isStarting;
        private SerializedProperty? _selectedProfileName;
        private SerializedProperty? _currentRoomId;
        private SerializedProperty? _lastError;

        private void OnEnable()
        {
            _profile = serializedObject.FindProperty("profile");
            _profileCatalog = serializedObject.FindProperty("profileCatalog");
            _overrideLaunchMode = serializedObject.FindProperty("overrideLaunchMode");
            _launchModeOverride = serializedObject.FindProperty("launchModeOverride");
            _sessionTokenOverride = serializedObject.FindProperty("sessionTokenOverride");
            _roomIdOverride = serializedObject.FindProperty("roomIdOverride");
            _startOnEnable = serializedObject.FindProperty("startOnEnable");
            _stopOnDisable = serializedObject.FindProperty("stopOnDisable");
            _isRunning = serializedObject.FindProperty("isRunning");
            _isStarting = serializedObject.FindProperty("isStarting");
            _selectedProfileName = serializedObject.FindProperty("selectedProfileName");
            _currentRoomId = serializedObject.FindProperty("currentRoomId");
            _lastError = serializedObject.FindProperty("lastError");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ShooterPlayModeProfileInspectorGui.DrawCatalogControls(Required(_profile), Required(_profileCatalog));

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Runtime Overrides", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(Required(_overrideLaunchMode));
                if (Required(_overrideLaunchMode).boolValue)
                {
                    EditorGUILayout.PropertyField(Required(_launchModeOverride));
                }

                EditorGUILayout.PropertyField(Required(_sessionTokenOverride));
                EditorGUILayout.PropertyField(Required(_roomIdOverride));
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Lifecycle", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(Required(_startOnEnable));
                EditorGUILayout.PropertyField(Required(_stopOnDisable));

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = Application.isPlaying;
                    if (GUILayout.Button("Start"))
                    {
                        ((ShooterRemoteStateSyncPlayModeLauncher)target).StartRemote();
                    }

                    if (GUILayout.Button("Stop"))
                    {
                        ((ShooterRemoteStateSyncPlayModeLauncher)target).StopRemote();
                    }

                    GUI.enabled = true;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Previous Template"))
                    {
                        ((ShooterRemoteStateSyncPlayModeLauncher)target).UsePreviousProfileTemplate();
                    }

                    if (GUILayout.Button("Next Template"))
                    {
                        ((ShooterRemoteStateSyncPlayModeLauncher)target).UseNextProfileTemplate();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Rebuild Views"))
                    {
                        ((ShooterRemoteStateSyncPlayModeLauncher)target).RebuildViews();
                    }

                    if (GUILayout.Button("Copy Room Id"))
                    {
                        ((ShooterRemoteStateSyncPlayModeLauncher)target).CopyCurrentRoomId();
                    }
                }
            }

            ShooterPlayModeProfileInspectorGui.DrawStatus(
                Required(_isRunning),
                Required(_isStarting),
                Required(_selectedProfileName),
                Required(_currentRoomId),
                Required(_lastError));

            serializedObject.ApplyModifiedProperties();
        }

        private static SerializedProperty Required(SerializedProperty? property)
        {
            return property ?? throw new MissingReferenceException("Shooter remote launcher inspector binding is missing.");
        }
    }
}

#nullable enable

using AbilityKit.Demo.Shooter.View.PlayMode;
using UnityEditor;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.View.Editor
{
    [CustomEditor(typeof(ShooterFrameRecordReplayPlayModeProfile))]
    public sealed class ShooterFrameRecordReplayPlayModeProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty? _replayPath;
        private SerializedProperty? _syncTemplateId;
        private SerializedProperty? _controlledPlayerId;
        private SerializedProperty? _randomSeed;
        private SerializedProperty? _worldScale;
        private SerializedProperty? _timeScale;

        private void OnEnable()
        {
            _replayPath = serializedObject.FindProperty("replayPath");
            _syncTemplateId = serializedObject.FindProperty("syncTemplateId");
            _controlledPlayerId = serializedObject.FindProperty("controlledPlayerId");
            _randomSeed = serializedObject.FindProperty("randomSeed");
            _worldScale = serializedObject.FindProperty("worldScale");
            _timeScale = serializedObject.FindProperty("timeScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Replay Source", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(Required(_replayPath));
            }

            ShooterPlayModeProfileInspectorGui.DrawTemplatePicker(
                serializedObject,
                Required(_syncTemplateId),
                playerCountProperty: null,
                _randomSeed,
                _controlledPlayerId,
                _worldScale);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Replay Runtime", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(Required(_controlledPlayerId));
                EditorGUILayout.PropertyField(Required(_randomSeed));
                EditorGUILayout.Slider(Required(_worldScale), 0.25f, 8f, new GUIContent("World Scale"));
                EditorGUILayout.Slider(Required(_timeScale), 0f, 4f, new GUIContent("Time Scale"));

                Required(_controlledPlayerId).intValue = Mathf.Max(1, Required(_controlledPlayerId).intValue);
                Required(_worldScale).floatValue = Mathf.Max(0.001f, Required(_worldScale).floatValue);
                Required(_timeScale).floatValue = Mathf.Max(0f, Required(_timeScale).floatValue);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static SerializedProperty Required(SerializedProperty? property)
        {
            return property ?? throw new MissingReferenceException("Shooter replay profile inspector binding is missing.");
        }
    }
}

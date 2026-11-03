#nullable enable

using AbilityKit.Demo.Shooter.View.PlayMode;
using UnityEditor;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.View.Editor
{
    [CustomEditor(typeof(ShooterRemoteStateSyncPlayModeProfile))]
    public sealed class ShooterRemoteStateSyncPlayModeProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty? _launchMode;
        private SerializedProperty? _host;
        private SerializedProperty? _port;
        private SerializedProperty? _sessionToken;
        private SerializedProperty? _region;
        private SerializedProperty? _serverId;
        private SerializedProperty? _roomId;
        private SerializedProperty? _timeoutSeconds;
        private SerializedProperty? _syncTemplateId;
        private SerializedProperty? _randomSeed;
        private SerializedProperty? _playerCount;
        private SerializedProperty? _controlledPlayerId;
        private SerializedProperty? _worldScale;

        private void OnEnable()
        {
            _launchMode = serializedObject.FindProperty("launchMode");
            _host = serializedObject.FindProperty("host");
            _port = serializedObject.FindProperty("port");
            _sessionToken = serializedObject.FindProperty("sessionToken");
            _region = serializedObject.FindProperty("region");
            _serverId = serializedObject.FindProperty("serverId");
            _roomId = serializedObject.FindProperty("roomId");
            _timeoutSeconds = serializedObject.FindProperty("timeoutSeconds");
            _syncTemplateId = serializedObject.FindProperty("syncTemplateId");
            _randomSeed = serializedObject.FindProperty("randomSeed");
            _playerCount = serializedObject.FindProperty("playerCount");
            _controlledPlayerId = serializedObject.FindProperty("controlledPlayerId");
            _worldScale = serializedObject.FindProperty("worldScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ShooterPlayModeProfileInspectorGui.DrawTemplatePicker(
                serializedObject,
                Required(_syncTemplateId),
                Required(_playerCount),
                _randomSeed,
                _controlledPlayerId,
                _worldScale);

            ShooterPlayModeProfileInspectorGui.DrawRuntimeTuning(
                Required(_playerCount),
                Required(_randomSeed),
                Required(_controlledPlayerId),
                Required(_worldScale));

            ShooterPlayModeProfileInspectorGui.DrawNetworkEndpoint(
                Required(_launchMode),
                Required(_host),
                Required(_port),
                Required(_sessionToken),
                Required(_region),
                Required(_serverId),
                Required(_roomId),
                Required(_timeoutSeconds));

            serializedObject.ApplyModifiedProperties();
        }

        private static SerializedProperty Required(SerializedProperty? property)
        {
            return property ?? throw new MissingReferenceException("Shooter remote profile inspector binding is missing.");
        }
    }
}

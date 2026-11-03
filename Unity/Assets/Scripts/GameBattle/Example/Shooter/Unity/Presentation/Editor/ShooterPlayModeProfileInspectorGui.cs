#nullable enable

using System;
using AbilityKit.Demo.Shooter.View.PlayMode;
using AbilityKit.Network.Runtime;
using UnityEditor;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.View.Editor
{
    internal static class ShooterPlayModeProfileInspectorGui
    {
        public static void DrawTemplatePicker(
            SerializedObject serializedObject,
            SerializedProperty syncTemplateIdProperty,
            SerializedProperty? playerCountProperty,
            SerializedProperty? randomSeedProperty,
            SerializedProperty? controlledPlayerIdProperty,
            SerializedProperty? worldScaleProperty)
        {
            var templates = ShooterAcceptanceCatalog.SyncTemplates;
            var templateIndex = FindTemplateIndex(syncTemplateIdProperty.stringValue);
            var names = new GUIContent[templates.Count];
            for (var i = 0; i < templates.Count; i++)
            {
                var template = templates[i];
                var statusSuffix = template.IsRunnable ? string.Empty : " (Reserved)";
                names[i] = new GUIContent($"{template.DisplayName}{statusSuffix}", template.Description);
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Sync Scheme", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                var selectedIndex = EditorGUILayout.Popup(new GUIContent("Template"), templateIndex, names);
                if (EditorGUI.EndChangeCheck() && selectedIndex >= 0 && selectedIndex < templates.Count)
                {
                    ApplyTemplate(serializedObject, templates[selectedIndex], syncTemplateIdProperty, playerCountProperty, randomSeedProperty, controlledPlayerIdProperty, worldScaleProperty);
                }

                var selectedTemplate = templates[Mathf.Clamp(selectedIndex, 0, templates.Count - 1)];
                EditorGUILayout.LabelField("Model", selectedTemplate.SyncModel.ToString());
                EditorGUILayout.LabelField("Network", selectedTemplate.NetworkEnvironmentId);
                EditorGUILayout.LabelField("Carrier", selectedTemplate.ExpectedCarrierName);
                EditorGUILayout.LabelField("Send Policy", selectedTemplate.SendPolicy.ToString());
                EditorGUILayout.LabelField("Convergence", selectedTemplate.ConvergenceKind.ToString());
                EditorGUILayout.HelpBox(selectedTemplate.Description, selectedTemplate.IsRunnable ? MessageType.Info : MessageType.Warning);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Predict Rollback"))
                    {
                        ApplyTemplate(serializedObject, ShooterSyncTemplateIds.PredictRollbackAuthority, syncTemplateIdProperty, playerCountProperty, randomSeedProperty, controlledPlayerIdProperty, worldScaleProperty);
                    }

                    if (GUILayout.Button("Interpolation"))
                    {
                        ApplyTemplate(serializedObject, ShooterSyncTemplateIds.AuthoritativeInterpolationPresentation, syncTemplateIdProperty, playerCountProperty, randomSeedProperty, controlledPlayerIdProperty, worldScaleProperty);
                    }

                    if (GUILayout.Button("Hybrid"))
                    {
                        ApplyTemplate(serializedObject, ShooterSyncTemplateIds.HybridHeroPrediction, syncTemplateIdProperty, playerCountProperty, randomSeedProperty, controlledPlayerIdProperty, worldScaleProperty);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Batch State"))
                    {
                        ApplyTemplate(serializedObject, ShooterSyncTemplateIds.BatchStateLowFrequency, syncTemplateIdProperty, playerCountProperty, randomSeedProperty, controlledPlayerIdProperty, worldScaleProperty);
                    }

                    if (GUILayout.Button("Mass LOD"))
                    {
                        ApplyTemplate(serializedObject, ShooterSyncTemplateIds.MassBattleLodAoi, syncTemplateIdProperty, playerCountProperty, randomSeedProperty, controlledPlayerIdProperty, worldScaleProperty);
                    }
                }
            }
        }

        public static void DrawRuntimeTuning(
            SerializedProperty playerCountProperty,
            SerializedProperty randomSeedProperty,
            SerializedProperty controlledPlayerIdProperty,
            SerializedProperty worldScaleProperty)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Runtime Tuning", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(playerCountProperty, new GUIContent("Players"));
                EditorGUILayout.PropertyField(controlledPlayerIdProperty, new GUIContent("Controlled Player"));
                EditorGUILayout.PropertyField(randomSeedProperty, new GUIContent("Random Seed"));
                EditorGUILayout.Slider(worldScaleProperty, 0.25f, 8f, new GUIContent("World Scale"));

                controlledPlayerIdProperty.intValue = Mathf.Clamp(controlledPlayerIdProperty.intValue, 1, Math.Max(1, playerCountProperty.intValue));
                playerCountProperty.intValue = Math.Max(1, playerCountProperty.intValue);
                worldScaleProperty.floatValue = Mathf.Max(0.001f, worldScaleProperty.floatValue);
            }
        }

        public static void DrawNetworkEndpoint(
            SerializedProperty launchModeProperty,
            SerializedProperty hostProperty,
            SerializedProperty portProperty,
            SerializedProperty sessionTokenProperty,
            SerializedProperty regionProperty,
            SerializedProperty serverIdProperty,
            SerializedProperty roomIdProperty,
            SerializedProperty timeoutSecondsProperty)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Gateway", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(launchModeProperty);
                EditorGUILayout.PropertyField(hostProperty);
                EditorGUILayout.PropertyField(portProperty);
                EditorGUILayout.PropertyField(sessionTokenProperty);
                EditorGUILayout.PropertyField(regionProperty);
                EditorGUILayout.PropertyField(serverIdProperty);
                EditorGUILayout.PropertyField(roomIdProperty);
                EditorGUILayout.Slider(timeoutSecondsProperty, 1f, 60f, new GUIContent("Timeout Seconds"));
                portProperty.intValue = Math.Max(1, portProperty.intValue);
            }
        }

        public static void DrawStatus(params SerializedProperty[] properties)
        {
            using (new EditorGUI.DisabledScope(true))
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
                foreach (var property in properties)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
        }

        public static void DrawCatalogControls(SerializedProperty profileProperty, SerializedProperty profileCatalogProperty)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Profile Source", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(profileCatalogProperty);
                EditorGUILayout.PropertyField(profileProperty);
            }
        }

        private static int FindTemplateIndex(string templateId)
        {
            var templates = ShooterAcceptanceCatalog.SyncTemplates;
            for (var i = 0; i < templates.Count; i++)
            {
                if (string.Equals(templates[i].Id, templateId, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return 0;
        }

        private static void ApplyTemplate(
            SerializedObject serializedObject,
            string templateId,
            SerializedProperty syncTemplateIdProperty,
            SerializedProperty? playerCountProperty,
            SerializedProperty? randomSeedProperty,
            SerializedProperty? controlledPlayerIdProperty,
            SerializedProperty? worldScaleProperty)
        {
            ApplyTemplate(serializedObject, ShooterAcceptanceCatalog.GetSyncTemplate(templateId), syncTemplateIdProperty, playerCountProperty, randomSeedProperty, controlledPlayerIdProperty, worldScaleProperty);
        }

        private static void ApplyTemplate(
            SerializedObject serializedObject,
            in ShooterSyncTemplate template,
            SerializedProperty syncTemplateIdProperty,
            SerializedProperty? playerCountProperty,
            SerializedProperty? randomSeedProperty,
            SerializedProperty? controlledPlayerIdProperty,
            SerializedProperty? worldScaleProperty)
        {
            syncTemplateIdProperty.stringValue = template.Id;
            if (playerCountProperty != null)
            {
                playerCountProperty.intValue = Math.Max(playerCountProperty.intValue, template.RecommendedPlayerCount);
            }

            if (controlledPlayerIdProperty != null)
            {
                var maxControlledPlayerId = playerCountProperty != null ? playerCountProperty.intValue : controlledPlayerIdProperty.intValue;
                controlledPlayerIdProperty.intValue = Mathf.Clamp(controlledPlayerIdProperty.intValue, 1, Math.Max(1, maxControlledPlayerId));
            }

            if (randomSeedProperty != null && randomSeedProperty.intValue == 0)
            {
                randomSeedProperty.intValue = 3901;
            }

            if (worldScaleProperty != null)
            {
                worldScaleProperty.floatValue = Mathf.Max(0.001f, worldScaleProperty.floatValue);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

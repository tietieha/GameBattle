#nullable enable

using System;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.View.PlayMode
{
    [CreateAssetMenu(
        fileName = "ShooterRemoteStateSyncPlayModeProfileCatalog",
        menuName = "AbilityKit/Shooter/Remote State Sync Play Mode Profile Catalog")]
    public sealed class ShooterRemoteStateSyncPlayModeProfileCatalog : ScriptableObject
    {
        [SerializeField] private ShooterRemoteStateSyncPlayModeProfile? fallbackProfile;
        [SerializeField] private ShooterRemoteStateSyncPlayModeProfile[] profiles = Array.Empty<ShooterRemoteStateSyncPlayModeProfile>();
        [SerializeField] private int selectedIndex;

        public ShooterRemoteStateSyncPlayModeProfile? FallbackProfile => fallbackProfile;
        public int ProfileCount => profiles?.Length ?? 0;
        public int SelectedIndex => ClampIndex(selectedIndex);
        public string SelectedProfileName => ResolveProfile()?.name ?? string.Empty;

        public ShooterRemoteStateSyncPlayModeProfile? ResolveProfile()
        {
            if (profiles == null || profiles.Length <= 0)
            {
                return fallbackProfile;
            }

            selectedIndex = ClampIndex(selectedIndex);
            return profiles[selectedIndex] != null ? profiles[selectedIndex] : fallbackProfile;
        }

        public void SelectNext()
        {
            SelectOffset(1);
        }

        public void SelectPrevious()
        {
            SelectOffset(-1);
        }

        public void SelectOffset(int offset)
        {
            if (profiles == null || profiles.Length <= 0)
            {
                selectedIndex = 0;
                return;
            }

            selectedIndex = (selectedIndex + offset) % profiles.Length;
            if (selectedIndex < 0)
            {
                selectedIndex += profiles.Length;
            }
        }

        private int ClampIndex(int value)
        {
            if (profiles == null || profiles.Length <= 0)
            {
                return 0;
            }

            return Mathf.Clamp(value, 0, profiles.Length - 1);
        }
    }
}

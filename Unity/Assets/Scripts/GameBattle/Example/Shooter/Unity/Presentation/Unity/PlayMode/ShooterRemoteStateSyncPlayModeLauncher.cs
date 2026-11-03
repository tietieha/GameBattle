#nullable enable

using System;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.View.PlayMode
{
    /// <summary>
    /// Scene component that starts the reusable Shooter remote state-sync host from Unity Play Mode.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShooterRemoteStateSyncPlayModeLauncher : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private ShooterRemoteStateSyncPlayModeProfile? profile;
        [SerializeField] private ShooterRemoteStateSyncPlayModeProfileCatalog? profileCatalog;

        [Header("Runtime Overrides")]
        [SerializeField] private bool overrideLaunchMode;
        [SerializeField] private ShooterRemoteStateSyncLaunchMode launchModeOverride = ShooterRemoteStateSyncLaunchMode.RestoreFirst;
        [SerializeField] private string sessionTokenOverride = string.Empty;
        [SerializeField] private string roomIdOverride = string.Empty;

        [Header("Lifecycle")]
        [SerializeField] private bool startOnEnable = true;
        [SerializeField] private bool stopOnDisable = true;

        [Header("Status")]
        [SerializeField] private bool isRunning;
        [SerializeField] private bool isStarting;
        [SerializeField] private string selectedProfileName = string.Empty;
        [SerializeField] private string currentRoomId = string.Empty;
        [SerializeField] private string lastError = string.Empty;

        private async void OnEnable()
        {
            if (!startOnEnable)
            {
                RefreshStatus();
                return;
            }

            await StartRemoteAsync();
        }

        private void OnDisable()
        {
            if (stopOnDisable)
            {
                ShooterRemoteStateSyncPlayModeHost.Stop();
            }

            RefreshStatus();
        }

        [ContextMenu("Start Remote Shooter")]
        public async void StartRemote()
        {
            await StartRemoteAsync();
        }

        [ContextMenu("Stop Remote Shooter")]
        public void StopRemote()
        {
            ShooterRemoteStateSyncPlayModeHost.Stop();
            RefreshStatus();
        }

        [ContextMenu("Use Previous Profile Template")]
        public void UsePreviousProfileTemplate()
        {
            profileCatalog?.SelectPrevious();
            RefreshStatus();
        }

        [ContextMenu("Use Next Profile Template")]
        public void UseNextProfileTemplate()
        {
            profileCatalog?.SelectNext();
            RefreshStatus();
        }

        [ContextMenu("Rebuild Shooter Views")]
        public void RebuildViews()
        {
            ShooterRemoteStateSyncPlayModeHost.RebuildViews();
            RefreshStatus();
        }

        [ContextMenu("Copy Current Room Id")]
        public void CopyCurrentRoomId()
        {
            RefreshStatus();
            if (string.IsNullOrWhiteSpace(currentRoomId))
            {
                return;
            }

            GUIUtility.systemCopyBuffer = currentRoomId;
        }

        private async System.Threading.Tasks.Task StartRemoteAsync()
        {
            RefreshStatus();

            try
            {
                var launchOptions = BuildLaunchOptions();
                var launch = await ShooterRemoteStateSyncPlayModeHost.StartAsync(launchOptions);
                currentRoomId = launch.Flow.RoomId;
                lastError = string.Empty;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                Debug.LogException(ex, this);
            }
            finally
            {
                RefreshStatus();
            }
        }

        private ShooterRemoteStateSyncLaunchOptions BuildLaunchOptions()
        {
            var selectedProfile = ResolveProfile();
            if (selectedProfile == null)
            {
                throw new InvalidOperationException("Shooter remote state-sync profile is not assigned.");
            }

            return selectedProfile.BuildLaunchOptions(
                string.IsNullOrWhiteSpace(sessionTokenOverride) ? null : sessionTokenOverride,
                string.IsNullOrWhiteSpace(roomIdOverride) ? null : roomIdOverride,
                overrideLaunchMode ? launchModeOverride : null);
        }

        private ShooterRemoteStateSyncPlayModeProfile? ResolveProfile()
        {
            return profileCatalog != null ? profileCatalog.ResolveProfile() : profile;
        }

        private void RefreshStatus()
        {
            isRunning = ShooterRemoteStateSyncPlayModeHost.IsRunning;
            isStarting = ShooterRemoteStateSyncPlayModeHost.IsStarting;
            selectedProfileName = ResolveProfile()?.name ?? string.Empty;

            var flow = ShooterRemoteStateSyncPlayModeHost.Flow;
            if (flow.HasValue)
            {
                currentRoomId = flow.Value.RoomId;
            }

            var error = ShooterRemoteStateSyncPlayModeHost.LastError;
            if (error != null)
            {
                lastError = error.Message;
            }
        }
    }
}

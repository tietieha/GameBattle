#nullable enable

using System;
using AbilityKit.Core.Recording.FrameRecord;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.View.PlayMode
{
    /// <summary>
    /// Scene component that replays a Shooter frame-record file through the reusable Play Mode view pipeline.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShooterFrameRecordReplayPlayModeLauncher : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private ShooterFrameRecordReplayPlayModeProfile? profile;
        [SerializeField] private ShooterFrameRecordReplayPlayModeProfileCatalog? profileCatalog;

        [Header("Runtime Overrides")]
        [SerializeField] private string replayPathOverride = string.Empty;

        [Header("Playback")]
        [SerializeField] private bool startOnEnable = true;
        [SerializeField] private bool stopOnDisable = true;

        [Header("Rendering")]
        [SerializeField] private ShooterUnityViewRenderBackend renderBackend = ShooterUnityViewRenderBackendCatalog.DefaultBackend;

        [Header("Status")]
        [SerializeField] private bool isRunning;
        [SerializeField] private long stepCount;
        [SerializeField] private long renderCount;
        [SerializeField] private int replayFrameCursor;
        [SerializeField] private int replayInputFrameCount;
        [SerializeField] private string selectedProfileName = string.Empty;
        [SerializeField] private string resolvedReplayPath = string.Empty;
        [SerializeField] private string lastError = string.Empty;
        [SerializeField] private string effectiveRenderBackend = string.Empty;

        private ShooterFrameRecordInputSource? _inputSource;
        private UnityShooterSwitchableViewSink? _viewSink;
        private ShooterPlaySessionRunner? _runner;

        private void OnEnable()
        {
            if (startOnEnable)
            {
                StartReplay();
            }
            else
            {
                RefreshStatus();
            }
        }

        private void OnDisable()
        {
            if (stopOnDisable)
            {
                StopReplay();
            }
            else
            {
                RefreshStatus();
            }
        }

        private void Update()
        {
            var selectedProfile = ResolveProfile();
            if (_runner == null || !_runner.IsRunning)
            {
                RefreshStatus();
                return;
            }

            try
            {
                _runner.Tick(Time.deltaTime * (selectedProfile?.TimeScale ?? 1f));
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                Debug.LogException(ex, this);
                StopReplay();
            }

            RefreshStatus();
        }

        [ContextMenu("Start Shooter Replay")]
        public void StartReplay()
        {
            StopReplay();

            try
            {
                var selectedProfile = ResolveProfile();
                if (selectedProfile == null)
                {
                    throw new InvalidOperationException("Shooter frame-record replay profile is not assigned.");
                }

                var replayPath = string.IsNullOrWhiteSpace(replayPathOverride) ? selectedProfile.ReplayPath : replayPathOverride;
                resolvedReplayPath = ResolveReplayPath(replayPath);
                var record = FrameRecordCodecs.Current.Load(resolvedReplayPath);
                _inputSource = new ShooterFrameRecordInputSource(record, selectedProfile.ControlledPlayerId);
                _viewSink = new UnityShooterSwitchableViewSink(ShooterUnityViewRenderBackendCatalog.Normalize(renderBackend));
                _runner = new ShooterPlaySessionRunner(_inputSource, _viewSink);

                _runner.Start(selectedProfile.BuildSessionOptions());
                lastError = string.Empty;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                Debug.LogException(ex, this);
                StopReplay();
            }

            RefreshStatus();
        }

        [ContextMenu("Stop Shooter Replay")]
        public void StopReplay()
        {
            if (_runner != null)
            {
                _runner.Dispose();
                _runner = null;
            }
            else
            {
                _viewSink?.Clear();
            }

            _inputSource = null;
            _viewSink = null;
            RefreshStatus();
        }

        [ContextMenu("Use Previous Replay Template")]
        public void UsePreviousReplayTemplate()
        {
            profileCatalog?.SelectPrevious();
            RefreshStatus();
        }

        [ContextMenu("Use Next Replay Template")]
        public void UseNextReplayTemplate()
        {
            profileCatalog?.SelectNext();
            RefreshStatus();
        }

        [ContextMenu("Rebuild Shooter Replay Views")]
        public void RebuildViews()
        {
            _viewSink?.RebuildAll();
            RefreshStatus();
        }

        private ShooterFrameRecordReplayPlayModeProfile? ResolveProfile()
        {
            return profileCatalog != null ? profileCatalog.ResolveProfile() : profile;
        }

        private void RefreshStatus()
        {
            isRunning = _runner?.IsRunning == true;
            stepCount = _runner?.StepCount ?? 0L;
            renderCount = _runner?.RenderCount ?? 0L;
            replayFrameCursor = _inputSource?.FrameCursor ?? 0;
            replayInputFrameCount = _inputSource?.InputFrameCount ?? 0;
            selectedProfileName = ResolveProfile()?.name ?? string.Empty;
            effectiveRenderBackend = ShooterUnityViewRenderBackendCatalog.Get(_viewSink?.Backend ?? ShooterUnityViewRenderBackendCatalog.Normalize(renderBackend)).DisplayName;
        }

        private static string ResolveReplayPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("Replay path is empty.");
            }

            if (System.IO.Path.IsPathRooted(path))
            {
                return path;
            }

            var assetsRelativePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, path));
            if (System.IO.File.Exists(assetsRelativePath))
            {
                return assetsRelativePath;
            }

            var projectRelativePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", path));
            if (System.IO.File.Exists(projectRelativePath))
            {
                return projectRelativePath;
            }

            return assetsRelativePath;
        }
    }
}

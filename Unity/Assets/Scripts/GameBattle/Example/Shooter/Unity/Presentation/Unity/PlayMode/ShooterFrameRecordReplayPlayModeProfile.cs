#nullable enable

using System;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.View.PlayMode
{
    [CreateAssetMenu(
        fileName = "ShooterFrameRecordReplayPlayModeProfile",
        menuName = "AbilityKit/Shooter/Frame Record Replay Play Mode Profile")]
    public sealed class ShooterFrameRecordReplayPlayModeProfile : ScriptableObject
    {
        [Header("Replay")]
        [SerializeField] private string replayPath = "../../artifacts/shooter_multiprocess_smoke/records/input-state-create.record.bin";
        [SerializeField] private string syncTemplateId = ShooterSyncTemplateIds.PredictRollbackAuthority;
        [SerializeField] private int controlledPlayerId = 1;
        [SerializeField] private int randomSeed = 3901;
        [SerializeField] private float worldScale = 1f;

        [Header("Playback")]
        [SerializeField] private float timeScale = 1f;

        public string ReplayPath => string.IsNullOrWhiteSpace(replayPath)
            ? "../../artifacts/shooter_multiprocess_smoke/records/input-state-create.record.bin"
            : replayPath;

        public string SyncTemplateId => string.IsNullOrWhiteSpace(syncTemplateId) ? ShooterSyncTemplateIds.PredictRollbackAuthority : syncTemplateId;
        public int ControlledPlayerId => Math.Max(1, controlledPlayerId);
        public int RandomSeed => randomSeed;
        public float WorldScale => Mathf.Max(0.001f, worldScale);
        public float TimeScale => Mathf.Max(0f, timeScale);

        public ShooterPlayModeSessionOptions BuildSessionOptions()
        {
            return ShooterPlayModeSessionOptions.FromTemplate(
                ShooterAcceptanceCatalog.GetSyncTemplate(SyncTemplateId),
                RandomSeed,
                ControlledPlayerId,
                WorldScale);
        }
    }
}

using System;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.FrameSync.Rollback;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public sealed class ShooterPackedSnapshotRollbackProvider : IRollbackStateProvider
    {
        public const int DefaultKey = 21001;

        private readonly IShooterBattleRuntimePort _runtime;
        private readonly ulong _worldId;

        public ShooterPackedSnapshotRollbackProvider(IShooterBattleRuntimePort runtime, ulong worldId, int key = DefaultKey)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _worldId = worldId;
            Key = key;
        }

        public int Key { get; }

        public byte[] Export(FrameIndex frame)
        {
            return _runtime.ExportPackedSnapshotBytes(_worldId, isFullSnapshot: true, authorityOverride: false);
        }

        public void Import(FrameIndex frame, byte[] payload)
        {
            if (payload == null || payload.Length == 0)
            {
                return;
            }

            _runtime.ImportPackedSnapshotBytes(payload);
        }
    }
}

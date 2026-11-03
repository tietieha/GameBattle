namespace AbilityKit.Game.Flow
{
    internal readonly struct BattleViewProjectileVfxSyncPlan
    {
        public BattleViewProjectileVfxSyncPlan(int desiredVfxId)
        {
            DesiredVfxId = desiredVfxId;
        }

        public int DesiredVfxId { get; }

        public bool HasVfx => DesiredVfxId > 0;

        public bool IsSatisfiedBy(BattleViewHandle handle)
        {
            if (handle == null) return false;
            if (!HasVfx) return handle.VfxEntityId.Index == 0;
            return handle.VfxEntityId.Index != 0 && handle.VfxId == DesiredVfxId;
        }
    }
}

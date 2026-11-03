using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public readonly struct BattleProjectileVfxIds
    {
        public BattleProjectileVfxIds(int onSpawnVfxId, int onHitVfxId, int onExpireVfxId)
            : this(0, onSpawnVfxId, onHitVfxId, onExpireVfxId)
        {
        }

        public BattleProjectileVfxIds(int projectileVfxId, int onSpawnVfxId, int onHitVfxId, int onExpireVfxId)
        {
            ProjectileVfxId = projectileVfxId;
            OnSpawnVfxId = onSpawnVfxId;
            OnHitVfxId = onHitVfxId;
            OnExpireVfxId = onExpireVfxId;
        }

        public int ProjectileVfxId { get; }
        public int OnSpawnVfxId { get; }
        public int OnHitVfxId { get; }
        public int OnExpireVfxId { get; }
    }

    public readonly struct BattleProjectileTriggerHitInput
    {
        public BattleProjectileTriggerHitInput(int templateId, in MobaFloat3 hitPoint)
        {
            TemplateId = templateId;
            HitPoint = hitPoint;
        }

        public int TemplateId { get; }
        public MobaFloat3 HitPoint { get; }
    }

    public readonly struct BattleProjectileTriggerHitVfxSpec
    {
        public BattleProjectileTriggerHitVfxSpec(int vfxId, in MobaFloat3 position)
        {
            VfxId = vfxId;
            Position = position;
        }

        public int VfxId { get; }
        public MobaFloat3 Position { get; }
        public bool IsValid => VfxId > 0;
    }

    public sealed class BattleProjectileVfxResolver
    {
        public bool TryResolveTriggerHit(
            in BattleProjectileTriggerHitInput input,
            in BattleProjectileVfxIds vfxIds,
            out BattleProjectileTriggerHitVfxSpec spec)
        {
            spec = default;

            if (input.TemplateId <= 0) return false;
            if (vfxIds.OnHitVfxId <= 0) return false;

            spec = new BattleProjectileTriggerHitVfxSpec(vfxIds.OnHitVfxId, input.HitPoint);
            return true;
        }

        public int ResolveSnapshotVfxId(in BattleProjectileVfxIds vfxIds, int kind)
        {
            if (kind == BattleProjectileEventKinds.Spawn)
            {
                return vfxIds.OnSpawnVfxId > 0 ? vfxIds.OnSpawnVfxId : vfxIds.ProjectileVfxId;
            }

            if (kind == BattleProjectileEventKinds.Hit) return vfxIds.OnHitVfxId;
            if (kind == BattleProjectileEventKinds.Exit) return vfxIds.OnExpireVfxId;
            return 0;
        }

        public MobaFloat3 ResolveSnapshotPosition(float x, float y, float z)
        {
            return new MobaFloat3(x, y, z);
        }
    }

    public static class BattleProjectileEventKinds
    {
        public const int Spawn = 1;
        public const int Hit = 2;
        public const int Exit = 3;
    }
}

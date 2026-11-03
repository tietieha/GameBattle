using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public readonly struct BattleProjectileVfxSpawnSpec
    {
        public BattleProjectileVfxSpawnSpec(int vfxId, in MobaFloat3 position, int followTargetEntityId)
        {
            VfxId = vfxId;
            Position = position;
            FollowTargetEntityId = followTargetEntityId;
        }

        public int VfxId { get; }
        public MobaFloat3 Position { get; }
        public int FollowTargetEntityId { get; }

        public bool IsEmpty => VfxId <= 0;
    }
}

using AbilityKit.Ability.Host;

namespace AbilityKit.Demo.Moba.Services.EntityConstruction
{
    public static class ActorEntityMetaApplier
    {
        public static void Apply(ActorEntity entity, in MobaEntityInfo info)
        {
            if (entity == null) return;

            if (entity.hasTeam) entity.ReplaceTeam(info.Team);
            else entity.AddTeam(info.Team);

            if (entity.hasEntityMainType) entity.ReplaceEntityMainType(info.MainType);
            else entity.AddEntityMainType(info.MainType);

            if (entity.hasUnitSubType) entity.ReplaceUnitSubType(info.UnitSubType);
            else entity.AddUnitSubType(info.UnitSubType);

            if (entity.hasOwnerPlayerId) entity.ReplaceOwnerPlayerId(info.OwnerPlayer);
            else entity.AddOwnerPlayerId(info.OwnerPlayer);
        }
    }
}

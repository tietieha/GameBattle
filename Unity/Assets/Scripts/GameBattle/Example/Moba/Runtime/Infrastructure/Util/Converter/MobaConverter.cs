using AbilityKit.Ability.Host;
using AbilityKit.Protocol.Moba;
using AbilityKit.Core.Mathematics;
using AbilityKit.Demo.Moba;
using AbilityKit.Demo.Moba.Config.BattleDemo.MO;
using AbilityKit.Demo.Moba.Services.EntityConstruction;

namespace AbilityKit.Demo.Moba.Util.Converter
{
    // 逻辑层通用数据转换工具，用于将外部输入转换为逻辑层使用的数据结构。
    public static class MobaConverter
    {
        // 将 System.Numerics.Vector3 转为逻辑 Vec3。
        public static Vec3 ToVec3(in System.Numerics.Vector3 v)
        {
            return new Vec3(v.X, v.Y, v.Z);
        }

        // 将 System.Numerics.Quaternion 转为逻辑 Quat。
        public static Quat ToQuat(in System.Numerics.Quaternion q)
        {
            return new Quat(q.X, q.Y, q.Z, q.W);
        }

        // 从欧拉角（弧度）创建逻辑 Quat，按 Yaw-Pitch-Roll 顺序组合：Y * X * Z。
        public static Quat ToQuatFromEulerRad(float pitchRad, float yawRad, float rollRad)
        {
            var qx = Quat.FromAxisAngle(Vec3.Right, pitchRad);
            var qy = Quat.FromAxisAngle(Vec3.Up, yawRad);
            var qz = Quat.FromAxisAngle(Vec3.Forward, rollRad);
            return (qy * qx * qz).Normalized;
        }

        // 从朝向角（Yaw，弧度）创建逻辑 Quat，常用于 MOBA 的平面转向。
        public static Quat ToYawRotationRad(float yawRad)
        {
            return Quat.FromAxisAngle(Vec3.Up, yawRad).Normalized;
        }

        // 将平面坐标（x,z）转换为 3D 坐标（y 默认 0）。
        public static Vec3 ToXZ(float x, float z, float y = 0f)
        {
            return new Vec3(x, y, z);
        }

        // 创建逻辑 Transform3。
        public static Transform3 ToTransform(in Vec3 position, in Quat rotation, in Vec3 scale)
        {
            return new Transform3(position, rotation, scale);
        }

        // 创建逻辑 Transform3（常用：单位缩放）。
        public static Transform3 ToTransform(in Vec3 position, in Quat rotation)
        {
            return new Transform3(position, rotation, Vec3.One);
        }

        // 创建逻辑 Transform3（常用：Yaw 转向 + 单位缩放）。
        public static Transform3 ToTransformYaw(in Vec3 position, float yawRad)
        {
            return new Transform3(position, ToYawRotationRad(yawRad), Vec3.One);
        }

        public static MobaActorBuildSpec ToActorBuildSpec(int actorId, in MobaPlayerLoadout loadout)
        {
            var spawnPos = new Vec3(loadout.SpawnX, loadout.SpawnY, loadout.SpawnZ);
            var transform = new Transform3(spawnPos, Quat.Identity, Vec3.One);
            var mainType = (EntityMainType)loadout.MainType;
            var unitSubType = (UnitSubType)loadout.UnitSubType;

            return new MobaActorBuildSpec(
                new MobaEntityInfo(
                    actorId: actorId,
                    kind: ActorArchetypeFactory.CreateKindFromType(mainType, unitSubType),
                    transform: transform,
                    team: (Team)loadout.TeamId,
                    mainType: mainType,
                    unitSubType: unitSubType,
                    ownerPlayer: loadout.PlayerId,
                    templateId: loadout.AttributeTemplateId),
                sourceKind: MobaActorBuildSourceKind.PlayerLoadout,
                sourceId: loadout.HeroId,
                ownerActorId: 0);
        }

        public static MobaActorBuildSpec ToSummonActorBuildSpec(
            int actorId,
            int summonId,
            SummonMO summon,
            global::ActorEntity caster,
            in Vec3 position,
            bool hasForward,
            in Vec3 forward)
        {
            var spawnPos = position.SqrMagnitude > 0f ? position : caster.transform.Value.Position;
            var rotation = caster.transform.Value.Rotation;
            if (hasForward)
            {
                var f = new Vec3(forward.X, 0f, forward.Z);
                if (f.SqrMagnitude > 0.0001f)
                {
                    rotation = Quat.LookRotation(f, Vec3.Up);
                }
            }

            var unitSubType = (UnitSubType)summon.UnitSubType;
            var team = caster.hasTeam ? caster.team.Value : Team.None;
            var ownerPlayer = caster.hasOwnerPlayerId ? caster.ownerPlayerId.Value : default(PlayerId);

            return new MobaActorBuildSpec(
                new MobaEntityInfo(
                    actorId: actorId,
                    kind: MobaEntityKind.Summon,
                    transform: new Transform3(spawnPos, rotation, Vec3.One),
                    team: team,
                    mainType: EntityMainType.Unit,
                    unitSubType: unitSubType,
                    ownerPlayer: ownerPlayer,
                    templateId: summon.AttributeTemplateId),
                sourceKind: MobaActorBuildSourceKind.Summon,
                sourceId: summonId,
                ownerActorId: caster.hasActorId ? caster.actorId.Value : 0);
        }

        public static MobaActorBuildSpec ToProjectileActorBuildSpec(
            int actorId,
            int projectileCode,
            global::ActorEntity caster,
            in Vec3 spawnPos,
            in Vec3 direction)
        {
            return ToProjectileSpec(actorId, projectileCode, caster, in spawnPos, in direction, MobaActorBuildSourceKind.Projectile, MobaEntityKind.Projectile);
        }

        public static MobaActorBuildSpec ToProjectileLauncherActorBuildSpec(
            int actorId,
            int launcherId,
            global::ActorEntity caster,
            in Vec3 spawnPos,
            in Vec3 direction)
        {
            return ToProjectileSpec(actorId, launcherId, caster, in spawnPos, in direction, MobaActorBuildSourceKind.ProjectileLauncher, MobaEntityKind.ProjectileLauncher);
        }

        private static MobaActorBuildSpec ToProjectileSpec(
            int actorId,
            int templateId,
            global::ActorEntity caster,
            in Vec3 spawnPos,
            in Vec3 direction,
            MobaActorBuildSourceKind sourceKind,
            MobaEntityKind entityKind)
        {
            var d = direction.SqrMagnitude > 0f ? direction.Normalized : Vec3.Forward;
            var rotation = Quat.LookRotation(d, Vec3.Up);
            var team = caster.hasTeam ? caster.team.Value : Team.None;
            var ownerPlayer = caster.hasOwnerPlayerId ? caster.ownerPlayerId.Value : default(PlayerId);

            return new MobaActorBuildSpec(
                new MobaEntityInfo(
                    actorId: actorId,
                    kind: entityKind,
                    transform: new Transform3(spawnPos, rotation, Vec3.One),
                    team: team,
                    mainType: EntityMainType.Projectile,
                    unitSubType: UnitSubType.Bullet,
                    ownerPlayer: ownerPlayer,
                    templateId: templateId),
                sourceKind: sourceKind,
                sourceId: templateId,
                ownerActorId: caster.hasActorId ? caster.actorId.Value : 0);
        }
    }
}


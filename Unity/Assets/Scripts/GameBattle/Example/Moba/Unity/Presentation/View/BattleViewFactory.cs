using AbilityKit.Demo.Moba.Config.BattleDemo.MO;
using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Game.Battle.Entity;
using AbilityKit.Game.Battle.Vfx;
using UnityEngine;

namespace AbilityKit.Game.Flow
{
    public static class BattleViewFactory
    {
        private static BattleViewResourceProvider DefaultResources => BattleViewResourceProvider.Default;

        public static MobaConfigDatabase Configs
        {
            get => DefaultResources.Configs;
            set => DefaultResources.Configs = value;
        }

        public static VfxDatabase VfxDb
        {
            get => DefaultResources.VfxDb;
            set => DefaultResources.VfxDb = value;
        }

        public static MobaConfigDatabase GetOrLoadConfigs()
        {
            return DefaultResources.GetOrLoadConfigs();
        }

        public static GameObject CreateShellGameObject(int actorId, int modelId)
        {
            return DefaultResources.CreateShellGameObject(actorId, modelId);
        }

        public static GameObject CreateModelGo(int modelId)
        {
            return DefaultResources.CreateModelGo(modelId);
        }

        public static GameObject CreateVfxGo(int vfxId)
        {
            return DefaultResources.CreateVfxGo(vfxId);
        }

        public static int ResolveModelId(BattleEntityMetaComponent meta)
        {
            return DefaultResources.ResolveModelId(meta);
        }

        public static int ResolveProjectileVfxId(BattleEntityMetaComponent meta)
        {
            return DefaultResources.ResolveProjectileVfxId(meta);
        }

        public static ProjectileMO TryGetProjectile(int templateId)
        {
            return DefaultResources.TryGetProjectile(templateId);
        }

        public static AoeMO TryGetAoe(int templateId)
        {
            return DefaultResources.TryGetAoe(templateId);
        }

        public static VfxDatabase GetOrLoadVfxDb()
        {
            return DefaultResources.GetOrLoadVfxDb();
        }
    }
}

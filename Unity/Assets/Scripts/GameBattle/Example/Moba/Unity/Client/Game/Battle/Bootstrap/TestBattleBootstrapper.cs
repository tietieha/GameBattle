using AbilityKit.Ability.Host.Extensions.Moba.CreateWorld;
using AbilityKit.Demo.Moba.Systems;
using AbilityKit.Game.Battle.Shared.Logging;
using AbilityKit.Protocol.Moba;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AbilityKit.Game.Flow
{
    public sealed class TestBattleBootstrapper : IBattleBootstrapper, IBattleStartConfigProvider
    {
        private BattleStartConfig _config;

        public BattleStartConfig Config => _config;

        public BattleStartPlan Build()
        {
            var cfg = LoadConfig();
            _config = cfg;

            MobaBattleLaunchSpec launchSpec;
            if (cfg.UseRoomGameStartSpec)
            {
                var roomSpec = cfg.BuildRoomGameStartSpec();
                launchSpec = cfg.BuildLaunchSpec(in roomSpec);
            }
            else
            {
                launchSpec = cfg.BuildLaunchSpec();
            }

            var req = launchSpec.ToEnterReq();
            var initData = launchSpec.ToWorldInitData(MobaWorldBootstrapModule.InitOpCode);

            return cfg.BuildPlan(req, initData.Payload, initData.OpCode, launchSpec);
        }

        private static BattleStartConfig LoadConfig()
        {
#if UNITY_EDITOR
            var logger = new UnityBattleLogger();
            var guids = AssetDatabase.FindAssets($"t:{nameof(BattleStartConfig)}");
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<BattleStartConfig>(path);
                if (asset != null)
                {
                    logger.Info($"[TestBattleBootstrapper] Loaded BattleStartConfig from: {path}");
                    return asset;
                }
            }

            logger.Warning("[TestBattleBootstrapper] BattleStartConfig not found via AssetDatabase. Falling back to defaults (Local mode).");
#endif
            return ScriptableObject.CreateInstance<BattleStartConfig>();
        }
    }
}

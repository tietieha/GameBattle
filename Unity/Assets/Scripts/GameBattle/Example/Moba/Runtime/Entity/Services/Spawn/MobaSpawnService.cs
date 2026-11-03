using System;
using AbilityKit.Ability.World.Services;
using AbilityKit.Ability.World.Services.Attributes;
using AbilityKit.Coordinator;

namespace AbilityKit.Demo.Moba.Services
{
    public interface ILogicWorldSpawnService : IService
    {
        bool CreateLogicWorldSpawns(LogicWorldSpawnData[] spawns);
    }

    /// <summary>
    /// 兼容仍会解析出生服务的生成宿主适配器。
    /// 正式 MOBA 启动流程必须在 bootstrap 前提供 WorldInitData，并通过 StartGameStage 启动。
    /// </summary>
    [WorldService(typeof(MobaSpawnService))]
    [WorldService(typeof(ILogicWorldSpawnService))]
    [WorldService(typeof(ISpawnService))]
    public sealed class MobaSpawnService : ILogicWorldSpawnService, ISpawnService
    {
        public bool CreateSpawns(PlayerSpawnData[] spawns)
        {
            return CreateLogicWorldSpawns(ToLogicWorldSpawns(spawns));
        }

        public bool CreateLogicWorldSpawns(LogicWorldSpawnData[] spawns)
        {
            if (spawns == null || spawns.Length == 0)
            {
                MobaRuntimeLog.Warning(MobaRuntimeLogModule.Bootstrap, MobaRuntimeLogPurpose.Validation, nameof(MobaSpawnService), "No spawns to create");
                return false;
            }

            MobaRuntimeLog.Warning(MobaRuntimeLogModule.Bootstrap, MobaRuntimeLogPurpose.Rejection, nameof(MobaSpawnService), "Direct spawn startup is not supported. Provide WorldInitData before bootstrap and start through StartGameStage.");
            return false;
        }

        private static LogicWorldSpawnData[] ToLogicWorldSpawns(PlayerSpawnData[] spawns)
        {
            if (spawns == null || spawns.Length == 0)
            {
                return Array.Empty<LogicWorldSpawnData>();
            }

            var logicWorldSpawns = new LogicWorldSpawnData[spawns.Length];
            for (int i = 0; i < spawns.Length; i++)
            {
                var spawn = spawns[i];
                logicWorldSpawns[i] = new LogicWorldSpawnData(
                    spawn.PlayerId,
                    spawn.CharacterId,
                    spawn.TeamId,
                    spawn.X,
                    spawn.Y,
                    spawn.Z,
                    spawn.Name);
            }

            return logicWorldSpawns;
        }

        public void Dispose()
        {
        }
    }
}

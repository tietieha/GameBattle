using System;
using AbilityKit.Ability.FrameSync.Rollback;
using AbilityKit.Ability.Host;
using AbilityKit.Ability.Host.Extensions.FrameSync;
using AbilityKit.Ability.Host.Extensions.Time;
using AbilityKit.Ability.Host.Extensions.WorldStart;
using AbilityKit.Ability.Host.Framework;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Network.Abstractions;

namespace AbilityKit.Game.Flow
{
    internal static class ConfirmedAuthorityRuntimeModuleFactory
    {
        public static HostRuntimeModuleHost Create(
            float fixedDelta,
            Func<WorldId, IConsumableRemoteFrameSource<PlayerInputCommand[]>> resolveRemoteInputs,
            Func<WorldId, int> resolveIdealFrameLimit)
        {
            return new HostRuntimeModuleHost()
                .Add(new ClientPredictionDriverModule(
                    resolveRemoteInputs: resolveRemoteInputs,
                    resolveLocalInputs: _ => null,
                    resolveIdealFrameLimit: resolveIdealFrameLimit,
                    inputDelayFrames: 0,
                    maxPredictionAheadFrames: 0,
                    minPredictionWindow: 0,
                    backlogEwmaAlpha: 0.20f,
                    enableRollback: false,
                    rollbackHistoryFrames: 0,
                    rollbackCaptureEveryNFrames: 0,
                    buildRollbackRegistry: _ => new RollbackRegistry(),
                    buildComputeHash: _ => null))
                .Add(new ServerFrameTimeModule(fixedDelta))
                .Add(new WorldAutoStartModule());
        }
    }
}

using System;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Core.Logging;

namespace AbilityKit.Game.Flow
{
    internal static class SessionWorldBootstrapValidator
    {
        public static void ValidateServices(IWorld world, string label)
        {
            try
            {
                if (world?.Services == null)
                {
                    Log.Error($"[BattleSessionFeature] {label} bootstrap failed: world.Services is null");
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"[BattleSessionFeature] {label} bootstrap threw");
            }
        }
    }
}

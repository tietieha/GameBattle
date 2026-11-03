using AbilityKit.Core.Logging;
using AbilityKit.Demo.Moba.Config.BattleDemo.MO;
using AbilityKit.Demo.Moba.Config.Core;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleViewAoeConfigResolver
    {
        public AoeMO TryGet(MobaConfigDatabase configs, int templateId)
        {
            if (templateId <= 0) return null;
            if (configs == null) return null;

            try
            {
                return configs.GetAoe(templateId);
            }
            catch (System.Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
        }
    }
}

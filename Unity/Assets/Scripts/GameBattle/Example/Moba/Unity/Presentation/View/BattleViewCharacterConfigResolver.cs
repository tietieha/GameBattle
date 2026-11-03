using AbilityKit.Core.Logging;
using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Game.Battle.Entity;

namespace AbilityKit.Game.Flow
{
    internal sealed class BattleViewCharacterConfigResolver
    {
        public int ResolveModelId(MobaConfigDatabase configs, BattleEntityMetaComponent meta)
        {
            if (meta == null) return 0;
            if (meta.Kind != BattleEntityKind.Character) return 0;
            if (configs == null) return 0;

            try
            {
                var character = configs.GetCharacter(meta.EntityCode);
                return character != null ? character.ModelId : 0;
            }
            catch (System.Exception ex)
            {
                Log.Exception(ex);
                return 0;
            }
        }
    }
}

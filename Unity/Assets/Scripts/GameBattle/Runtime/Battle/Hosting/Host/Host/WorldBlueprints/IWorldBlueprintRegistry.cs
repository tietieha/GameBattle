using AbilityKit.Ability.World.Abstractions;

namespace AbilityKit.Ability.Host.WorldBlueprints
{
    /// <summary>
    /// 世界蓝图注册表接口
    /// </summary>
    public interface IWorldBlueprintRegistry
    {
        bool TryGet(string worldType, out IWorldBlueprint blueprint);
    }
}

using System.Collections.Generic;
using GameViewModules = AbilityKit.Game.View.Modules;

namespace AbilityKit.Game.Flow.Modules
{
    public interface IGameModule<TContext> : GameViewModules.IGameModule<TContext>
    {
    }

    public interface IGameModuleTick<TContext> : GameViewModules.IGameModuleTick<TContext>
    {
    }

    public interface IGameModuleRebind<TContext> : GameViewModules.IGameModuleRebind<TContext>
    {
    }

    public interface IGameModuleId : GameViewModules.IGameModuleId
    {
    }

    public interface IGameModuleDependencies : GameViewModules.IGameModuleDependencies
    {
    }
}

using System;
using System.Collections.Generic;
using GameViewModules = AbilityKit.Game.View.Modules;

namespace AbilityKit.Game.Flow.Modules
{
    public sealed class ModuleHost<TContext, TModule> where TModule : class, IGameModule<TContext>
    {
        private readonly GameViewModules.ModuleHost<TContext, TModule> _inner;

        public ModuleHost(List<TModule> modules, Action<string> fail)
        {
            _inner = new GameViewModules.ModuleHost<TContext, TModule>(modules, fail);
        }

        public IReadOnlyList<TModule> Modules => _inner.Modules;
        public bool IsAttached => _inner.IsAttached;

        public bool TryGetModuleById(string id, out TModule module)
        {
            var found = _inner.TryGetModuleById(id, out var resolved);
            module = resolved!;
            return found;
        }

        public List<string> GetModuleIds()
        {
            var modules = _inner.Modules;
            var ids = new List<string>(modules.Count);
            for (var i = 0; i < modules.Count; i++)
            {
                if (modules[i] is IGameModuleId mid && !string.IsNullOrEmpty(mid.Id))
                {
                    ids.Add(mid.Id);
                }
                else
                {
                    ids.Add(modules[i]?.GetType().Name ?? "<null>");
                }
            }

            return ids;
        }

        public bool TrySortByDependencies()
        {
            return _inner.TrySortByDependencies();
        }

        public void Attach(in TContext ctx)
        {
            _inner.Attach(in ctx);
        }

        public void Detach(in TContext ctx)
        {
            _inner.Detach(in ctx);
        }

        public void Tick(in TContext ctx, float deltaTime)
        {
            _inner.Tick(in ctx, deltaTime);
        }

        public void RebindAll(in TContext ctx)
        {
            _inner.RebindAll(in ctx);
        }

        public void ForEach<TI>(Action<TI> visitor) where TI : class
        {
            _inner.ForEach(visitor);
        }

        public void ForEachReverse<TI>(Action<TI> visitor) where TI : class
        {
            _inner.ForEachReverse(visitor);
        }
    }
}

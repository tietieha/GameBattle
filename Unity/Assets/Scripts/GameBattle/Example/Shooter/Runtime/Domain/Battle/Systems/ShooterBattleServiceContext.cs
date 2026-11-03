using System;
using System.Collections.Generic;
using AbilityKit.World.Svelto;
using Svelto.ECS;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal interface IShooterBattleServiceResolver
    {
        bool TryResolve<T>(out T? service) where T : class;

        T Resolve<T>() where T : class;
    }

    internal sealed class ShooterBattleServiceContext : IShooterBattleServiceResolver
    {
        private readonly ISveltoWorldContext _sveltoContext;
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public ShooterBattleServiceContext(ISveltoWorldContext sveltoContext)
        {
            _sveltoContext = sveltoContext ?? throw new ArgumentNullException(nameof(sveltoContext));
            Add(_sveltoContext)
                .Add(_sveltoContext.EnginesRoot)
                .Add(_sveltoContext.EntitiesDB)
                .Add(_sveltoContext.EntityFactory)
                .Add(_sveltoContext.EntityFunctions);
        }

        public ISveltoWorldContext SveltoContext => _sveltoContext;

        public EnginesRoot EnginesRoot => _sveltoContext.EnginesRoot;

        public EntitiesDB EntitiesDB => _sveltoContext.EntitiesDB;

        public ShooterBattleServiceContext Add<T>(T service) where T : class
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            _services[typeof(T)] = service;
            return this;
        }

        public bool TryResolve<T>(out T? service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var value) && value is T typed)
            {
                service = typed;
                return true;
            }

            service = null;
            return false;
        }

        public T Resolve<T>() where T : class
        {
            if (TryResolve<T>(out var service))
            {
                return service!;
            }

            throw new InvalidOperationException($"Shooter battle service is not registered: {typeof(T).FullName}");
        }
    }
}

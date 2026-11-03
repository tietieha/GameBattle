using System;
using System.Collections.Generic;
using System.Linq;

namespace AbilityKit.Context
{
    /// <summary>
    /// 查询条件
    /// </summary>
    public readonly struct QueryCondition
    {
        public QueryConditionKind Kind { get; }
        public int PropertyTypeId { get; }

        public QueryCondition(QueryConditionKind kind, int propertyTypeId)
        {
            Kind = kind;
            PropertyTypeId = propertyTypeId;
        }
    }

    public enum QueryConditionKind
    {
        With = 0,
        Without = 1
    }

    /// <summary>
    /// 查询器
    /// 用于查询满足条件的实体
    /// </summary>
    public sealed class Query
    {
        private readonly ContextRegistry _registry;
        private readonly List<QueryCondition> _conditions = new List<QueryCondition>();
        private readonly List<Func<ContextRegistry, long, bool>> _entityPredicates = new List<Func<ContextRegistry, long, bool>>();

        internal Query(ContextRegistry registry = null)
        {
            _registry = registry;
        }

        public Query With<T>() where T : IProperty
        {
            var type = PropertyTypeRegistry.Instance.Get<T>() ?? PropertyTypeRegistry.Instance.Register<T>();
            _conditions.Add(new QueryCondition(QueryConditionKind.With, type.Id));
            return this;
        }

        public Query Without<T>() where T : IProperty
        {
            var type = PropertyTypeRegistry.Instance.Get<T>() ?? PropertyTypeRegistry.Instance.Register<T>();
            _conditions.Add(new QueryCondition(QueryConditionKind.Without, type.Id));
            return this;
        }

        public Query Where(Func<long, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            _entityPredicates.Add((_, entityId) => predicate(entityId));
            return this;
        }

        public Query Where<T>(Func<long, T, bool> predicate) where T : class, IProperty
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            _entityPredicates.Add((registry, entityId) =>
            {
                var property = registry.Get<T>(entityId);
                return property != null && predicate(entityId, property);
            });
            return this;
        }

        public IEnumerable<long> Execute()
        {
            if (_registry == null)
                throw new InvalidOperationException("Query was not created by QueryBuilder. Use Execute(ContextRegistry) or ContextRegistry.Query().CreateQuery().");

            return Execute(_registry);
        }

        public IEnumerable<long> Execute(ContextRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));

            IEnumerable<long> candidates = registry.GetAllEntityIds();

            foreach (var condition in _conditions)
            {
                candidates = condition.Kind == QueryConditionKind.With
                    ? candidates.Intersect(registry.GetEntitiesWith(condition.PropertyTypeId))
                    : candidates.Except(registry.GetEntitiesWith(condition.PropertyTypeId));
            }

            foreach (var predicate in _entityPredicates)
            {
                candidates = candidates.Where(entityId => predicate(registry, entityId));
            }

            return candidates.ToList();
        }
    }

    /// <summary>
    /// 查询构建器
    /// </summary>
    public sealed class QueryBuilder
    {
        private readonly ContextRegistry _registry;

        internal QueryBuilder(ContextRegistry registry)
        {
            _registry = registry;
        }

        public Query CreateQuery()
        {
            return new Query(_registry);
        }
    }
}

using System;
using System.Collections.Generic;

namespace AbilityKit.Game.View.Flow
{
    public sealed class PhaseStateFeatureRegistry<TKey, TContext, TFeature>
        where TKey : notnull
        where TFeature : class, IPhaseFeature<TContext>
    {
        private readonly Dictionary<TKey, PhaseStateFeatureBinding<TContext, TFeature>> _bindings;
        private readonly Action<string>? _fail;

        public PhaseStateFeatureRegistry(Action<string>? fail = null, int initialCapacity = 8)
        {
            _bindings = new Dictionary<TKey, PhaseStateFeatureBinding<TContext, TFeature>>(Math.Max(0, initialCapacity));
            _fail = fail;
        }

        public int Count => _bindings.Count;

        public PhaseStateFeatureRegistry<TKey, TContext, TFeature> Add(TKey key, PhaseStateFeatureBinding<TContext, TFeature> binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            _bindings.Add(key, binding);
            return this;
        }

        public PhaseStateFeatureRegistry<TKey, TContext, TFeature> Set(TKey key, PhaseStateFeatureBinding<TContext, TFeature> binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            _bindings[key] = binding;
            return this;
        }

        public bool Remove(TKey key)
        {
            return _bindings.Remove(key);
        }

        public bool TryGet(TKey key, out PhaseStateFeatureBinding<TContext, TFeature>? binding)
        {
            return _bindings.TryGetValue(key, out binding);
        }

        public int Enter(TKey key, in TContext ctx)
        {
            if (!_bindings.TryGetValue(key, out var binding))
            {
                _fail?.Invoke($"Phase state binding not registered: {key}");
                return 0;
            }
 
            return binding.Enter(in ctx);
        }
 
        public bool Exit(TKey key, in TContext ctx)
        {
            if (!_bindings.TryGetValue(key, out var binding))
            {
                _fail?.Invoke($"Phase state binding not registered: {key}");
                return false;
            }
 
            binding.Exit(in ctx);
            return true;
        }
    }
}

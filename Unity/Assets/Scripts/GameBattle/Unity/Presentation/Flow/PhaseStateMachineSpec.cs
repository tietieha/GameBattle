using System;
using System.Collections.Generic;

namespace AbilityKit.Game.View.Flow
{
    public sealed class PhaseStateMachineSpec<TKey, TEvent>
        where TKey : notnull
        where TEvent : notnull
    {
        private readonly List<TKey> _states;
        private readonly List<PhaseStateTransitionSpec<TKey, TEvent>> _transitions;
        private bool _hasStartState;
        private bool _isFrozen;
        private TKey _startState;

        public PhaseStateMachineSpec(string id, int initialStateCapacity = 8, int initialTransitionCapacity = 8)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("State machine id is required.", nameof(id));

            Id = id;
            _states = new List<TKey>(Math.Max(0, initialStateCapacity));
            _transitions = new List<PhaseStateTransitionSpec<TKey, TEvent>>(Math.Max(0, initialTransitionCapacity));
            _startState = default!;
        }

        public string Id { get; }
        public bool HasStartState => _hasStartState;
        public bool IsFrozen => _isFrozen;
        public TKey StartState => _hasStartState ? _startState : throw new InvalidOperationException($"State machine '{Id}' has no start state.");
        public IReadOnlyList<TKey> States => _states;
        public IReadOnlyList<PhaseStateTransitionSpec<TKey, TEvent>> Transitions => _transitions;

        public PhaseStateMachineSpec<TKey, TEvent> Freeze()
        {
            _isFrozen = true;
            return this;
        }

        public PhaseStateMachineSpec<TKey, TEvent> AddState(TKey state)
        {
            ThrowIfFrozen();
            if (state == null) throw new ArgumentNullException(nameof(state));

            _states.Add(state);
            return this;
        }

        public PhaseStateMachineSpec<TKey, TEvent> SetStartState(TKey state)
        {
            ThrowIfFrozen();
            if (state == null) throw new ArgumentNullException(nameof(state));

            _startState = state;
            _hasStartState = true;
            return this;
        }

        public PhaseStateMachineSpec<TKey, TEvent> AddTransition(TEvent trigger, TKey from, TKey to, string? conditionId = null)
        {
            ThrowIfFrozen();
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));

            _transitions.Add(new PhaseStateTransitionSpec<TKey, TEvent>(trigger, from, to, conditionId));
            return this;
        }

        private void ThrowIfFrozen()
        {
            if (_isFrozen)
            {
                throw new InvalidOperationException($"Phase state machine spec '{Id}' is frozen.");
            }
        }
    }

    public readonly struct PhaseStateTransitionSpec<TKey, TEvent>
        where TKey : notnull
        where TEvent : notnull
    {
        public PhaseStateTransitionSpec(TEvent trigger, TKey from, TKey to, string? conditionId = null)
        {
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
            ConditionId = conditionId;
        }

        public TEvent Trigger { get; }
        public TKey From { get; }
        public TKey To { get; }
        public string? ConditionId { get; }
    }
}

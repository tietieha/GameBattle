using System;
using System.Collections.Generic;
using System.Linq;
using AbilityKit.Game.View.Foundation;

namespace AbilityKit.Game.View.Flow
{
    public sealed class PhaseStateMachineValidationResult
    {
        private readonly List<string> _errors;

        internal PhaseStateMachineValidationResult(List<string> errors)
        {
            _errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public bool IsValid => _errors.Count == 0;
        public IReadOnlyList<string> Errors => _errors;
    }

    public sealed class PhaseStateMachineValidator<TKey, TEvent>
        where TKey : notnull
        where TEvent : notnull
    {
        private readonly IEqualityComparer<TKey> _stateComparer;

        public PhaseStateMachineValidator(IEqualityComparer<TKey>? stateComparer = null)
        {
            _stateComparer = stateComparer ?? EqualityComparer<TKey>.Default;
        }

        public PhaseStateMachineValidationResult Validate(
            PhaseStateMachineSpec<TKey, TEvent> spec,
            IReadOnlyCollection<string>? conditionIds = null)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            var errors = new List<string>();
            ValidateStates(spec, errors);
            ValidateTransitions(spec, conditionIds, errors);
            return new PhaseStateMachineValidationResult(errors);
        }

        private void ValidateStates(PhaseStateMachineSpec<TKey, TEvent> spec, List<string> errors)
        {
            using var states = ViewFrameworkPools.GetList<TKey>(spec.States.Count);

            for (var i = 0; i < spec.States.Count; i++)
            {
                var state = spec.States[i];
                if (states.List.Contains(state, _stateComparer))
                {
                    errors.Add($"State machine '{spec.Id}' has duplicated state: {state}");
                }
                else
                {
                    states.List.Add(state);
                }
            }

            if (!spec.HasStartState)
            {
                errors.Add($"State machine '{spec.Id}' has no start state.");
                return;
            }

            if (!states.List.Contains(spec.StartState, _stateComparer))
            {
                errors.Add($"State machine '{spec.Id}' start state is not registered: {spec.StartState}");
            }
        }

        private void ValidateTransitions(
            PhaseStateMachineSpec<TKey, TEvent> spec,
            IReadOnlyCollection<string>? conditionIds,
            List<string> errors)
        {
            for (var i = 0; i < spec.Transitions.Count; i++)
            {
                var transition = spec.Transitions[i];
                if (!ContainsState(spec.States, transition.From))
                {
                    errors.Add($"State machine '{spec.Id}' transition {i} source state is not registered: {transition.From}");
                }

                if (!ContainsState(spec.States, transition.To))
                {
                    errors.Add($"State machine '{spec.Id}' transition {i} target state is not registered: {transition.To}");
                }

                if (!string.IsNullOrEmpty(transition.ConditionId) &&
                    conditionIds != null &&
                    !conditionIds.Contains(transition.ConditionId))
                {
                    errors.Add($"State machine '{spec.Id}' transition {i} references unknown condition id: {transition.ConditionId}");
                }
            }
        }

        private bool ContainsState(IReadOnlyList<TKey> states, TKey state)
        {
            for (var i = 0; i < states.Count; i++)
            {
                if (_stateComparer.Equals(states[i], state))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

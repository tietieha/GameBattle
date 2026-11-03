using System;
using System.Collections.Generic;

namespace AbilityKit.Game.View.Flow
{
    public sealed class PhaseStateFeatureSpec
    {
        private readonly List<string> _featureIds;
        private readonly List<string> _enterBeforeActionIds;
        private readonly List<string> _enterAfterActionIds;
        private readonly List<string> _exitActionIds;
        private readonly List<string> _switchFlowIds;
        private bool _isFrozen;

        public PhaseStateFeatureSpec(string stateId, bool clearBeforeEnter = false, int initialFeatureCapacity = 4)
        {
            if (string.IsNullOrEmpty(stateId)) throw new ArgumentException("State id is required.", nameof(stateId));
 
            StateId = stateId;
            ClearBeforeEnter = clearBeforeEnter;
            _featureIds = new List<string>(Math.Max(0, initialFeatureCapacity));
            _enterBeforeActionIds = new List<string>(1);
            _enterAfterActionIds = new List<string>(1);
            _exitActionIds = new List<string>(1);
            _switchFlowIds = new List<string>(1);
        }
 
        public string StateId { get; }
        public bool ClearBeforeEnter { get; }
        public bool IsFrozen => _isFrozen;
        public IReadOnlyList<string> FeatureIds => _featureIds;
        public IReadOnlyList<string> EnterBeforeActionIds => _enterBeforeActionIds;
        public IReadOnlyList<string> EnterAfterActionIds => _enterAfterActionIds;
        public IReadOnlyList<string> ExitActionIds => _exitActionIds;
        public IReadOnlyList<string> SwitchFlowIds => _switchFlowIds;

        public PhaseStateFeatureSpec Freeze()
        {
            _isFrozen = true;
            return this;
        }

        public PhaseStateFeatureSpec AddFeature(string featureId)
        {
            ThrowIfFrozen();
            if (string.IsNullOrEmpty(featureId)) throw new ArgumentException("Feature id is required.", nameof(featureId));

            _featureIds.Add(featureId);
            return this;
        }

        public PhaseStateFeatureSpec AddEnterBeforeAction(string actionId)
        {
            ThrowIfFrozen();
            AddActionId(actionId, _enterBeforeActionIds, nameof(actionId));
            return this;
        }

        public PhaseStateFeatureSpec AddEnterAfterAction(string actionId)
        {
            ThrowIfFrozen();
            AddActionId(actionId, _enterAfterActionIds, nameof(actionId));
            return this;
        }

        public PhaseStateFeatureSpec AddExitAction(string actionId)
        {
            ThrowIfFrozen();
            AddActionId(actionId, _exitActionIds, nameof(actionId));
            return this;
        }

        public PhaseStateFeatureSpec AddSwitchFlow(string switchFlowId)
        {
            ThrowIfFrozen();
            if (string.IsNullOrEmpty(switchFlowId)) throw new ArgumentException("Switch flow id is required.", nameof(switchFlowId));

            _switchFlowIds.Add(switchFlowId);
            return this;
        }

        private void ThrowIfFrozen()
        {
            if (_isFrozen)
            {
                throw new InvalidOperationException($"Phase state feature spec '{StateId}' is frozen.");
            }
        }

        private static void AddActionId(string actionId, List<string> actionIds, string paramName)
        {
            if (string.IsNullOrEmpty(actionId)) throw new ArgumentException("Action id is required.", paramName);

            actionIds.Add(actionId);
        }
    }
}

using System;
using System.Collections.Generic;
using AbilityKit.Game.View.Foundation;

namespace AbilityKit.Game.View.Flow
{
    public sealed class PhaseStateFeatureValidationResult
    {
        private readonly List<string> _errors;

        internal PhaseStateFeatureValidationResult(List<string> errors)
        {
            _errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public bool IsValid => _errors.Count == 0;
        public IReadOnlyList<string> Errors => _errors;
    }

    public sealed class PhaseStateFeatureValidator
    {
        public PhaseStateFeatureValidationResult Validate(
            IReadOnlyList<PhaseStateFeatureSpec> specs,
            PhaseFeatureCatalog catalog)
        {
            return Validate(specs, catalog, actionCatalog: null);
        }

        public PhaseStateFeatureValidationResult Validate(
            IReadOnlyList<PhaseStateFeatureSpec> specs,
            PhaseFeatureCatalog catalog,
            PhaseActionCatalog? actionCatalog)
        {
            return Validate(specs, catalog, actionCatalog, switchFlowCatalog: null);
        }

        public PhaseStateFeatureValidationResult Validate(
            IReadOnlyList<PhaseStateFeatureSpec> specs,
            PhaseFeatureCatalog catalog,
            PhaseActionCatalog? actionCatalog,
            PhaseSwitchFlowCatalog? switchFlowCatalog)
        {
            if (specs == null) throw new ArgumentNullException(nameof(specs));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
 
            var errors = new List<string>();
            using var stateIds = ViewFrameworkPools.GetList<string>(specs.Count);

            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                if (spec == null)
                {
                    errors.Add($"Phase state spec at index {i} is null.");
                    continue;
                }

                if (stateIds.List.Contains(spec.StateId))
                {
                    errors.Add($"Phase state id duplicated: {spec.StateId}");
                }
                else
                {
                    stateIds.List.Add(spec.StateId);
                }

                ValidateFeatureIds(spec, catalog, errors);
                ValidateActionIds(spec, actionCatalog, errors);
                ValidateSwitchFlowIds(spec, switchFlowCatalog, errors);
            }
 
            return new PhaseStateFeatureValidationResult(errors);
        }

        private static void ValidateFeatureIds(
            PhaseStateFeatureSpec spec,
            PhaseFeatureCatalog catalog,
            List<string> errors)
        {
            using var featureIds = ViewFrameworkPools.GetList<string>(spec.FeatureIds.Count);

            for (var i = 0; i < spec.FeatureIds.Count; i++)
            {
                var featureId = spec.FeatureIds[i];
                if (string.IsNullOrEmpty(featureId))
                {
                    errors.Add($"Phase state '{spec.StateId}' has empty feature id at index {i}.");
                    continue;
                }

                if (featureIds.List.Contains(featureId))
                {
                    errors.Add($"Phase state '{spec.StateId}' references feature id more than once: {featureId}");
                }
                else
                {
                    featureIds.List.Add(featureId);
                }

                if (!catalog.Contains(featureId))
                {
                    errors.Add($"Phase state '{spec.StateId}' references unknown feature id: {featureId}");
                }
            }
        }

        private static void ValidateActionIds(
            PhaseStateFeatureSpec spec,
            PhaseActionCatalog? catalog,
            List<string> errors)
        {
            ValidateActionIds(spec, catalog, spec.EnterBeforeActionIds, "enter before", errors);
            ValidateActionIds(spec, catalog, spec.EnterAfterActionIds, "enter after", errors);
            ValidateActionIds(spec, catalog, spec.ExitActionIds, "exit", errors);
        }

        private static void ValidateActionIds(
            PhaseStateFeatureSpec spec,
            PhaseActionCatalog? catalog,
            IReadOnlyList<string> actionIds,
            string stageName,
            List<string> errors)
        {
            using var ids = ViewFrameworkPools.GetList<string>(actionIds.Count);

            for (var i = 0; i < actionIds.Count; i++)
            {
                var actionId = actionIds[i];
                if (string.IsNullOrEmpty(actionId))
                {
                    errors.Add($"Phase state '{spec.StateId}' has empty {stageName} action id at index {i}.");
                    continue;
                }

                if (ids.List.Contains(actionId))
                {
                    errors.Add($"Phase state '{spec.StateId}' references {stageName} action id more than once: {actionId}");
                }
                else
                {
                    ids.List.Add(actionId);
                }

                if (catalog != null && !catalog.Contains(actionId))
                {
                    errors.Add($"Phase state '{spec.StateId}' references unknown {stageName} action id: {actionId}");
                }
            }
        }

        private static void ValidateSwitchFlowIds(
            PhaseStateFeatureSpec spec,
            PhaseSwitchFlowCatalog? catalog,
            List<string> errors)
        {
            using var ids = ViewFrameworkPools.GetList<string>(spec.SwitchFlowIds.Count);

            for (var i = 0; i < spec.SwitchFlowIds.Count; i++)
            {
                var switchFlowId = spec.SwitchFlowIds[i];
                if (string.IsNullOrEmpty(switchFlowId))
                {
                    errors.Add($"Phase state '{spec.StateId}' has empty switch flow id at index {i}.");
                    continue;
                }

                if (ids.List.Contains(switchFlowId))
                {
                    errors.Add($"Phase state '{spec.StateId}' references switch flow id more than once: {switchFlowId}");
                }
                else
                {
                    ids.List.Add(switchFlowId);
                }

                if (catalog != null && !catalog.Contains(switchFlowId))
                {
                    errors.Add($"Phase state '{spec.StateId}' references unknown switch flow id: {switchFlowId}");
                }
            }
        }
    }
}

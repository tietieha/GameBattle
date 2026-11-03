using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Triggering.Validation
{
    /// <summary>
    /// Validates optional formal execution roots attached to trigger plan entries.
    /// </summary>
    public sealed class ExecutionRootValidator<TCtx> : ITriggerValidator<TCtx>
    {
        private readonly TriggerPlanExecutableValidator _validator = new TriggerPlanExecutableValidator();

        public string Name => "执行树结构校验";
        public int Priority => 2;
        public bool IsCritical => true;

        public ValidationResult Validate(in TriggerPlanDatabase<TCtx> database, in ValidationContext<TCtx> context)
        {
            var result = ValidationResult.Success;
            if (database.Plans == null || database.Plans.Length == 0)
            {
                return result;
            }

            for (int i = 0; i < database.Plans.Length; i++)
            {
                var entry = database.Plans[i];
                if (entry.ExecutionRoot == null)
                {
                    continue;
                }

                var path = $"{entry.GetPath()}.executionRoot";
                var rootResult = _validator.Validate(entry.ExecutionRoot, path);
                result.Merge(in rootResult);
            }

            return result;
        }
    }
}

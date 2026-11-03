using System.Collections.Generic;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线快照。
    /// </summary>
    public class AbilityPipelineSnapshot
    {
        /// <summary>
        /// 各阶段状态快照。
        /// </summary>
        public Dictionary<string, object?> PhaseStates { get; set; } = new Dictionary<string, object?>();

        /// <summary>
        /// 关联的管线上下文。
        /// </summary>
        public IAbilityPipelineContext? Context { get; set; }
    }
}

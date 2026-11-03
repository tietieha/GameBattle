using System;
using System.Collections.Generic;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线扩展点处理逻辑。
    /// </summary>
    public abstract partial class AbilityPipeline<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        private Dictionary<AbilityPipelinePhaseId, List<IAbilityPipelineExtensionPoint<TCtx>>> _extensionPoints =
            new Dictionary<AbilityPipelinePhaseId, List<IAbilityPipelineExtensionPoint<TCtx>>>(8);
    
        /// <summary>
        /// 添加阶段扩展点。
        /// </summary>
        public void AddExtensionPoint(AbilityPipelinePhaseId phaseId, IAbilityPipelineExtensionPoint<TCtx> extension, int order = 0)
        {
            if (!_extensionPoints.TryGetValue(phaseId, out var list))
            {
                list = new List<IAbilityPipelineExtensionPoint<TCtx>>(4);
                _extensionPoints[phaseId] = list;
            }
            list.Add(extension);
        }
    
        /// <summary>
        /// 触发阶段开始扩展点。
        /// </summary>
        protected void ExecuteExtensionPhaseStart(AbilityPipelinePhaseId phaseId, TCtx context, IAbilityPipelinePhase<TCtx> phase)
        {
            if (_extensionPoints.TryGetValue(phaseId, out var extensions))
            {
                for (int i = 0; i < extensions.Count; i++)
                {
                    extensions[i].OnPhaseStart(context, phase);
                }
            }
        }
    
        /// <summary>
        /// 触发阶段完成扩展点。
        /// </summary>
        protected void ExecuteExtensionPhaseComplete(AbilityPipelinePhaseId phaseId, TCtx context, IAbilityPipelinePhase<TCtx> phase)
        {
            if (_extensionPoints.TryGetValue(phaseId, out var extensions))
            {
                for (int i = 0; i < extensions.Count; i++)
                {
                    extensions[i].OnPhaseComplete(context, phase);
                }
            }
        }
    }
}

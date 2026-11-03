using System.Collections.Generic;
using AbilityKit.Demo.Moba.Services;
using AbilityKit.Demo.Moba.Services.Buffs.Runtime;
using AbilityKit.GameplayTags;
using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace AbilityKit.Demo.Moba.Components
{
    [Actor]
    public sealed class BuffsComponent : IComponent
    {
        public List<BuffRuntime> Active;
    }

    public sealed class BuffRuntime : AbilityKit.Buffs.BuffState
    {
        public MobaGameplayOrigin Origin;
        public MobaContextSourceView ContextSource;
        public MobaSkillCastRuntimeHandle SkillRuntimeHandle;
        public MobaSkillRuntimeRetainHandle SkillRuntimeRetainHandle;
        public ContinuousTagRequirements TagRequirements;
        public BuffContinuousRuntime Continuous;
        public List<BuffModifierBinding> ModifierBindings;
    }

    public sealed class BuffModifierBinding
    {
        public int ModifierId;
        public float Value;
    }
}

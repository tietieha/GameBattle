using System;

namespace AbilityKit.Demo.Moba.Share.Config
{
    public enum EHandlerType
    {
        CheckCooldown = 1,
        CheckResource = 2,
        CheckState = 3,
        CheckTarget = 4,

        ConsumeResource = 101,
        StartCooldown = 102,
        ApplyBuff = 103,
        AddTag = 104,
        RemoveTag = 105,

        CustomAction = 1000,
    }

    [Serializable]
    public abstract class SkillHandlerDTO
    {
        public int Type;
    }

    [Serializable]
    public class CheckCooldownDTO : SkillHandlerDTO
    {
        public CheckCooldownDTO()
        {
            Type = (int)EHandlerType.CheckCooldown;
        }
    }

    [Serializable]
    public class CheckResourceDTO : SkillHandlerDTO
    {
        public int ResourceType;
        public NumericRefDTO MinAmount;

        public CheckResourceDTO()
        {
            Type = (int)EHandlerType.CheckResource;
        }
    }

    [Serializable]
    public class CheckStateDTO : SkillHandlerDTO
    {
        public string[] RequiredTags;
        public string[] BlockedTags;
        public int Target;

        public CheckStateDTO()
        {
            Type = (int)EHandlerType.CheckState;
        }
    }

    [Serializable]
    public class CheckTargetDTO : SkillHandlerDTO
    {
        public bool RequireTarget;
        public NumericRefDTO MinDistance;
        public NumericRefDTO MaxDistance;
        public string[] TargetTags;

        public CheckTargetDTO()
        {
            Type = (int)EHandlerType.CheckTarget;
        }
    }

    [Serializable]
    public class ConsumeResourceDTO : SkillHandlerDTO
    {
        public int ResourceType;
        public NumericRefDTO Amount;
        public string FailMessageKey;

        public ConsumeResourceDTO()
        {
            Type = (int)EHandlerType.ConsumeResource;
        }
    }

    [Serializable]
    public class StartCooldownDTO : SkillHandlerDTO
    {
        public NumericRefDTO CooldownMs;

        public StartCooldownDTO()
        {
            Type = (int)EHandlerType.StartCooldown;
        }
    }

    [Serializable]
    public class ApplyBuffDTO : SkillHandlerDTO
    {
        public int BuffId;
        public int Target;
        public int StackPolicy;

        public ApplyBuffDTO()
        {
            Type = (int)EHandlerType.ApplyBuff;
        }
    }

    [Serializable]
    public class AddTagDTO : SkillHandlerDTO
    {
        public string[] Tags;
        public int Target;
        public NumericRefDTO DurationMs;

        public AddTagDTO()
        {
            Type = (int)EHandlerType.AddTag;
        }
    }

    [Serializable]
    public class RemoveTagDTO : SkillHandlerDTO
    {
        public string[] Tags;
        public int Target;

        public RemoveTagDTO()
        {
            Type = (int)EHandlerType.RemoveTag;
        }
    }

    [Serializable]
    public class CustomActionDTO : SkillHandlerDTO
    {
        public string ActionName;
        public NamedArgDTO[] Args;

        public CustomActionDTO()
        {
            Type = (int)EHandlerType.CustomAction;
        }
    }

    [Serializable]
    public class NamedArgDTO
    {
        public string Name;
        public NumericRefDTO Value;
    }

    [Serializable]
    public class SkillFlowHandlerConfigDTO
    {
        public SkillHandlerDTO[] PreCastHandlers;
        public SkillHandlerDTO[] PostCastHandlers;
        public SkillHandlerDTO[] OnFailHandlers;
    }
}

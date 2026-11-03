using System;

namespace AbilityKit.Demo.Moba.Share.Config
{
    [Serializable]
    public sealed class CharacterDTO
    {
        public int Id;
        public string Name;
        public int ModelId;
        public int AttributeTemplateId;
        public int[] SkillIds;
        public int[] PassiveSkillIds;
    }
}

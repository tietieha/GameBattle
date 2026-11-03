using System;

namespace AbilityKit.Triggering.Runtime.Config.Cue
{
    /// <summary>
    /// Cue 配置实现（静态配置数据）。
    /// 只表达通用 cue 分类、模板/资源标识与扩展载荷，不包含具体表现业务字段。
    /// </summary>
    [Serializable]
    public struct CueConfig : ICueConfig
    {
        public ECueKind Kind { get; set; }
        public ECueLevel Level { get; set; }
        public string CueId { get; set; }
        public string PrimaryAssetId { get; set; }
        public string SecondaryAssetId { get; set; }
        public string ExtraData { get; set; }

        public static CueConfig None => new CueConfig { Kind = ECueKind.None };

        public static CueConfig Create(
            ECueKind kind,
            string cueId = null,
            string primaryAssetId = null,
            string secondaryAssetId = null,
            string extraData = null,
            ECueLevel level = ECueLevel.Trigger) => new CueConfig
        {
            Kind = kind,
            Level = level == ECueLevel.None ? ECueLevel.Trigger : level,
            CueId = cueId,
            PrimaryAssetId = primaryAssetId,
            SecondaryAssetId = secondaryAssetId,
            ExtraData = extraData
        };

        public static CueConfig Descriptor(string cueId, string primaryAssetId = null, string secondaryAssetId = null, string extraData = null, ECueLevel level = ECueLevel.Trigger) => new CueConfig
        {
            Kind = ECueKind.Descriptor,
            Level = level == ECueLevel.None ? ECueLevel.Trigger : level,
            CueId = cueId,
            PrimaryAssetId = primaryAssetId,
            SecondaryAssetId = secondaryAssetId,
            ExtraData = extraData
        };

        public static CueConfig Behavior(string cueId, string primaryAssetId = null, string secondaryAssetId = null, string extraData = null) =>
            Descriptor(cueId, primaryAssetId, secondaryAssetId, extraData, ECueLevel.Behavior);

        public bool IsEmpty =>
            Kind == ECueKind.None &&
            string.IsNullOrEmpty(CueId) &&
            string.IsNullOrEmpty(PrimaryAssetId) &&
            string.IsNullOrEmpty(SecondaryAssetId) &&
            string.IsNullOrEmpty(ExtraData);
    }
}
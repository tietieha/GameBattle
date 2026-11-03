namespace AbilityKit.Triggering.Runtime.Config.Cue
{
    /// <summary>
    /// Cue 配置接口（静态配置数据）。
    /// 框架层只暴露通用的 CueId / PrimaryAssetId / SecondaryAssetId / ExtraData 语义，不绑定具体表现业务字段。
    /// </summary>
    public interface ICueConfig
    {
        ECueKind Kind { get; }
        ECueLevel Level { get; }
        string CueId { get; }
        string PrimaryAssetId { get; }
        string SecondaryAssetId { get; }
        string ExtraData { get; }
        bool IsEmpty { get; }
    }
}
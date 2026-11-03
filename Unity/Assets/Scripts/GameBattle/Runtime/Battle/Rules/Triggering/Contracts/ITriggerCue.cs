using AbilityKit.Triggering.Runtime.Config.Cue;

namespace AbilityKit.Triggering.Runtime
{
    /// <summary>
    /// Trigger Cue 的通用描述数据。
    /// 该结构只表达框架层可理解的 cue 分类、标识与扩展载荷，不绑定具体业务字段。
    /// </summary>
    public readonly struct TriggerCueDescriptor
    {
        public static readonly TriggerCueDescriptor Empty = default;

        public readonly Config.ECueLevel Level;
        public readonly string Kind;
        public readonly string CueId;
        public readonly string PrimaryAssetId;
        public readonly string SecondaryAssetId;
        public readonly string Payload;

        public TriggerCueDescriptor(
            string kind,
            string cueId = null,
            string primaryAssetId = null,
            string secondaryAssetId = null,
            string payload = null,
            Config.ECueLevel level = Config.ECueLevel.Trigger)
        {
            Level = level == Config.ECueLevel.None ? Config.ECueLevel.Trigger : level;
            Kind = kind;
            CueId = cueId;
            PrimaryAssetId = primaryAssetId;
            SecondaryAssetId = secondaryAssetId;
            Payload = payload;
        }

        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(Kind) &&
            string.IsNullOrWhiteSpace(CueId) &&
            string.IsNullOrWhiteSpace(PrimaryAssetId) &&
            string.IsNullOrWhiteSpace(SecondaryAssetId) &&
            string.IsNullOrWhiteSpace(Payload);

        public static TriggerCueDescriptor FromConfig(ICueConfig cueConfig)
        {
            if (cueConfig == null || cueConfig.IsEmpty) return Empty;

            return new TriggerCueDescriptor(
                kind: cueConfig.Kind.ToString(),
                cueId: cueConfig.CueId,
                primaryAssetId: cueConfig.PrimaryAssetId,
                secondaryAssetId: cueConfig.SecondaryAssetId,
                payload: cueConfig.ExtraData,
                level: cueConfig.Level);
        }
    }

    /// <summary>
    /// 触发器 Cue 回调接口。
    /// 用于在触发器的各个生命周期节点执行对应回调。
    ///
    /// 使用方式：
    /// - 推荐使用泛型版本 ITriggerCue&lt;TCueParams&gt; 以获得类型安全的参数
    /// - 如需通用接入，可使用非泛型版本，通过 context.Args 读取上下文参数
    /// </summary>
    public interface ITriggerCue
    {
        /// <summary>
        /// 条件评估成功，进入 Execute 阶段前调用。
        /// </summary>
        void OnConditionPassed(in TriggerCueContext context);

        /// <summary>
        /// 条件评估失败，触发器跳过前调用。
        /// </summary>
        void OnConditionFailed(in TriggerCueContext context);

        /// <summary>
        /// 行为执行前调用（每个 Action 调用前都会触发）。
        /// </summary>
        void OnBeforeAction(in TriggerCueContext context, int actionIndex);

        /// <summary>
        /// 所有行为执行完成后调用。
        /// </summary>
        void OnExecuted(in TriggerCueContext context);

        /// <summary>
        /// 触发器被显式打断（ExecutionControl.StopPropagation/Cancel）时调用。
        /// </summary>
        void OnInterrupted(in TriggerCueContext context);

        /// <summary>
        /// 触发器因优先级机制被跳过（ShouldBlock 返回 true）时调用。
        /// </summary>
        void OnSkipped(in TriggerCueContext context);
    }

    /// <summary>
    /// 泛型 Trigger Cue 接口。
    /// TCueParams 必须实现 ICueParams 接口，用于提供类型安全的上下文参数。
    ///
    /// 示例：
    /// <code>
    /// public readonly struct DamageCueParams : ICueParams
    /// {
    ///     public double DamageValue;
    ///     public bool IsCritical;
    /// }
    ///
    /// public sealed class DamageCue : ITriggerCue&lt;DamageCueParams&gt;
    /// {
    ///     public void OnExecuted(in TriggerCueContext&lt;DamageCueParams&gt; context)
    ///     {
    ///         // 编译期类型安全，context.Args 就是 DamageCueParams
    ///         var scale = 1.0f + (context.Args.DamageValue / 1000f);
    ///     }
    /// }
    /// </code>
    /// </summary>
    public interface ITriggerCue<TCueParams> : ITriggerCue
        where TCueParams : ICueParams
    {
        /// <summary>
        /// 条件评估成功，进入 Execute 阶段前调用
        /// </summary>
        void OnConditionPassed(in TriggerCueContext<TCueParams> context);

        /// <summary>
        /// 条件评估失败，触发器跳过前调用
        /// </summary>
        void OnConditionFailed(in TriggerCueContext<TCueParams> context);

        /// <summary>
        /// 行为执行前调用
        /// </summary>
        void OnBeforeAction(in TriggerCueContext<TCueParams> context, int actionIndex);

        /// <summary>
        /// 所有行为执行完成后调用
        /// </summary>
        void OnExecuted(in TriggerCueContext<TCueParams> context);

        /// <summary>
        /// 触发器被显式打断时调用
        /// </summary>
        void OnInterrupted(in TriggerCueContext<TCueParams> context);

        /// <summary>
        /// 触发器因优先级机制被跳过时调用
        /// </summary>
        void OnSkipped(in TriggerCueContext<TCueParams> context);
    }
}

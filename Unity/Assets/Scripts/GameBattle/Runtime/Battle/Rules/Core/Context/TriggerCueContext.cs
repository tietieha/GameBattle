using AbilityKit.Triggering.Runtime.Config;

namespace AbilityKit.Triggering.Runtime
{
    /// <summary>
    /// 可选的 Cue 生命周期扩展数据提供者。
    /// 事件参数实现该接口后，TriggerRunner 与行为级 Action 派发会把返回的数据写入 TriggerCueContext.CueData/CuePayload。
    /// </summary>
    public interface ITriggerCueDataProvider
    {
        bool TryGetCueData(
            ECueLevel cueLevel,
            ECueLifecycleStage cueStage,
            int actionIndex,
            in TriggerCueDescriptor cueDescriptor,
            out object cueData,
            out string cuePayload);
    }

    /// <summary>
    /// 触发器 Cue 回调的上下文
    /// 携带触发器调度时的全部信息，供 Cue 层渲染使用
    /// </summary>
    /// <typeparam name="TCueParams">业务层定义的参数类型，实现 ICueParams 接口</typeparam>
    public readonly struct TriggerCueContext<TCueParams>
        where TCueParams : ICueParams
    {
        /// <summary>事件 ID（StableStringId 或 IntId）</summary>
        public readonly int EventId;

        /// <summary>事件类型名称</summary>
        public readonly string EventName;

        /// <summary>事件参数（业务层定义的参数结构）</summary>
        public readonly TCueParams Args;

        /// <summary>触发器 Phase</summary>
        public readonly int Phase;

        /// <summary>触发器 Priority</summary>
        public readonly int Priority;

        /// <summary>触发器注册顺序号（全局自增，用于同 Phase/Priority 时的稳定排序）</summary>
        public readonly long Order;

        /// <summary>触发器唯一标识（TriggerPlan.TriggerId）</summary>
        public readonly int TriggerId;

        /// <summary>触发器类型名称（用于调试溯源）</summary>
        public readonly string TriggerTypeName;

        /// <summary>打断原因（当触发器被跳过或打断时有效）</summary>
        public readonly ETriggerShortCircuitReason InterruptReason;

        /// <summary>打断来源名称（用于调试）</summary>
        public readonly string InterruptSourceName;

        /// <summary>打断触发器的 TriggerId（用于溯源）</summary>
        public readonly int InterruptTriggerId;

        /// <summary>打断时条件是否通过</summary>
        public readonly bool InterruptConditionPassed;

        /// <summary>打断控制句柄（可能为 null）</summary>
        public readonly ExecutionControl Control;

        /// <summary>Cue 归属级别：Trigger 或 Behavior。</summary>
        public readonly ECueLevel CueLevel;

        /// <summary>当前 Cue 生命周期阶段。</summary>
        public readonly ECueLifecycleStage CueStage;

        /// <summary>行为级 Cue 对应的 Action/行为索引；非行为级阶段为 -1。</summary>
        public readonly int ActionIndex;

        /// <summary>当前生命周期回调对应的 Cue 描述。行为级 Cue 可用它覆盖触发器级默认描述。</summary>
        public readonly TriggerCueDescriptor CueDescriptor;

        /// <summary>当前生命周期回调携带的通用扩展数据。业务层可在 Cue 实现中按需转换读取。</summary>
        public readonly object CueData;

        /// <summary>当前生命周期回调携带的通用文本载荷。未显式指定时默认使用 CueDescriptor.Payload。</summary>
        public readonly string CuePayload;

        public TriggerCueContext(
            int eventId,
            string eventName,
            TCueParams args,
            int phase,
            int priority,
            long order,
            int triggerId,
            string triggerTypeName,
            ETriggerShortCircuitReason interruptReason,
            string interruptSourceName,
            int interruptTriggerId,
            bool interruptConditionPassed,
            ExecutionControl control)
            : this(
                eventId,
                eventName,
                args,
                phase,
                priority,
                order,
                triggerId,
                triggerTypeName,
                interruptReason,
                interruptSourceName,
                interruptTriggerId,
                interruptConditionPassed,
                control,
                ECueLevel.Trigger,
                ECueLifecycleStage.None,
                -1)
        {
        }

        public TriggerCueContext(
            int eventId,
            string eventName,
            TCueParams args,
            int phase,
            int priority,
            long order,
            int triggerId,
            string triggerTypeName,
            ETriggerShortCircuitReason interruptReason,
            string interruptSourceName,
            int interruptTriggerId,
            bool interruptConditionPassed,
            ExecutionControl control,
            ECueLevel cueLevel,
            ECueLifecycleStage cueStage,
            int actionIndex,
            in TriggerCueDescriptor cueDescriptor = default,
            object cueData = null,
            string cuePayload = null)
        {
            EventId = eventId;
            EventName = eventName;
            Args = args;
            Phase = phase;
            Priority = priority;
            Order = order;
            TriggerId = triggerId;
            TriggerTypeName = triggerTypeName;
            InterruptReason = interruptReason;
            InterruptSourceName = interruptSourceName;
            InterruptTriggerId = interruptTriggerId;
            InterruptConditionPassed = interruptConditionPassed;
            Control = control;
            CueLevel = cueLevel;
            CueStage = cueStage;
            ActionIndex = actionIndex;
            CueDescriptor = cueDescriptor.IsEmpty ? TriggerCueDescriptor.Empty : cueDescriptor;
            CueData = cueData;
            CuePayload = cuePayload ?? CueDescriptor.Payload;
        }
    }

    /// <summary>
    /// 非泛型 TriggerCueContext（向后兼容）
    /// Args 使用 object 类型，需要强制转换
    /// </summary>
    public readonly struct TriggerCueContext
    {
        /// <summary>事件 ID（StableStringId 或 IntId）</summary>
        public readonly int EventId;

        /// <summary>事件类型名称</summary>
        public readonly string EventName;

        /// <summary>事件参数（Payload）</summary>
        public readonly object Args;

        /// <summary>触发器 Phase</summary>
        public readonly int Phase;

        /// <summary>触发器 Priority</summary>
        public readonly int Priority;

        /// <summary>触发器注册顺序号（全局自增，用于同 Phase/Priority 时的稳定排序）</summary>
        public readonly long Order;

        /// <summary>触发器唯一标识（TriggerPlan.TriggerId）</summary>
        public readonly int TriggerId;

        /// <summary>触发器类型名称（用于调试溯源）</summary>
        public readonly string TriggerTypeName;

        /// <summary>打断原因（当触发器被跳过或打断时有效）</summary>
        public readonly ETriggerShortCircuitReason InterruptReason;

        /// <summary>打断来源名称（用于调试）</summary>
        public readonly string InterruptSourceName;

        /// <summary>打断触发器的 TriggerId（用于溯源）</summary>
        public readonly int InterruptTriggerId;

        /// <summary>打断时条件是否通过</summary>
        public readonly bool InterruptConditionPassed;

        /// <summary>打断控制句柄（可能为 null）</summary>
        public readonly ExecutionControl Control;

        /// <summary>Cue 归属级别：Trigger 或 Behavior。</summary>
        public readonly ECueLevel CueLevel;

        /// <summary>当前 Cue 生命周期阶段。</summary>
        public readonly ECueLifecycleStage CueStage;

        /// <summary>行为级 Cue 对应的 Action/行为索引；非行为级阶段为 -1。</summary>
        public readonly int ActionIndex;

        /// <summary>当前生命周期回调对应的 Cue 描述。行为级 Cue 可用它覆盖触发器级默认描述。</summary>
        public readonly TriggerCueDescriptor CueDescriptor;

        /// <summary>当前生命周期回调携带的通用扩展数据。业务层可在 Cue 实现中按需转换读取。</summary>
        public readonly object CueData;

        /// <summary>当前生命周期回调携带的通用文本载荷。未显式指定时默认使用 CueDescriptor.Payload。</summary>
        public readonly string CuePayload;

        public TriggerCueContext(
            int eventId,
            string eventName,
            object args,
            int phase,
            int priority,
            long order,
            int triggerId,
            string triggerTypeName,
            ETriggerShortCircuitReason interruptReason,
            string interruptSourceName,
            int interruptTriggerId,
            bool interruptConditionPassed,
            ExecutionControl control)
            : this(
                eventId,
                eventName,
                args,
                phase,
                priority,
                order,
                triggerId,
                triggerTypeName,
                interruptReason,
                interruptSourceName,
                interruptTriggerId,
                interruptConditionPassed,
                control,
                ECueLevel.Trigger,
                ECueLifecycleStage.None,
                -1)
        {
        }

        public TriggerCueContext(
            int eventId,
            string eventName,
            object args,
            int phase,
            int priority,
            long order,
            int triggerId,
            string triggerTypeName,
            ETriggerShortCircuitReason interruptReason,
            string interruptSourceName,
            int interruptTriggerId,
            bool interruptConditionPassed,
            ExecutionControl control,
            ECueLevel cueLevel,
            ECueLifecycleStage cueStage,
            int actionIndex,
            in TriggerCueDescriptor cueDescriptor = default,
            object cueData = null,
            string cuePayload = null)
        {
            EventId = eventId;
            EventName = eventName;
            Args = args;
            Phase = phase;
            Priority = priority;
            Order = order;
            TriggerId = triggerId;
            TriggerTypeName = triggerTypeName;
            InterruptReason = interruptReason;
            InterruptSourceName = interruptSourceName;
            InterruptTriggerId = interruptTriggerId;
            InterruptConditionPassed = interruptConditionPassed;
            Control = control;
            CueLevel = cueLevel;
            CueStage = cueStage;
            ActionIndex = actionIndex;
            CueDescriptor = cueDescriptor.IsEmpty ? TriggerCueDescriptor.Empty : cueDescriptor;
            CueData = cueData;
            CuePayload = cuePayload ?? CueDescriptor.Payload;
        }
    }
}

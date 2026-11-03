using System;
using System.Collections.Generic;

namespace AbilityKit.Triggering.Runtime.RuleScheduler
{
    /// <summary>
    /// 规则调度语义模式。
    /// 面向自然语言规则拆解后的时间意图，而不是具体业务对象。
    /// </summary>
    public enum ERuleScheduleMode : byte
    {
        Immediate = 0,
        Delayed = 1,
        Every = 2,
        WhileActive = 3,
    }

    /// <summary>
    /// 规则调度运行状态。
    /// </summary>
    public enum ERuleScheduleState : byte
    {
        Registered = 0,
        WaitingDelay = 1,
        Running = 2,
        Paused = 3,
        Completed = 4,
        Interrupted = 5,
        Cancelled = 6,
    }

    /// <summary>
    /// 稳定规则调度句柄。
    /// </summary>
    public readonly struct RuleScheduleHandle : IEquatable<RuleScheduleHandle>
    {
        public readonly string DriverId;
        public readonly int InstanceId;
        public readonly int Version;

        public RuleScheduleHandle(string driverId, int instanceId, int version)
        {
            DriverId = driverId;
            InstanceId = instanceId;
            Version = version;
        }

        public bool IsValid => !string.IsNullOrEmpty(DriverId) && InstanceId > 0 && Version > 0;
        public static RuleScheduleHandle Invalid => default;

        public bool Equals(RuleScheduleHandle other)
        {
            return string.Equals(DriverId, other.DriverId, StringComparison.Ordinal)
                && InstanceId == other.InstanceId
                && Version == other.Version;
        }

        public override bool Equals(object obj) => obj is RuleScheduleHandle other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(DriverId, InstanceId, Version);
        public static bool operator ==(RuleScheduleHandle left, RuleScheduleHandle right) => left.Equals(right);
        public static bool operator !=(RuleScheduleHandle left, RuleScheduleHandle right) => !left.Equals(right);
        public override string ToString() => IsValid ? $"RuleSchedule[{DriverId}:{InstanceId}:{Version}]" : "RuleSchedule[Invalid]";
    }

    /// <summary>
    /// 规则调度计划。
    /// </summary>
    public readonly struct RuleSchedulePlan
    {
        public readonly ERuleScheduleMode Mode;
        public readonly float DelayMs;
        public readonly float IntervalMs;
        public readonly int MaxOccurrences;
        public readonly float Speed;
        public readonly string GroupId;
        public readonly string SubjectId;
        public readonly string Label;
        public readonly bool CanBeInterrupted;
        public readonly bool ReplaceExisting;

        public RuleSchedulePlan(
            ERuleScheduleMode mode,
            float delayMs = 0f,
            float intervalMs = 0f,
            int maxOccurrences = 1,
            float speed = 1f,
            string groupId = null,
            string subjectId = null,
            string label = null,
            bool canBeInterrupted = true,
            bool replaceExisting = false)
        {
            Mode = mode;
            DelayMs = Math.Max(0f, delayMs);
            IntervalMs = Math.Max(0f, intervalMs);
            MaxOccurrences = maxOccurrences;
            Speed = speed <= 0f ? 1f : speed;
            GroupId = groupId;
            SubjectId = subjectId;
            Label = label;
            CanBeInterrupted = canBeInterrupted;
            ReplaceExisting = replaceExisting;
        }

        public static RuleSchedulePlan Now(string groupId = null, string subjectId = null, string label = null)
        {
            return new RuleSchedulePlan(ERuleScheduleMode.Immediate, groupId: groupId, subjectId: subjectId, label: label);
        }

        public static RuleSchedulePlan After(float delayMs, string groupId = null, string subjectId = null, string label = null)
        {
            return new RuleSchedulePlan(ERuleScheduleMode.Delayed, delayMs: delayMs, maxOccurrences: 1, groupId: groupId, subjectId: subjectId, label: label);
        }

        public static RuleSchedulePlan Every(float intervalMs, int maxOccurrences = -1, float delayMs = 0f, string groupId = null, string subjectId = null, string label = null)
        {
            return new RuleSchedulePlan(ERuleScheduleMode.Every, delayMs, intervalMs, maxOccurrences, groupId: groupId, subjectId: subjectId, label: label);
        }

        public static RuleSchedulePlan WhileActive(float intervalMs, float delayMs = 0f, string groupId = null, string subjectId = null, string label = null)
        {
            return new RuleSchedulePlan(ERuleScheduleMode.WhileActive, delayMs, intervalMs, -1, groupId: groupId, subjectId: subjectId, label: label);
        }

        public RuleSchedulePlan WithReplacement(bool replaceExisting = true)
        {
            return new RuleSchedulePlan(Mode, DelayMs, IntervalMs, MaxOccurrences, Speed, GroupId, SubjectId, Label, CanBeInterrupted, replaceExisting);
        }

        public RuleSchedulePlan WithSpeed(float speed)
        {
            return new RuleSchedulePlan(Mode, DelayMs, IntervalMs, MaxOccurrences, speed, GroupId, SubjectId, Label, CanBeInterrupted, ReplaceExisting);
        }
    }

    /// <summary>
    /// 规则调度快照。
    /// </summary>
    public readonly struct RuleScheduleSnapshot
    {
        public readonly RuleScheduleHandle Handle;
        public readonly RuleSchedulePlan Plan;
        public readonly ERuleScheduleState State;
        public readonly float ElapsedMs;
        public readonly float LastExecuteMs;
        public readonly int OccurrenceCount;
        public readonly string InterruptReason;

        public RuleScheduleSnapshot(
            RuleScheduleHandle handle,
            RuleSchedulePlan plan,
            ERuleScheduleState state,
            float elapsedMs,
            float lastExecuteMs,
            int occurrenceCount,
            string interruptReason)
        {
            Handle = handle;
            Plan = plan;
            State = state;
            ElapsedMs = elapsedMs;
            LastExecuteMs = lastExecuteMs;
            OccurrenceCount = occurrenceCount;
            InterruptReason = interruptReason;
        }
    }

    /// <summary>
    /// 单次规则调度执行上下文。
    /// </summary>
    public readonly struct RuleScheduleContext
    {
        public readonly RuleScheduleHandle Handle;
        public readonly RuleSchedulePlan Plan;
        public readonly float DeltaTimeMs;
        public readonly float ScaledDeltaMs;
        public readonly float ElapsedMs;
        public readonly int OccurrenceIndex;
        public readonly object UserContext;

        public RuleScheduleContext(
            RuleScheduleHandle handle,
            RuleSchedulePlan plan,
            float deltaTimeMs,
            float scaledDeltaMs,
            float elapsedMs,
            int occurrenceIndex,
            object userContext)
        {
            Handle = handle;
            Plan = plan;
            DeltaTimeMs = deltaTimeMs;
            ScaledDeltaMs = scaledDeltaMs;
            ElapsedMs = elapsedMs;
            OccurrenceIndex = occurrenceIndex;
            UserContext = userContext;
        }
    }

    /// <summary>
    /// 规则调度效果。
    /// </summary>
    public interface IRuleScheduleEffect
    {
        bool CanExecute(in RuleScheduleContext context);
        void Execute(in RuleScheduleContext context);
        void OnCompleted(in RuleScheduleContext context);
        void OnInterrupted(in RuleScheduleContext context, string reason);
    }

    /// <summary>
    /// 规则调度效果基类。
    /// </summary>
    public abstract class RuleScheduleEffectBase : IRuleScheduleEffect
    {
        public virtual bool CanExecute(in RuleScheduleContext context) => true;
        public abstract void Execute(in RuleScheduleContext context);
        public virtual void OnCompleted(in RuleScheduleContext context) { }
        public virtual void OnInterrupted(in RuleScheduleContext context, string reason) { }
    }

    /// <summary>
    /// 委托式规则调度效果。
    /// </summary>
    public sealed class DelegateRuleScheduleEffect : RuleScheduleEffectBase
    {
        private readonly Action<RuleScheduleContext> _execute;
        private readonly Predicate<RuleScheduleContext> _canExecute;
        private readonly Action<RuleScheduleContext> _onCompleted;
        private readonly Action<RuleScheduleContext, string> _onInterrupted;

        public DelegateRuleScheduleEffect(
            Action<RuleScheduleContext> execute,
            Predicate<RuleScheduleContext> canExecute = null,
            Action<RuleScheduleContext> onCompleted = null,
            Action<RuleScheduleContext, string> onInterrupted = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _onCompleted = onCompleted;
            _onInterrupted = onInterrupted;
        }

        public override bool CanExecute(in RuleScheduleContext context) => _canExecute == null || _canExecute(context);
        public override void Execute(in RuleScheduleContext context) => _execute(context);
        public override void OnCompleted(in RuleScheduleContext context) => _onCompleted?.Invoke(context);
        public override void OnInterrupted(in RuleScheduleContext context, string reason) => _onInterrupted?.Invoke(context, reason);
    }

    /// <summary>
    /// 可替换规则调度驱动。
    /// </summary>
    public interface IRuleSchedulerDriver
    {
        string DriverId { get; }
        RuleScheduleHandle Schedule(in RuleSchedulePlan plan, IRuleScheduleEffect effect);
        bool TryGet(RuleScheduleHandle handle, out RuleScheduleSnapshot snapshot);
        IReadOnlyList<RuleScheduleSnapshot> FindByGroup(string groupId);
        IReadOnlyList<RuleScheduleSnapshot> FindBySubject(string subjectId);
        bool Pause(RuleScheduleHandle handle);
        bool Resume(RuleScheduleHandle handle);
        bool Interrupt(RuleScheduleHandle handle, string reason = null);
        bool Cancel(RuleScheduleHandle handle);
        int InterruptGroup(string groupId, string reason = null);
        int CancelGroup(string groupId);
        void Update(float deltaTimeMs, object userContext = null);
        void Clear();
    }
}

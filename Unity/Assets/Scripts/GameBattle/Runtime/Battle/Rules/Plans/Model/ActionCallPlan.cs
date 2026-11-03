using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime.Config;
using AbilityKit.Triggering.Runtime.Config.Cue;
using AbilityKit.Triggering.Variables.Numeric;

namespace AbilityKit.Triggering.Runtime.Plan
{
    public readonly struct ActionArgumentsPlan
    {
        public readonly byte Arity;
        public readonly NumericValueRef Arg0;
        public readonly NumericValueRef Arg1;
        public readonly Dictionary<string, ActionArgValue> NamedArgs;

        public bool HasNamedArgs => NamedArgs != null && NamedArgs.Count > 0;

        public ActionArgumentsPlan(byte arity, NumericValueRef arg0, NumericValueRef arg1, Dictionary<string, ActionArgValue> namedArgs)
        {
            Arity = arity;
            Arg0 = arg0;
            Arg1 = arg1;
            NamedArgs = namedArgs;
        }
    }

    public readonly struct ActionSchedulePlan
    {
        public readonly Config.EActionScheduleMode Mode;
        public readonly float Param;
        public readonly int MaxExecutions;
        public readonly bool CanBeInterrupted;

        public ActionSchedulePlan(Config.EActionScheduleMode mode, float param, int maxExecutions, bool canBeInterrupted)
        {
            Mode = mode;
            Param = param;
            MaxExecutions = maxExecutions;
            CanBeInterrupted = canBeInterrupted;
        }
    }

    public readonly struct ActionExecutionPlan
    {
        public readonly Config.EActionExecutionPolicy Policy;
        public readonly int RetryMaxRetries;
        public readonly float RetryDelayMs;

        public ActionExecutionPlan(Config.EActionExecutionPolicy policy, int retryMaxRetries = 3, float retryDelayMs = 0f)
        {
            Policy = policy;
            RetryMaxRetries = retryMaxRetries;
            RetryDelayMs = retryDelayMs;
        }
    }

    /// <summary>
    /// Action 调用计划（参数化动作描述）
    /// </summary>
    public readonly struct ActionCallPlan
    {
        public readonly ActionId Id;
        public readonly byte Arity;
        public readonly NumericValueRef Arg0;
        public readonly NumericValueRef Arg1;

        /// <summary>
        /// 具名参数字典（key=参数名，value=参数值引用）
        /// 为 null 时表示向后兼容的位置参数模式（使用 Arg0/Arg1）
        /// </summary>
        public readonly Dictionary<string, ActionArgValue> Args;

        /// <summary>
        /// 调度模式（Action 自身如何运行）
        /// Immediate: 立即执行一次
        /// Delayed: 延迟执行（等待 ScheduleParam 毫秒）
        /// Periodic: 周期执行（每 ScheduleParam 毫秒）
        /// Continuous: 持续调度执行（按 ScheduleParam 间隔，直到外部中断或达到执行次数）
        /// Timeline: 时间线执行（按时间轴序列）
        /// </summary>
        public readonly Config.EActionScheduleMode ScheduleMode;

        /// <summary>
        /// 调度参数
        /// Delayed: 延迟时间（毫秒）
        /// Periodic: 周期间隔（毫秒）
        /// Timeline: 时间线总时长（毫秒）
        /// </summary>
        public readonly float ScheduleParam;

        /// <summary>
        /// 最大执行次数（-1=无限，仅对 Periodic/Delayed 有效）
        /// </summary>
        public readonly int MaxExecutions;

        /// <summary>
        /// 是否可被中断（持续行为有效）
        /// </summary>
        public readonly bool CanBeInterrupted;

        /// <summary>
        /// 执行策略（单次执行的约束）
        /// </summary>
        public readonly Config.EActionExecutionPolicy ExecutionPolicy;

        /// <summary>
        /// WithRetry 策略的最大重试次数。
        /// </summary>
        public readonly int RetryMaxRetries;

        /// <summary>
        /// WithRetry 策略的单次重试延迟（毫秒）。0 表示同帧立即重试。
        /// </summary>
        public readonly float RetryDelayMs;

        /// <summary>
        /// 行为级 Cue 描述。为空时该 Action 不产生行为级 cue。
        /// </summary>
        public readonly TriggerCueDescriptor Cue;

        /// <summary>
        /// 创建无参数的动作调用（默认 Immediate）
        /// </summary>
        public ActionCallPlan(ActionId id)
        {
            Id = id;
            Arity = 0;
            Arg0 = default;
            Arg1 = default;
            Args = null;
            ScheduleMode = Config.EActionScheduleMode.Immediate;
            ScheduleParam = 0;
            MaxExecutions = -1;
            CanBeInterrupted = true;
            ExecutionPolicy = Config.EActionExecutionPolicy.Immediate;
            RetryMaxRetries = 3;
            RetryDelayMs = 0f;
            Cue = TriggerCueDescriptor.Empty;
        }

        /// <summary>
        /// 创建带参数的动作调用（默认 Immediate）
        /// </summary>
        public ActionCallPlan(ActionId id, NumericValueRef arg0, NumericValueRef arg1 = default, NumericValueRef arg2 = default)
        {
            Id = id;
            if (arg2.Kind != ENumericValueRefKind.Const || arg2.ConstValue != 0 || !string.IsNullOrEmpty(arg2.Key) || !string.IsNullOrEmpty(arg2.ExprText))
            {
                Arity = 3;
                Arg0 = arg0;
                Arg1 = arg1;
                Args = new Dictionary<string, ActionArgValue> { ["_2"] = ActionArgValue.Of(arg2, "_2") };
            }
            else if (arg1.Kind != ENumericValueRefKind.Const || arg1.ConstValue != 0 || !string.IsNullOrEmpty(arg1.Key) || !string.IsNullOrEmpty(arg1.ExprText))
            {
                Arity = 2;
                Arg0 = arg0;
                Arg1 = arg1;
                Args = null;
            }
            else if (arg0.Kind != ENumericValueRefKind.Const || arg0.ConstValue != 0 || !string.IsNullOrEmpty(arg0.Key) || !string.IsNullOrEmpty(arg0.ExprText))
            {
                Arity = 1;
                Arg0 = arg0;
                Arg1 = default;
                Args = null;
            }
            else
            {
                Arity = 0;
                Arg0 = default;
                Arg1 = default;
                Args = null;
            }
            ScheduleMode = Config.EActionScheduleMode.Immediate;
            ScheduleParam = 0;
            MaxExecutions = -1;
            CanBeInterrupted = true;
            ExecutionPolicy = Config.EActionExecutionPolicy.Immediate;
            RetryMaxRetries = 3;
            RetryDelayMs = 0f;
            Cue = TriggerCueDescriptor.Empty;
        }

        /// <summary>
        /// 创建带常量参数的动作调用（默认 Immediate）
        /// </summary>
        public ActionCallPlan(ActionId id, params double[] constArgs)
        {
            Id = id;
            switch (constArgs.Length)
            {
                case 0:
                    Arity = 0;
                    Arg0 = default;
                    Arg1 = default;
                    Args = null;
                    break;
                case 1:
                    Arity = 1;
                    Arg0 = NumericValueRef.Const(constArgs[0]);
                    Arg1 = default;
                    Args = null;
                    break;
                case 2:
                    Arity = 2;
                    Arg0 = NumericValueRef.Const(constArgs[0]);
                    Arg1 = NumericValueRef.Const(constArgs[1]);
                    Args = null;
                    break;
                default:
                    Arity = (byte)constArgs.Length;
                    Arg0 = NumericValueRef.Const(constArgs[0]);
                    Arg1 = NumericValueRef.Const(constArgs[1]);
                    Args = new Dictionary<string, ActionArgValue>();
                    for (int i = 2; i < constArgs.Length; i++)
                        Args[$"__{i}"] = ActionArgValue.OfConst(constArgs[i], $"__{i}");
                    break;
            }
            ScheduleMode = Config.EActionScheduleMode.Immediate;
            ScheduleParam = 0;
            MaxExecutions = -1;
            CanBeInterrupted = true;
            ExecutionPolicy = Config.EActionExecutionPolicy.Immediate;
            RetryMaxRetries = 3;
            RetryDelayMs = 0f;
            Cue = TriggerCueDescriptor.Empty;
        }

        /// <summary>
        /// 创建带有具名参数的 ActionCallPlan（默认 Immediate）
        /// </summary>
        public static ActionCallPlan WithArgs(ActionId id, Dictionary<string, ActionArgValue> args)
        {
            return new ActionCallPlan(id, args);
        }

        private ActionCallPlan(ActionId id, Dictionary<string, ActionArgValue> args)
        {
            Id = id;
            Arity = (byte)(args != null ? args.Count : 0);
            Arg0 = default;
            Arg1 = default;
            Args = args;
            ScheduleMode = Config.EActionScheduleMode.Immediate;
            ScheduleParam = 0;
            MaxExecutions = -1;
            CanBeInterrupted = true;
            ExecutionPolicy = Config.EActionExecutionPolicy.Immediate;
            RetryMaxRetries = 3;
            RetryDelayMs = 0f;
            Cue = TriggerCueDescriptor.Empty;
        }

        /// <summary>
        /// 是否使用了具名参数模式
        /// </summary>
        public bool HasNamedArgs => Arguments.HasNamedArgs;

        public ActionArgumentsPlan Arguments => new ActionArgumentsPlan(Arity, Arg0, Arg1, Args);

        public ActionSchedulePlan Schedule => new ActionSchedulePlan(ScheduleMode, ScheduleParam, MaxExecutions, CanBeInterrupted);

        public ActionExecutionPlan Execution => new ActionExecutionPlan(ExecutionPolicy, RetryMaxRetries, RetryDelayMs);

        /// <summary>
        /// 完整构造函数（用于扩展方法创建修改后的副本）
        /// </summary>
        public ActionCallPlan(
            ActionId id,
            byte arity,
            NumericValueRef arg0,
            NumericValueRef arg1,
            Dictionary<string, ActionArgValue> args,
            Config.EActionScheduleMode scheduleMode,
            float scheduleParam,
            int maxExecutions,
            bool canBeInterrupted,
            Config.EActionExecutionPolicy executionPolicy,
            int retryMaxRetries = 3,
            float retryDelayMs = 0f,
            in TriggerCueDescriptor cue = default)
        {
            if (retryMaxRetries < 0) throw new ArgumentOutOfRangeException(nameof(retryMaxRetries));
            if (retryDelayMs < 0f) throw new ArgumentOutOfRangeException(nameof(retryDelayMs));

            Id = id;
            Arity = arity;
            Arg0 = arg0;
            Arg1 = arg1;
            Args = args;
            ScheduleMode = scheduleMode;
            ScheduleParam = scheduleParam;
            MaxExecutions = maxExecutions;
            CanBeInterrupted = canBeInterrupted;
            ExecutionPolicy = executionPolicy;
            RetryMaxRetries = retryMaxRetries;
            RetryDelayMs = retryDelayMs;
            Cue = cue.IsEmpty ? TriggerCueDescriptor.Empty : cue;
        }
    }
}

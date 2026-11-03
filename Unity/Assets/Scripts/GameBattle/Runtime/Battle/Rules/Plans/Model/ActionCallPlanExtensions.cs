using System.Collections.Generic;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime.Config;
using AbilityKit.Triggering.Variables.Numeric;

namespace AbilityKit.Triggering.Runtime.Plan
{
    /// <summary>
    /// ActionCallPlan 的扩展方法
    /// </summary>
    public static class ActionCallPlanExtensions
    {
        /// <summary>
        /// 创建无参数的动作调用（默认 Immediate）
        /// </summary>
        public static ActionCallPlan Call(ActionId id)
        {
            return new ActionCallPlan(id);
        }

        /// <summary>
        /// 创建带一个参数的动作调用（默认 Immediate）
        /// </summary>
        public static ActionCallPlan Call(this ActionId id, NumericValueRef arg0)
        {
            return new ActionCallPlan(id, arg0);
        }

        /// <summary>
        /// 创建带两个参数的动作调用（默认 Immediate）
        /// </summary>
        public static ActionCallPlan Call(this ActionId id, NumericValueRef arg0, NumericValueRef arg1)
        {
            return new ActionCallPlan(id, arg0, arg1);
        }

        /// <summary>
        /// 创建带有具名参数的动作调用（默认 Immediate）
        /// </summary>
        public static ActionCallPlan CallArgs(this ActionId id, Dictionary<string, ActionArgValue> args)
        {
            return ActionCallPlan.WithArgs(id, args);
        }

        /// <summary>
        /// 创建带有两个具名参数的动作调用（默认 Immediate）
        /// </summary>
        public static ActionCallPlan CallArgs(this ActionId id, string name0, double value0, string name1, double value1)
        {
            return ActionCallPlan.WithArgs(id, new Dictionary<string, ActionArgValue>
            {
                [name0] = ActionArgValue.OfConst(value0, name0),
                [name1] = ActionArgValue.OfConst(value1, name1)
            });
        }

        /// <summary>
        /// 创建带有三个具名参数的动作调用（默认 Immediate）
        /// </summary>
        public static ActionCallPlan CallArgs(this ActionId id, string name0, double value0, string name1, double value1, string name2, double value2)
        {
            return ActionCallPlan.WithArgs(id, new Dictionary<string, ActionArgValue>
            {
                [name0] = ActionArgValue.OfConst(value0, name0),
                [name1] = ActionArgValue.OfConst(value1, name1),
                [name2] = ActionArgValue.OfConst(value2, name2)
            });
        }

        /// <summary>
        /// 创建带具名参数的动作调用（扩展方法版本）
        /// </summary>
        public static ActionCallPlan WithArgs(this ActionId id, Dictionary<string, ActionArgValue> args)
        {
            return ActionCallPlan.WithArgs(id, args);
        }

        /// <summary>
        /// 创建带两个具名参数的动作调用（扩展方法版本）
        /// </summary>
        public static ActionCallPlan WithArgs(this ActionId id, string name0, double value0, string name1, double value1)
        {
            return ActionCallPlan.WithArgs(id, new Dictionary<string, ActionArgValue>
            {
                [name0] = ActionArgValue.OfConst(value0, name0),
                [name1] = ActionArgValue.OfConst(value1, name1)
            });
        }

        /// <summary>
        /// 创建带三个具名参数的动作调用（扩展方法版本）
        /// </summary>
        public static ActionCallPlan WithArgs(this ActionId id, string name0, double value0, string name1, double value1, string name2, double value2)
        {
            return ActionCallPlan.WithArgs(id, new Dictionary<string, ActionArgValue>
            {
                [name0] = ActionArgValue.OfConst(value0, name0),
                [name1] = ActionArgValue.OfConst(value1, name1),
                [name2] = ActionArgValue.OfConst(value2, name2)
            });
        }

        // ========== 调度模式工厂方法 ==========

        /// <summary>
        /// 创建立即执行的动作
        /// </summary>
        public static ActionCallPlan Immediate(this ActionId id)
        {
            var plan = new ActionCallPlan(id);
            return new ActionCallPlan(
                plan.Id, plan.Arity, plan.Arg0, plan.Arg1, plan.Args,
                Config.EActionScheduleMode.Immediate, 0, -1, true,
                Config.EActionExecutionPolicy.Immediate);
        }

        /// <summary>
        /// 创建延迟执行的动作
        /// </summary>
        /// <param name="delayMs">延迟时间（毫秒）</param>
        /// <param name="maxExecutions">最大执行次数，-1=无限</param>
        public static ActionCallPlan Delayed(this ActionId id, float delayMs, int maxExecutions = 1)
        {
            var plan = new ActionCallPlan(id);
            return new ActionCallPlan(
                plan.Id, plan.Arity, plan.Arg0, plan.Arg1, plan.Args,
                Config.EActionScheduleMode.Delayed, delayMs, maxExecutions, true,
                Config.EActionExecutionPolicy.Immediate);
        }

        /// <summary>
        /// 创建周期执行的动作
        /// </summary>
        /// <param name="intervalMs">周期间隔（毫秒）</param>
        /// <param name="maxExecutions">最大执行次数，-1=无限</param>
        /// <param name="canBeInterrupted">是否可中断</param>
        public static ActionCallPlan Periodic(this ActionId id, float intervalMs, int maxExecutions = -1, bool canBeInterrupted = true)
        {
            var plan = new ActionCallPlan(id);
            return new ActionCallPlan(
                plan.Id, plan.Arity, plan.Arg0, plan.Arg1, plan.Args,
                Config.EActionScheduleMode.Periodic, intervalMs, maxExecutions, canBeInterrupted,
                Config.EActionExecutionPolicy.Immediate);
        }

        /// <summary>
        /// 创建持续调度执行的动作（按间隔执行，直到外部中断或达到执行次数）
        /// </summary>
        /// <param name="canBeInterrupted">是否可中断</param>
        public static ActionCallPlan Continuous(this ActionId id, bool canBeInterrupted = true)
        {
            var plan = new ActionCallPlan(id);
            return new ActionCallPlan(
                plan.Id, plan.Arity, plan.Arg0, plan.Arg1, plan.Args,
                Config.EActionScheduleMode.Continuous, 0, -1, canBeInterrupted,
                Config.EActionExecutionPolicy.Immediate);
        }

        /// <summary>
        /// 创建带执行策略的动作
        /// </summary>
        public static ActionCallPlan WithExecutionPolicy(this ActionCallPlan plan, Config.EActionExecutionPolicy policy)
        {
            return new ActionCallPlan(
                plan.Id, plan.Arity, plan.Arg0, plan.Arg1, plan.Args,
                plan.ScheduleMode, plan.ScheduleParam, plan.MaxExecutions, plan.CanBeInterrupted,
                policy, plan.RetryMaxRetries, plan.RetryDelayMs);
        }

        /// <summary>
        /// 创建带重试策略的动作。
        /// </summary>
        public static ActionCallPlan WithRetry(this ActionCallPlan plan, int maxRetries = 3, float retryDelayMs = 0f)
        {
            return new ActionCallPlan(
                plan.Id, plan.Arity, plan.Arg0, plan.Arg1, plan.Args,
                plan.ScheduleMode, plan.ScheduleParam, plan.MaxExecutions, plan.CanBeInterrupted,
                Config.EActionExecutionPolicy.WithRetry, maxRetries, retryDelayMs);
        }

        /// <summary>
        /// 创建带调度参数的动作
        /// </summary>
        public static ActionCallPlan WithSchedule(this ActionCallPlan plan, Config.EActionScheduleMode mode, float param = 0, int maxExecutions = -1, bool canBeInterrupted = true)
        {
            return new ActionCallPlan(
                plan.Id, plan.Arity, plan.Arg0, plan.Arg1, plan.Args,
                mode, param, maxExecutions, canBeInterrupted,
                plan.ExecutionPolicy, plan.RetryMaxRetries, plan.RetryDelayMs);
        }
    }
}

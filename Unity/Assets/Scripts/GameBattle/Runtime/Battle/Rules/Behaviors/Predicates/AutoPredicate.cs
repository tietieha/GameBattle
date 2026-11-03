using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Runtime.Plan;
using AbilityKit.Triggering.Eventing;
using AbilityKit.Core.Logging;

namespace AbilityKit.Triggering.Runtime.Behavior.Predicates
{
    /// <summary>
    /// 自动 Predicate 的基类
    /// 用户只需继承此类并实现抽象方法，框架自动完成注册
    /// </summary>
    /// <example>
    /// // 用户只需要写这一个文件：
    /// public sealed class HasBuffPredicate : AutoPredicate
    /// {
    ///     public int BuffId { get; private set; }
    ///
    ///     protected override string PredicateType => "has_buff";
    ///     protected override int Order => 10;
    ///
    ///     public override void ParseFrom(Dictionary&lt;string, ActionArgValue&gt; namedArgs, ExecCtx ctx)
    ///     {
    ///         BuffId = AutoPredicateExtensions.ResolveInt(this, namedArgs, "buff_id", 0);
    ///     }
    ///
    ///     public override bool Evaluate(IBehaviorContext context)
    ///     {
    ///         // 业务逻辑
    ///     }
    /// }
    /// </example>
    public abstract class AutoPredicate : IConditionalBehavior
    {
        /// <summary>
        /// Predicate 类型标识（子类必须重写）
        /// </summary>
        protected abstract string PredicateType { get; }

        /// <summary>
        /// 注册顺序（小的先注册，默认 0）
        /// </summary>
        protected virtual int Order => 0;

        /// <summary>
        /// 从具名参数字典解析属性值（子类必须重写）
        /// </summary>
        public virtual void ParseFrom(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx) { }

        /// <summary>
        /// 评估条件（子类必须重写）
        /// </summary>
        /// <param name="context">行为上下文，包含 Args、Blackboards、Actions 等</param>
        public abstract bool Evaluate(IBehaviorContext context);

        bool IConditionalBehavior.Evaluate(IBehaviorContext context)
        {
            try
            {
                return Evaluate(context);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"[Predicate] {GetType().Name} evaluation failed");
                return false;
            }
        }
    }

    /// <summary>
    /// Predicate 注册接口（由代码生成器实现）
    /// </summary>
    public interface IAutoPredicateRegistration
    {
        void Register();
    }

    /// <summary>
    /// AutoPredicate 的扩展方法
    /// </summary>
    public static class AutoPredicateExtensions
    {
        /// <summary>
        /// 解析浮点数值
        /// </summary>
        public static float ResolveFloat(this AutoPredicate predicate, Dictionary<string, ActionArgValue> namedArgs, string key, float defaultValue = 0)
        {
            if (namedArgs == null || !namedArgs.TryGetValue(key, out var value))
                return defaultValue;

            if (value.Ref.Kind == ENumericValueRefKind.Const)
                return (float)value.Ref.ConstValue;

            return defaultValue;
        }

        /// <summary>
        /// 解析整数值
        /// </summary>
        public static int ResolveInt(this AutoPredicate predicate, Dictionary<string, ActionArgValue> namedArgs, string key, int defaultValue = 0)
        {
            if (namedArgs == null || !namedArgs.TryGetValue(key, out var value))
                return defaultValue;

            if (value.Ref.Kind == ENumericValueRefKind.Const)
                return (int)System.Math.Round(value.Ref.ConstValue);

            return defaultValue;
        }
    }
}

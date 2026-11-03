using System;
using System.Collections.Generic;
using AbilityKit.Ability.World.DI;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime;
using AbilityKit.Triggering.Eventing;
using AbilityKit.Core.Logging;

namespace AbilityKit.Triggering.Runtime.Plan
{
    /// <summary>
    /// AutoPlanAction 的注册接口
    /// 由代码生成器实现
    /// </summary>
    public interface IAutoPlanActionRegistration
    {
        void Register(ActionRegistry actions, IWorldResolver services);
    }

    /// <summary>
    /// 自动 Plan Action 的基类
    /// 用户只需继承此类并实现抽象方法，框架自动完成注册
    /// </summary>
    /// <example>
    /// // 用户只需要写这一个文件：
    /// public sealed class GiveDamageAction : AutoPlanAction
    /// {
    ///     public float DamageValue { get; private set; }
    ///     public DamageType DamageType { get; private set; }
    ///
    ///     protected override string ActionId => "give_damage";
    ///     protected override int Order => 10;
    ///
    ///     public override void ParseFrom(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
    ///     {
    ///         DamageValue = ResolveFloat(namedArgs, "damage_value", 0);
    ///         DamageType = (DamageType)ResolveInt(namedArgs, "damage_type", 0);
    ///     }
    ///
    ///     public override void Execute(object triggerArgs, ExecCtx<IWorldResolver> ctx)
    ///     {
    ///         // 业务逻辑
    ///     }
    /// }
    /// </example>
    public abstract class AutoPlanAction : IPlanActionModule
    {
        /// <summary>
        /// Action ID（子类必须重写）
        /// </summary>
        protected abstract string ActionId { get; }

        /// <summary>
        /// 注册顺序（小的先注册，默认 0）
        /// </summary>
        protected virtual int Order => 0;

        /// <summary>
        /// 从具名参数字典解析属性值（子类必须重写）
        /// </summary>
        public virtual void ParseFrom(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx) { }

        /// <summary>
        /// 执行 Action（子类必须重写）
        /// </summary>
        public abstract void Execute(object triggerArgs, ExecCtx<IWorldResolver> ctx);

        /// <summary>
        /// 验证参数（可选重写）
        /// </summary>
        public virtual bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
        {
            error = null;
            return true;
        }

        void IPlanActionModule.Register(ActionRegistry actions, IWorldResolver services)
        {
            if (this is IAutoPlanActionRegistration registration)
            {
                registration.Register(actions, services);
            }
            else
            {
                RegisterInternal(actions, services);
            }
        }

        private void RegisterInternal(ActionRegistry actions, IWorldResolver services)
        {
            if (actions == null || string.IsNullOrEmpty(ActionId))
                return;

            var actionIdValue = ActionId;
            var actionId = new ActionId(StableStringId.Get("action:" + actionIdValue));
            var schema = new AutoSchemaImpl(this, actionId);
            ActionSchemaRegistry.Register<object, IWorldResolver>(schema);

            actions.Register<NamedAction0<object, object, IWorldResolver>>(actionId, (triggerArgs, args, ctx) => ExecuteSafe(triggerArgs, null, ctx), isDeterministic: true);
            actions.Register<NamedAction1<object, object, IWorldResolver>>(actionId, (triggerArgs, args, ctx) => ExecuteSafe(triggerArgs, ToNamedArgs(args), ctx), isDeterministic: true);
            actions.Register<NamedAction2<object, object, IWorldResolver>>(actionId, (triggerArgs, args, ctx) => ExecuteSafe(triggerArgs, ToNamedArgs(args), ctx), isDeterministic: true);
        }

        private void ExecuteSafe(object triggerArgs, Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
        {
            if (ctx.Context == null)
            {
                Log.Warning($"[Plan] {GetType().Name} skipped. ctx.Context is null");
                return;
            }

            try
            {
                ParseFrom(namedArgs, ctx);
                Execute(triggerArgs, ctx);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"[Plan] {GetType().Name} execution failed");
            }
        }

        private static Dictionary<string, ActionArgValue> ToNamedArgs(object rawArgs)
        {
            if (rawArgs is NamedArgsDict dict) return dict.InnerDict;
            if (rawArgs is Dictionary<string, ActionArgValue> d) return d;
            return null;
        }

        private sealed class AutoSchemaImpl : IActionSchema<object, IWorldResolver>
        {
            private readonly AutoPlanAction _action;
            private readonly ActionId _actionId;

            public AutoSchemaImpl(AutoPlanAction action, ActionId actionId)
            {
                _action = action;
                _actionId = actionId;
            }

            public ActionId ActionId => _actionId;
            public Type ArgsType => _action.GetType();

            public object ParseArgs(Dictionary<string, ActionArgValue> namedArgs, ExecCtx<IWorldResolver> ctx)
            {
                var clone = (AutoPlanAction)Activator.CreateInstance(_action.GetType());
                clone.ParseFrom(namedArgs, ctx);
                return clone;
            }

            public bool TryValidateArgs(ReadOnlySpan<KeyValuePair<string, ActionArgValue>> args, out string error)
            {
                return _action.TryValidateArgs(args, out error);
            }
        }
    }

    /// <summary>
    /// AutoPlanAction 的扩展方法
    /// </summary>
    public static class AutoPlanActionExtensions
    {
        /// <summary>
        /// 解析浮点数值
        /// </summary>
        public static float ResolveFloat(this AutoPlanAction action, Dictionary<string, ActionArgValue> namedArgs, string key, float defaultValue = 0)
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
        public static int ResolveInt(this AutoPlanAction action, Dictionary<string, ActionArgValue> namedArgs, string key, int defaultValue = 0)
        {
            if (namedArgs == null || !namedArgs.TryGetValue(key, out var value))
                return defaultValue;

            if (value.Ref.Kind == ENumericValueRefKind.Const)
                return (int)System.Math.Round(value.Ref.ConstValue);

            return defaultValue;
        }
    }
}

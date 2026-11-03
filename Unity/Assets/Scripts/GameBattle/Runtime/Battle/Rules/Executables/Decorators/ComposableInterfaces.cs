using System;
using AbilityKit.Triggering.Runtime.Context;

namespace AbilityKit.Triggering.Runtime.Executable
{
    /// <summary>
    /// 可组合行为接口 - 所有修饰器的基础
    /// </summary>
    public interface IComposableExecutable : ISimpleExecutable, IHasInner
    {
        bool OnBeforeExecute(ActionContext ctx);
        void OnAfterExecute(ActionContext ctx, ref ExecutionResult result);
    }

    /// <summary>
    /// 可组合行为基类 - 提供默认实现
    /// </summary>
    public abstract class ComposableExecutableBase : IComposableExecutable
    {
        public abstract string Name { get; }
        public abstract ExecutableMetadata Metadata { get; }

        public ISimpleExecutable Inner { get; set; }

        public virtual ExecutionResult Execute(ActionContext ctx)
        {
            if (!OnBeforeExecute(ctx))
                return ExecutionResult.Skipped("Decorator condition not met");

            ExecutionResult result = Inner?.Execute(ctx) ?? ExecutionResult.Success();
            OnAfterExecute(ctx, ref result);
            return result;
        }

        public virtual bool OnBeforeExecute(ActionContext ctx) => true;
        public virtual void OnAfterExecute(ActionContext ctx, ref ExecutionResult result) { }
    }
}

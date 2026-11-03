using AbilityKit.Triggering.Registry;

namespace AbilityKit.Triggering.Runtime
{
    /// <summary>
    /// Action 解析器接口
    /// 负责从 ActionId 查找对应的 Delegate
    /// 从 IActionRegistry 拆分出来，消除循环依赖
    /// </summary>
    public interface IActionResolver
    {
        /// <summary>
        /// 尝试获取指定类型的 Action
        /// </summary>
        bool TryGet<TDelegate>(ActionId actionId, out TDelegate action, out bool isDeterministic)
            where TDelegate : System.Delegate;

        /// <summary>
        /// 尝试获取指定类型的 Action（通过 Type）
        /// </summary>
        bool TryGet(ActionId actionId, System.Type delegateType, out object action, out bool isDeterministic);

        /// <summary>
        /// 尝试获取指定类型的 Function
        /// </summary>
        bool TryGetFunction<TDelegate>(FunctionId functionId, out TDelegate function, out bool isDeterministic)
            where TDelegate : System.Delegate;

        /// <summary>
        /// 尝试获取指定类型的 Function（通过 Type）
        /// </summary>
        bool TryGetFunction(FunctionId functionId, System.Type delegateType, out object function, out bool isDeterministic);
    }
}

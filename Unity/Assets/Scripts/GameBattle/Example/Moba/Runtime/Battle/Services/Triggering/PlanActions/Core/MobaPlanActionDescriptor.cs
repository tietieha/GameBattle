using AbilityKit.Triggering.Runtime.Plan;

namespace AbilityKit.Demo.Moba.Services.Triggering.PlanActions
{
    /// <summary>
    /// 已注册 MOBA 计划动作模块的运行时描述信息。
    /// </summary>
    public readonly struct MobaPlanActionDescriptor
    {
        public MobaPlanActionDescriptor(int order, string moduleName, string actionName, IPlanActionModule module)
        {
            Order = order;
            ModuleName = moduleName;
            ActionName = actionName;
            Module = module;
        }

        public int Order { get; }
        public string ModuleName { get; }
        public string ActionName { get; }
        public IPlanActionModule Module { get; }
    }
}

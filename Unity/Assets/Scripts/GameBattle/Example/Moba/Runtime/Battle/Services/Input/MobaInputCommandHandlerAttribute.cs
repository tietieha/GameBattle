using System;
using AbilityKit.Core.Markers;

/// <summary>
/// 文件名称: MobaInputCommandHandlerAttribute.cs
///
/// 功能描述: 标记可自动注册的 MOBA 输入命令处理器。
///
/// 创建日期: 2026-05-27
/// 修改日期: 2026-05-27
/// </summary>
namespace AbilityKit.Demo.Moba.Services
{
    /// <summary>
    /// 输入命令处理器标记，新增输入类型时只需新增处理器类并标记 OpCode。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MobaInputCommandHandlerAttribute : MobaBattleRouteAttribute
    {
        /// <summary>
        /// 创建输入命令处理器标记。
        /// </summary>
        /// <param name="opCode">输入命令码</param>
        public MobaInputCommandHandlerAttribute(int opCode)
            : base(opCode, MobaBattleRouteKind.RuntimeInput)
        {
        }

        /// <summary>
        /// 扫描到处理器类型时注册到输入注册表或协议注册表。
        /// </summary>
        /// <param name="implType">处理器实现类型</param>
        /// <param name="registry">注册表</param>
        public override void OnScanned(Type implType, IMarkerRegistry registry)
        {
            base.OnScanned(implType, registry);

            MobaInputCommandHandlerRegistry inputRegistry = registry as MobaInputCommandHandlerRegistry;
            inputRegistry?.Register(OpCode, implType);
        }

        protected override MobaBattleRouteDescriptor CreateDescriptor(Type ownerType)
        {
            return new MobaBattleRouteDescriptor(
                OpCode,
                MobaBattleRouteKind.RuntimeInput,
                ownerType,
                payloadType: PayloadType,
                handlerType: ownerType,
                name: Name);
        }
    }
}
using System;
using AbilityKit.Core.Markers;

/// <summary>
/// 文件名称: MobaSnapshotEmitterAttribute.cs
/// 
/// 功能描述: 标记可自动注册的 MOBA 快照输出器。
/// 
/// 创建日期: 2026-05-27
/// 修改日期: 2026-05-27
/// </summary>
namespace AbilityKit.Demo.Moba.Services
{
    /// <summary>
    /// 快照输出器标记，新增快照类型时通过 Attribute 注册输出顺序。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MobaSnapshotEmitterAttribute : MarkerAttribute
    {
        /// <summary>
        /// 快照输出优先级，数值越小越先输出。
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// 创建快照输出器标记。
        /// </summary>
        /// <param name="priority">输出优先级</param>
        public MobaSnapshotEmitterAttribute(int priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// 扫描到输出器类型时注册到快照注册表。
        /// </summary>
        /// <param name="implType">输出器实现类型</param>
        /// <param name="registry">注册表</param>
        public override void OnScanned(Type implType, IMarkerRegistry registry)
        {
            MobaSnapshotEmitterRegistry snapshotRegistry = registry as MobaSnapshotEmitterRegistry;
            snapshotRegistry?.Register(Priority, implType);
        }
    }
}
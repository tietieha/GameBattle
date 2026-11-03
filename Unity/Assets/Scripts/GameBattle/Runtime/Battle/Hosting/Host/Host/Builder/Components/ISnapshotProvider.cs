using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host.Framework;

namespace AbilityKit.Ability.Host.Builder.Components
{
    /// <summary>
    /// 快照提供器接口
    /// 负责提供世界状态快照
    /// </summary>
    public interface ISnapshotProvider
    {
        /// <summary>
        /// 获取快照
        /// </summary>
        bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot);

        /// <summary>
        /// 注册到 Runtime
        /// </summary>
        void Register(IHostRuntimeFeatures features);
    }
}

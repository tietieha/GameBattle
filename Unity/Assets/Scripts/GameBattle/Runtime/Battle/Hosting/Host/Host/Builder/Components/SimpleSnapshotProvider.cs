using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host.Builder.Components;
using AbilityKit.Ability.Host.Framework;

namespace AbilityKit.Ability.Host.Builder.Components
{
    /// <summary>
    /// 简单快照提供器
    /// 包装 IWorldStateSnapshotProvider 以实现 ISnapshotProvider 接口
    /// </summary>
    public sealed class SimpleSnapshotProvider : ISnapshotProvider
    {
        private readonly IWorldStateSnapshotProvider _inner;

        public SimpleSnapshotProvider(IWorldStateSnapshotProvider inner)
        {
            _inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
        }

        public bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot)
        {
            if (_inner != null)
            {
                return _inner.TryGetSnapshot(frame, out snapshot);
            }

            snapshot = default;
            return false;
        }

        public void Register(IHostRuntimeFeatures features)
        {
            if (features != null)
            {
                features.RegisterFeature<ISnapshotProvider>(this);
            }
        }
    }
}

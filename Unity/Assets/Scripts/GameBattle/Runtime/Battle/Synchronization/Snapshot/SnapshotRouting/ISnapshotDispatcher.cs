using System;

namespace AbilityKit.Core.Snapshots.Routing
{
    /// <summary>
    /// 快照派发的最小接口，允许表现层运行时提供自己的 FrameSnapshotDispatcher 实现，
    /// 并订阅 BattleLogicSession。
    /// </summary>
    public interface ISnapshotDispatcher : ISnapshotDecoderRegistry
    {
        /// <summary>
        /// 收到快照信封时触发的事件。
        /// </summary>
        event Action<AbilityKit.Ability.Host.ISnapshotEnvelope, AbilityKit.Ability.Host.WorldStateSnapshot> SnapshotReceived;

        /// <summary>
        /// 订阅指定快照类型。
        /// </summary>
        IDisposable Subscribe<T>(int opCode, Action<AbilityKit.Ability.Host.ISnapshotEnvelope, T> handler);
    }
}

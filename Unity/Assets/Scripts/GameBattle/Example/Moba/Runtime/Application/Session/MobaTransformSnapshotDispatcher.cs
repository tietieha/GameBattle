using System;
using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;
using AbilityKit.Core.Pooling;
using AbilityKit.Ability.Host.Extensions.Moba.Runtime;
using AbilityKit.Ability.World.Abstractions;
using AbilityKit.Demo.Moba.Services;
using AbilityKit.Protocol.Moba.StateSync;

/// <summary>
/// 从世界快照服务读取并派发表现层位置快照。
/// </summary>
namespace AbilityKit.Demo.Moba.Session
{
    /// <summary>
    /// 位置快照派发器，隔离快照读取、类型判断和回调派发逻辑。
    /// </summary>
    public sealed class MobaTransformSnapshotDispatcher
    {
        private static readonly ObjectPool<List<WorldStateSnapshot>> s_snapshotListPool = Pools.GetPool(
            createFunc: () => new List<WorldStateSnapshot>(8),
            onRelease: list => list.Clear(),
            defaultCapacity: 8,
            maxSize: 64,
            collectionCheck: false);

        private readonly IWorld _world;

        /// <summary>
        /// 创建位置快照派发器。
        /// </summary>
        /// <param name="world">战斗逻辑世界</param>
        public MobaTransformSnapshotDispatcher(IWorld world)
        {
            _world = world;
        }

        /// <summary>
        /// 尝试读取当前帧的位置快照并派发给表现层。
        /// </summary>
        /// <param name="frame">当前逻辑帧。</param>
        /// <param name="callback">表现层快照回调。</param>
        public void TryDispatch(FrameIndex frame, Action<int, MobaActorTransformSnapshotEntry[]> callback)
        {
            if (_world?.Services?.TryResolve<IMobaBattleRuntimePort>(out var runtime) != true || runtime == null)
            {
                throw new InvalidOperationException("MobaTransformSnapshotDispatcher requires IMobaBattleRuntimePort.");
            }

            var snapshots = s_snapshotListPool.Get();
            try
            {
                var count = runtime.CollectSnapshots(frame, snapshots);
                if (count <= 0)
                {
                    MobaRuntimeLog.Warning(MobaRuntimeLogModule.Snapshot, MobaRuntimeLogPurpose.Validation, nameof(MobaTransformSnapshotDispatcher), $"No snapshots available. frame={frame.Value}");
                    return;
                }

                WorldStateSnapshot transformSnapshot = default;
                var found = false;
                for (int i = 0; i < snapshots.Count; i++)
                {
                    var snapshot = snapshots[i];
                    if (snapshot.OpCode != AbilityKit.Protocol.Moba.MobaOpCodes.Snapshot.ActorTransform) continue;

                    transformSnapshot = snapshot;
                    found = true;
                    break;
                }

                if (!found)
                {
                    MobaRuntimeLog.Warning(MobaRuntimeLogModule.Snapshot, MobaRuntimeLogPurpose.Validation, nameof(MobaTransformSnapshotDispatcher), $"ActorTransform snapshot not available. frame={frame.Value} snapshotCount={snapshots.Count}");
                    return;
                }

                MobaActorTransformSnapshotEntry[] entries = MobaActorTransformSnapshotCodec.Deserialize(transformSnapshot.Payload);
                callback?.Invoke(frame.Value, entries);
                MobaRuntimeLog.Trace(MobaRuntimeLogModule.Snapshot, MobaRuntimeLogPurpose.RuntimeTrace, nameof(MobaTransformSnapshotDispatcher), $"Transform snapshot: {entries?.Length ?? 0} entities");
            }
            finally
            {
                s_snapshotListPool.Release(snapshots);
            }
        }
    }
}
using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;
using AbilityKit.Ability.Host.Extensions.Moba.Runtime;

namespace AbilityKit.Demo.Moba.Services
{
    public interface IMobaBattleOutputPort
    {
        /// <summary>
        /// 兼容单快照读取入口；生产同步循环应优先使用 <see cref="CollectSnapshots"/> 以完整收集同帧多路输出。
        /// </summary>
        bool TryGetSnapshot(FrameIndex frame, out WorldStateSnapshot snapshot);

        /// <summary>
        /// 批量快照收集入口，供帧同步、ET 驱动和 View Adapter 高频路径复用外部缓冲区。
        /// </summary>
        int CollectSnapshots(FrameIndex frame, IList<WorldStateSnapshot> snapshots, int maxSnapshots = 32);
    }

    public interface IMobaBattleDiagnosticsStateReadModel
    {
        /// <summary>
        /// 兼容数组读取入口；高频诊断采样应优先使用 <see cref="FillDiagnosticEntityStates"/>。
        /// </summary>
        MobaDiagnosticEntityState[] GetDiagnosticEntityStates();

        /// <summary>
        /// 填充调用方提供的缓冲区，避免诊断状态采样产生数组分配。
        /// </summary>
        int FillDiagnosticEntityStates(IList<MobaDiagnosticEntityState> buffer);
    }

    public interface IMobaLogicWorldStateReadModel : IMobaBattleDiagnosticsStateReadModel
    {
        /// <summary>
        /// 兼容数组读取入口；高频状态采样应优先使用 <see cref="FillAllEntityStates"/>。
        /// </summary>
        LogicWorldEntityState[] GetAllEntityStates();

        /// <summary>
        /// 填充调用方提供的缓冲区，作为逻辑世界状态读取的生产推荐路径。
        /// </summary>
        int FillAllEntityStates(IList<LogicWorldEntityState> buffer);
    }
}

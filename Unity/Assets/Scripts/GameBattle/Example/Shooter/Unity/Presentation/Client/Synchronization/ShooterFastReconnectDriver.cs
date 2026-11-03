#nullable enable

using System.Collections.Generic;
using AbilityKit.Network.Runtime.Sync;

namespace AbilityKit.Demo.Shooter.View
{
    /// <summary>
    /// 包装玩法无关的框架 <see cref="FastReconnectSession"/>，让 Shooter 恢复层可以驱动它
    /// （审计 §10.4：FastReconnect 的第一个真实消费方）。
    ///
    /// Shooter 仍以自身的快照导入、重放和业务原因分类作为路由依据；该驱动会把每个
    /// <see cref="ShooterClientRecoveryState"/> 迁移映射到框架阶段机
    /// （Connected → Disconnected → {Resuming | AwaitingFullSnapshot} → Recovered），并采集会话发出的统一
    /// <see cref="SyncHealthEvent"/> 流，让框架负责阶段判定与健康遥测，而不需要重新实现恢复逻辑。
    ///
    /// 协调器会容忍会话严格的迁移守卫：任何非法步骤都会被跳过而不是抛出异常，使包装保持纯增量
    /// （设计 §6 回滚说明）。
    /// </summary>
    internal sealed class ShooterFastReconnectDriver
    {
        private readonly FastReconnectPhaseDriver _driver;

        public ShooterFastReconnectDriver(int resumeWindowFrames)
        {
            _driver = new FastReconnectPhaseDriver(resumeWindowFrames);
        }

        /// <summary>当前框架恢复阶段，也是 Shooter 恢复状态的投影目标。</summary>
        public FastReconnectPhase Phase => _driver.Phase;

        /// <summary>框架恢复窗口（帧数），用于区分短距离追帧与完整快照恢复。</summary>
        public int ResumeWindowFrames => _driver.ResumeWindowFrames;

        /// <summary>自上次 <see cref="ResetEventBuffer"/> 以来累计的健康事件。</summary>
        public IReadOnlyList<SyncHealthEvent> CollectedEvents => _driver.CollectedEvents;

        /// <summary>清空单次操作的健康事件缓冲；每个公共入口点调用。</summary>
        public void ResetEventBuffer()
        {
            _driver.ResetEventBuffer();
        }

        /// <summary>
        /// 记录一次常规权威心跳（收到干净快照且没有待处理恢复）。
        /// 发出框架 <see cref="SyncHealthEventKind.SnapshotReceived"/> 事件。
        /// </summary>
        public void Heartbeat(int authoritativeFrame)
        {
            _driver.Heartbeat(authoritativeFrame);
        }

        /// <summary>
        /// 将会话推进到与 Shooter 新恢复状态匹配的阶段，每次只执行一个合法框架迁移，并收集发出的健康事件。
        /// </summary>
        public void Reconcile(FastReconnectPhase target, int authoritativeFrame, int gapHint)
        {
            _driver.Reconcile(target, authoritativeFrame, gapHint);
        }
    }
}

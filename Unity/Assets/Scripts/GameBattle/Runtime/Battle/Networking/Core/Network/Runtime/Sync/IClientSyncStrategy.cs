#nullable enable

namespace AbilityKit.Network.Runtime.Sync
{
    /// <summary>
    /// 客户端同步模型的玩法无关契约（同步演示框架的“A 轴”）。一个策略封装一种让本地客户端与权威时间线保持一致的方式，
    /// 例如预测回滚，或纯远端 actor 权威插值，并且不依赖任何具体示例（Shooter、Moba 等）。
    ///
    /// 示例只需提供两个玩法特定类型，其余部分可原样复用策略：<typeparamref name="TInput"/> 是玩家逐 tick 产生的本地命令，
    /// <typeparamref name="TSample"/> 是策略播放的已解码远端快照样本。其他内容（tick 节奏、校正记账、恢复状态）都位于框架中，
    /// 因此一个同步模型只需实现一次，就可以在多个示例间共享。
    /// </summary>
    /// <typeparam name="TInput">每个 tick 提交的玩法特定本地输入命令。</typeparam>
    /// <typeparam name="TSample">从权威端观察到的已解码远端快照样本。</typeparam>
    public interface IClientSyncStrategy<TInput, TSample> : INetworkSyncController
        where TSample : IRemoteSnapshotSample
    {
        /// <summary>
        /// 按给定帧间隔推进策略，并根据模型应用预测、重放和/或插值，返回本 tick 的模拟结果。
        /// </summary>
        /// <param name="deltaSeconds">距离上一个 tick 的经过时间，单位为秒。</param>
        SyncTickResult Tick(float deltaSeconds);

        /// <summary>
        /// 记录本地玩家当前 tick 的输入。不消费本地输入的模型（例如纯权威插值）可以忽略它。
        /// </summary>
        /// <param name="input">本帧产生的本地命令。</param>
        void SubmitInput(in TInput input);

        /// <summary>
        /// 将来自权威端的已解码远端快照样本送入策略，使其可以向权威状态校正、缓冲或插值。
        /// </summary>
        /// <param name="sample">从权威端观察到的远端样本。</param>
        void ObserveRemote(in TSample sample);

        /// <summary>
        /// 返回最近一次校正/恢复诊断；如果模型尚未校正，则返回 <see cref="SyncReconciliationReport.None"/>。
        /// </summary>
        SyncReconciliationReport GetReconciliationReport();
    }
}

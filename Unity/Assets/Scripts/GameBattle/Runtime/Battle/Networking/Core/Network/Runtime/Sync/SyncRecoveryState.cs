#nullable enable

namespace AbilityKit.Network.Runtime.Sync
{
    /// <summary>
    /// 客户端同步策略当前所处的玩法无关恢复阶段。统一了示例过去通过自定义枚举表达的状态
    /// （例如 Shooter 的 ShooterClientRecoveryState）。
    /// </summary>
    public enum SyncRecoveryState
    {
        /// <summary>本地模拟/播放正常跟随服务器。</summary>
        Normal = 0,

        /// <summary>通过快进本地模拟追赶较小的帧差。</summary>
        CatchUp = 1,

        /// <summary>漂移超过增量恢复能力；正在等待完整权威快照。</summary>
        AwaitingFullSnapshot = 2,

        /// <summary>正在应用已收到的完整权威快照。</summary>
        ApplyingFullSnapshot = 3,

        /// <summary>恢复刚刚完成，本地状态已还原到权威状态。</summary>
        Recovered = 4
    }
}

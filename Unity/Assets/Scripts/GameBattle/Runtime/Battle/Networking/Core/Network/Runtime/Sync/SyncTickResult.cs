#nullable enable

namespace AbilityKit.Network.Runtime.Sync
{
    /// <summary>
    /// 客户端同步策略推进一次 update 后的玩法无关结果。
    /// 记录本地模拟/播放推进了多远，供演示框架驱动表现，并比较不同同步模型与网络 profile 下的行为。
    /// </summary>
    public readonly struct SyncTickResult
    {
        /// <summary>表示策略未推进的结果（例如尚未启动）。</summary>
        public static readonly SyncTickResult NotStarted = new SyncTickResult(ticks: 0, frame: 0, stateHash: 0u);

        public SyncTickResult(int ticks, int frame, uint stateHash)
        {
            Ticks = ticks;
            Frame = frame;
            StateHash = stateHash;
        }

        /// <summary>本次 update 中推进的固定模拟/播放 tick 数。</summary>
        public int Ticks { get; }

        /// <summary>策略将本地模拟或播放推进到的帧。</summary>
        public int Frame { get; }

        /// <summary>本次 update 后本地模拟的状态哈希（不适用时为 0）。</summary>
        public uint StateHash { get; }
    }
}

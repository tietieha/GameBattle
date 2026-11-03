namespace AbilityKit.Protocol.Moba.StateSync
{
    /// <summary>
    /// StateSync 相关的 OpCode 定义
    /// </summary>
    public static class OpCodes
    {
        /// <summary>
        /// 订阅状态同步 - 客户端请求通过 Gateway 订阅服务器战斗快照。
        /// </summary>
        public const uint SubscribeStateSync = 103;

        /// <summary>
        /// 快照推送 - 服务器向客户端推送世界状态快照
        /// </summary>
        public const uint SnapshotPushed = 9002;

        /// <summary>
        /// Delta 快照推送 - 增量状态更新
        /// </summary>
        public const uint DeltaSnapshotPushed = 9003;

        /// <summary>
        /// 状态哈希验证请求
        /// </summary>
        public const uint StateHashRequest = 9004;

        /// <summary>
        /// 状态哈希验证响应
        /// </summary>
        public const uint StateHashResponse = 9005;
    }
}

namespace AbilityKit.Ability.StateSync.Snapshot
{
    /// <summary>
    /// 可哈希状态接口
    /// 业务层实现此接口来提供自定义的哈希计算
    /// 用于客户端和服务器之间的状态一致性验证
    ///
    /// 使用方式：
    /// 1. 业务层实体实现 IHashableState 接口
    /// 2. StateHashComputer.ComputeWithBusinessData() 会调用所有注册的哈希提供者
    /// 3. 框架层哈希与业务层哈希合并，产生最终的 StateHash
    /// </summary>
    public interface IHashableState
    {
        /// <summary>
        /// 获取此实体的哈希值
        /// 建议使用稳定、确定性的哈希算法
        /// </summary>
        /// <returns>实体状态的哈希值（64位）</returns>
        ulong ComputeHash();
    }

    /// <summary>
    /// 业务数据哈希提供者接口
    /// 用于在 StateHashComputer 中注册业务层的哈希计算逻辑
    /// </summary>
    public interface IBusinessHashProvider
    {
        /// <summary>
        /// 获取所有业务实体的哈希
        /// </summary>
        /// <returns>所有实体哈希的组合值</returns>
        ulong GetAllBusinessEntityHashes();
    }
}

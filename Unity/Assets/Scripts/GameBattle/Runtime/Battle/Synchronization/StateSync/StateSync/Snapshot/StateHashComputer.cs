using System;
using System.Security.Cryptography;

namespace AbilityKit.Ability.StateSync.Snapshot
{
    /// <summary>
    /// 状态哈希计算器
    /// 计算世界状态的哈希值，用于客户端和服务器之间的状态一致性验证
    /// </summary>
    public static class StateHashComputer
    {
        private const uint HASH_SEED = 0x9E3779B9u;

        /// <summary>
        /// 计算快照的框架级哈希
        /// </summary>
        public static StateHash Compute(WorldStateSnapshot snapshot)
        {
            if (snapshot == null) return StateHash.Invalid;

            unchecked
            {
                ulong hash = (ulong)HASH_SEED;
                hash = HashCombine(hash, (long)snapshot.Version);
                hash = HashCombine(hash, (long)snapshot.Frame);
                hash = HashCombine(hash, snapshot.Timestamp);
                hash = HashCombine(hash, (long)snapshot.WorldFlags);

                return new StateHash(hash);
            }
        }

        /// <summary>
        /// 计算包含业务数据的完整哈希
        /// 框架级哈希与业务层提供的哈希合并
        /// </summary>
        /// <param name="snapshot">世界快照</param>
        /// <param name="businessHashProvider">业务层哈希提供者（可选）</param>
        /// <returns>包含业务数据的完整哈希</returns>
        public static StateHash ComputeWithBusinessData(
            WorldStateSnapshot snapshot,
            IBusinessHashProvider businessHashProvider = null)
        {
            if (snapshot == null) return StateHash.Invalid;

            unchecked
            {
                // 基础哈希
                ulong hash = (ulong)HASH_SEED;
                hash = HashCombine(hash, (long)snapshot.Version);
                hash = HashCombine(hash, (long)snapshot.Frame);
                hash = HashCombine(hash, snapshot.Timestamp);
                hash = HashCombine(hash, (long)snapshot.WorldFlags);

                // 合并业务层哈希
                if (businessHashProvider != null)
                {
                    ulong businessHash = businessHashProvider.GetAllBusinessEntityHashes();
                    hash = HashCombine(hash, (long)businessHash);
                }

                return new StateHash(hash);
            }
        }

        /// <summary>
        /// 验证两个快照的哈希是否匹配
        /// </summary>
        /// <param name="clientHash">客户端哈希</param>
        /// <param name="serverHash">服务器哈希</param>
        /// <returns>是否匹配</returns>
        public static bool ValidateHash(StateHash clientHash, StateHash serverHash)
        {
            return clientHash.Value == serverHash.Value;
        }

        private static ulong HashCombine(ulong hash, long value)
        {
            return hash ^ (ulong)value * 0x9E3779B97F4A7C15UL + (hash << 15) + (hash >> 2);
        }

        public static byte[] ComputeFingerprint(byte[] data)
        {
            if (data == null || data.Length == 0) return Array.Empty<byte>();

            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(data);
        }
    }
}

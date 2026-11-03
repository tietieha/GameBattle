using System;
using System.Collections.Generic;
using AbilityKit.Pipeline.Pooling;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线注册表拥有者查询结果租约。
    /// </summary>
    /// <remarks>
    /// 使用完成后必须释放租约，释放后不要继续持有或访问其中的列表引用。
    /// </remarks>
    public readonly struct PipelineRegistryOwnerListLease : IDisposable
    {
        private static readonly IReadOnlyList<IPipelineLifeOwner> EmptyOwners = Array.Empty<IPipelineLifeOwner>();
        private readonly List<IPipelineLifeOwner>? _owners;

        /// <summary>
        /// 使用已租借的结果列表创建租约。
        /// </summary>
        public PipelineRegistryOwnerListLease(List<IPipelineLifeOwner> owners)
        {
            _owners = owners;
        }

        /// <summary>
        /// 查询结果列表。
        /// </summary>
        public IReadOnlyList<IPipelineLifeOwner> Owners => _owners ?? EmptyOwners;

        /// <summary>
        /// 查询结果数量。
        /// </summary>
        public int Count => _owners?.Count ?? 0;

        /// <summary>
        /// 获取指定索引的生命周期拥有者。
        /// </summary>
        public IPipelineLifeOwner this[int index] => Owners[index];

        /// <summary>
        /// 释放租约并归还内部列表。
        /// </summary>
        public void Dispose()
        {
            if (_owners == null) return;
            PipelinePools.ReleaseLifeOwnerList(_owners);
        }
    }
}

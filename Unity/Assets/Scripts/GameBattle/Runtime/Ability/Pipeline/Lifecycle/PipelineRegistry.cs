using System.Collections.Generic;
using AbilityKit.Pipeline.Pooling;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 管线生命周期注册表运行时实现。
    /// 轻量级版本，仅提供基础注册和查询功能。
    /// </summary>
    public sealed class PipelineRegistry : IPipelineRegistry
    {
        /// <summary>
        /// 全局注册表实例。
        /// </summary>
        public static readonly PipelineRegistry Instance = new PipelineRegistry();

        private readonly List<IPipelineLifeOwner> _owners = new List<IPipelineLifeOwner>(64);
        private bool _isInitialized;

        /// <summary>
        /// 当前活跃管线数量。
        /// </summary>
        public int ActiveCount => _owners.Count;

        /// <summary>
        /// 初始化注册表。
        /// </summary>
        public void Initialize()
        {
            _isInitialized = true;
        }

        /// <summary>
        /// 关闭注册表并清空活跃实例。
        /// </summary>
        public void Shutdown()
        {
            _isInitialized = false;
            _owners.Clear();
        }

        /// <summary>
        /// 注册一个管线生命周期拥有者。
        /// </summary>
        public void Register(IPipelineLifeOwner owner)
        {
            if (!_isInitialized || owner == null) return;
            if (!_owners.Contains(owner))
            {
                _owners.Add(owner);
            }
            PipelineRegistryEvents.OnRunStarted?.Invoke(owner);
        }

        /// <summary>
        /// 注销一个管线生命周期拥有者。
        /// </summary>
        public void Unregister(IPipelineLifeOwner owner)
        {
            if (owner == null) return;
            if (_owners.Remove(owner))
            {
                PipelineRegistryEvents.OnRunEnded?.Invoke(owner, owner.State);
            }
        }

        /// <summary>
        /// 获取所有活跃管线生命周期拥有者。
        /// </summary>
        public IReadOnlyList<IPipelineLifeOwner> GetActiveOwners()
        {
            return _owners;
        }

        /// <summary>
        /// 中断所有活跃管线。
        /// </summary>
        public void InterruptAll()
        {
            PipelineRegistryEvents.OnGlobalInterrupt?.Invoke();
            for (int i = 0; i < _owners.Count; i++)
            {
                if (_owners[i] is IPipelineInterruptible interruptible)
                {
                    interruptible.Interrupt();
                }
            }
        }

        /// <summary>
        /// 按当前阶段 ID 查询活跃管线。
        /// </summary>
        public IReadOnlyList<IPipelineLifeOwner> GetOwnersByPhase(AbilityPipelinePhaseId phaseId)
        {
            var result = new List<IPipelineLifeOwner>();
            FillOwnersByPhase(phaseId, result);
            return result;
        }

        /// <summary>
        /// 将当前阶段匹配的活跃管线追加到结果列表。
        /// </summary>
        public int FillOwnersByPhase(AbilityPipelinePhaseId phaseId, IList<IPipelineLifeOwner> results)
        {
            if (results == null) return 0;

            var startCount = results.Count;
            for (int i = 0; i < _owners.Count; i++)
            {
                if (_owners[i].CurrentPhaseId == phaseId)
                {
                    results.Add(_owners[i]);
                }
            }

            return results.Count - startCount;
        }

        /// <summary>
        /// 从对象池租借列表并填充当前阶段匹配的活跃管线。
        /// </summary>
        public PipelineRegistryOwnerListLease RentOwnersByPhase(AbilityPipelinePhaseId phaseId)
        {
            var result = PipelinePools.RentLifeOwnerList();
            FillOwnersByPhase(phaseId, result);
            return new PipelineRegistryOwnerListLease(result);
        }

        /// <summary>
        /// 按运行状态查询活跃管线。
        /// </summary>
        public IReadOnlyList<IPipelineLifeOwner> GetOwnersByState(EAbilityPipelineState state)
        {
            var result = new List<IPipelineLifeOwner>();
            FillOwnersByState(state, result);
            return result;
        }

        /// <summary>
        /// 将运行状态匹配的活跃管线追加到结果列表。
        /// </summary>
        public int FillOwnersByState(EAbilityPipelineState state, IList<IPipelineLifeOwner> results)
        {
            if (results == null) return 0;

            var startCount = results.Count;
            for (int i = 0; i < _owners.Count; i++)
            {
                if (_owners[i].State == state)
                {
                    results.Add(_owners[i]);
                }
            }

            return results.Count - startCount;
        }

        /// <summary>
        /// 从对象池租借列表并填充运行状态匹配的活跃管线。
        /// </summary>
        public PipelineRegistryOwnerListLease RentOwnersByState(EAbilityPipelineState state)
        {
            var result = PipelinePools.RentLifeOwnerList();
            FillOwnersByState(state, result);
            return new PipelineRegistryOwnerListLease(result);
        }
    }
}
